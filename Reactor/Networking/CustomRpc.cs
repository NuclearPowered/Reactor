using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace Reactor.Networking
{
    public enum RpcLocalHandling
    {
        None,
        Before,
        After
    }

    public abstract class UnsafeCustomRpc
    {
        internal CustomRpcManager Manager { get; set; }

        public uint Id { get; }
        public BasePlugin UnsafePlugin { get; }
        public string PluginId { get; }

        public abstract Type InnerNetObjectType { get; }

        public virtual SendOption SendOption { get; } = SendOption.Reliable;
        public abstract RpcLocalHandling LocalHandling { get; }

        protected UnsafeCustomRpc(BasePlugin plugin, uint id)
        {
            UnsafePlugin = plugin;
            PluginId = MetadataHelper.GetMetadata(plugin).GUID;
            Id = id;
        }

        public abstract void UnsafeWrite(MessageWriter writer, object data);
        public abstract object UnsafeRead(MessageReader reader);
        public abstract void UnsafeHandle(InnerNetObject innerNetObject, object data);

        public void UnsafeSend(InnerNetObject netObject, object data, bool immediately = false, int targetClientId = -1)
        {
            if (netObject == null) throw new ArgumentNullException(nameof(netObject));

            if (Manager == null)
            {
                throw new InvalidOperationException("Can't send unregistered CustomRpc");
            }

            if (LocalHandling == RpcLocalHandling.Before)
            {
                UnsafeHandle(netObject, data);
            }

            var writer = immediately switch
            {
                false => AmongUsClient.Instance.StartRpc(netObject.NetId, CustomRpcManager.CallId, SendOption),
                true => AmongUsClient.Instance.StartRpcImmediately(netObject.NetId, CustomRpcManager.CallId, SendOption, targetClientId)
            };

            var pluginNetId = ModList.GetById(PluginId).NetId;
            writer.WritePacked(pluginNetId);
            writer.WritePacked(Id);

            writer.StartMessage(0);
            UnsafeWrite(writer, data);
            writer.EndMessage();

            if (immediately)
            {
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else
            {
                writer.EndMessage();
            }

            if (LocalHandling == RpcLocalHandling.After)
            {
                UnsafeHandle(netObject, data);
            }
        }
    }

    public abstract class CustomRpc<TPlugin, TInnerNetObject, TData> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject
    {
        protected CustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public TPlugin Plugin => (TPlugin) UnsafePlugin;
        public override Type InnerNetObjectType => typeof(TInnerNetObject);

        public abstract void Write(MessageWriter writer, TData data);
        public abstract TData Read(MessageReader reader);
        public abstract void Handle(TInnerNetObject innerNetObject, TData data);

        public override void UnsafeWrite(MessageWriter writer, object data)
        {
            Write(writer, (TData) data);
        }

        public override object UnsafeRead(MessageReader reader)
        {
            return Read(reader);
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object data)
        {
            Handle((TInnerNetObject) innerNetObject, (TData) data);
        }

        public void Send(InnerNetObject netObject, TData data, bool immediately = false)
        {
            UnsafeSend(netObject, data, immediately);
        }

        public void SendTo(InnerNetObject netObject, int targetId, TData data)
        {
            UnsafeSend(netObject, data, true, targetId);
        }
    }

    public abstract class PlayerCustomRpc<TPlugin, TData> : CustomRpc<TPlugin, PlayerControl, TData> where TPlugin : BasePlugin
    {
        protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public void Send(TData data, bool immediately = false)
        {
            Send(PlayerControl.LocalPlayer, data, immediately);
        }

        public void SendTo(int targetId, TData data)
        {
            SendTo(PlayerControl.LocalPlayer, targetId, data);
        }
    }

    public class CustomRpcManager
    {
        public const byte CallId = byte.MaxValue;

        private readonly List<UnsafeCustomRpc> _list = new List<UnsafeCustomRpc>();
        private readonly Dictionary<Type, List<UnsafeCustomRpc>> _map = new Dictionary<Type, List<UnsafeCustomRpc>>();

        public IReadOnlyList<UnsafeCustomRpc> List => _list.AsReadOnly();

        public ILookup<Type, UnsafeCustomRpc> Map =>
            _map.SelectMany(pair => pair.Value, (pair, value) => new { pair.Key, Value = value })
                .ToLookup(pair => pair.Key, pair => pair.Value);

        public CustomRpcManager()
        {
            foreach (var type in HandleRpcPatch.InnerNetObjectTypes)
            {
                _map[type] = new List<UnsafeCustomRpc>();
            }
        }

        public UnsafeCustomRpc Register(UnsafeCustomRpc customRpc)
        {
            if (_list.Any(x => x.UnsafePlugin == customRpc.UnsafePlugin && x.Id == customRpc.Id))
            {
                throw new ArgumentException("Rpc with that id was already registered");
            }

            customRpc.Manager = this;
            _list.Add(customRpc);
            _map[customRpc.InnerNetObjectType].Add(customRpc);

            typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);

            return customRpc;
        }

        [HarmonyPatch]
        private static class HandleRpcPatch
        {
            internal static List<Type> InnerNetObjectTypes { get; } = typeof(InnerNetObject).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(InnerNetObject)) && x != typeof(LobbyBehaviour)).ToList();

            public static IEnumerable<MethodBase> TargetMethods()
            {
                return InnerNetObjectTypes.Select(x => x.GetMethod(nameof(InnerNetObject.HandleRpc), AccessTools.all));
            }

            public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if (callId == CallId)
                {
                    var manager = PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager;
                    var customRpcs = manager.Map[__instance.GetType()];

                    var pluginNetId = reader.ReadPackedUInt32();
                    var id = reader.ReadPackedUInt32();

                    var pluginId = ModList.GetByNetId(pluginNetId).Id;
                    var customRpc = customRpcs.Single(x => x.PluginId == pluginId && x.Id == id);

                    customRpc.UnsafeHandle(__instance, customRpc.UnsafeRead(reader.ReadMessage()));

                    return false;
                }

                return true;
            }
        }
    }

    public static class Rpc<T> where T : UnsafeCustomRpc
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception($"{typeof(T).FullName} isn't registered");
                }

                return _instance;
            }

            internal set
            {
                if (_instance != null)
                {
                    throw new Exception($"{typeof(T).FullName} is already registered");
                }

                _instance = value;
            }
        }
    }
}
