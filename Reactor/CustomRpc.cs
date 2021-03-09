using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;
using Reactor.Net;

namespace Reactor
{
    public enum RpcLocalHandling
    {
        None,
        Before,
        After
    }

    public abstract class UnsafeCustomRpc
    {
        public int? Id { get; internal set; }

        internal CustomRpcManager Manager { get; set; }
        public BasePlugin UnsafePlugin { get; }
        public string PluginId { get; }

        public abstract Type InnerNetObjectType { get; }

        public virtual SendOption SendOption { get; } = SendOption.Reliable;
        public abstract RpcLocalHandling LocalHandling { get; }

        protected UnsafeCustomRpc(BasePlugin plugin)
        {
            UnsafePlugin = plugin;
            PluginId = MetadataHelper.GetMetadata(plugin).GUID;
        }

        public abstract void UnsafeWrite(MessageWriter writer, object data);
        public abstract object UnsafeRead(MessageReader reader);
        public abstract void UnsafeHandle(InnerNetObject innerNetObject, object data);

        public void UnsafeSend(InnerNetObject netObject, object data, bool immediately = false, int targetClientId = -1)
        {
            if (netObject == null) throw new ArgumentNullException(nameof(netObject));

            if (Id == null)
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

            if (Manager.PluginIdMap.TryGetValue(PluginId, out var pluginId))
            {
                writer.WritePacked(pluginId);
            }
            else
            {
                writer.Write(PluginId);
            }

            writer.WritePacked(Id!.Value);
            UnsafeWrite(writer, data);

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
        protected CustomRpc(TPlugin plugin) : base(plugin)
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
        protected PlayerCustomRpc(TPlugin plugin) : base(plugin)
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

        public Dictionary<string, int> PluginIdMap { get; private set; }
        public Dictionary<int, string> PluginIdMapReversed { get; private set; }

        public CustomRpcManager()
        {
            foreach (var type in HandleRpcPatch.InnerNetObjectTypes)
            {
                _map[type] = new List<UnsafeCustomRpc>();
            }
        }

        public void ReloadPluginIdMap()
        {
            PluginIdMap = new Dictionary<string, int>();
            PluginIdMapReversed = new Dictionary<int, string>();

            var i = -1;

            foreach (var mod in ModList.GetCurrent().OrderBy(x => x.Id))
            {
                if (mod.Side == PluginSide.Both)
                {
                    PluginIdMap[mod.Id] = i;
                    PluginIdMapReversed[i] = mod.Id;
                    i--;
                }
            }
        }

        public UnsafeCustomRpc Register(UnsafeCustomRpc customRpc)
        {
            customRpc.Manager = this;
            customRpc.Id = List.Count(x => x.UnsafePlugin == customRpc.UnsafePlugin);
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

            private static string ReadString(MessageReader reader, int len)
            {
                if (reader.BytesRemaining < len) throw new InvalidDataException($"Read length is longer than message length: {len} of {reader.BytesRemaining}");

                var output = Encoding.UTF8.GetString(reader.Buffer, reader.readHead, len);
                reader.Position += len;
                return output;
            }

            public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if (callId == CallId)
                {
                    var manager = PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager;
                    var customRpcs = manager.Map[__instance.GetType()];

                    var lengthOrShortId = reader.ReadPackedInt32();

                    var pluginId = lengthOrShortId < 0
                        ? manager.PluginIdMapReversed[lengthOrShortId]
                        : ReadString(reader, lengthOrShortId);

                    var id = reader.ReadPackedInt32();

                    var customRpc = customRpcs.Single(x => x.PluginId == pluginId && x.Id == id);

                    customRpc.UnsafeHandle(__instance, customRpc.UnsafeRead(reader));

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
