using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;

namespace Reactor
{
    public abstract class UnsafeCustomRpc
    {
        public int? Id { get; internal set; }

        public BasePlugin UnsafePlugin { get; }
        public string PluginId { get; }

        public abstract Type InnerNetObjectType { get; }

        public abstract bool ShouldHandleLocally { get; }

        protected UnsafeCustomRpc(BasePlugin plugin)
        {
            UnsafePlugin = plugin;
            PluginId = MetadataHelper.GetMetadata(plugin).GUID;
        }

        public abstract void UnsafeWrite(MessageWriter writer, object data);
        public abstract object UnsafeRead(MessageReader reader);
        public abstract void UnsafeHandle(InnerNetObject innerNetObject, object data);
    }

    public abstract class CustomRpc<TPlugin, TInnerNetObject, TData> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject where TData : struct
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
            customRpc.Id = List.Count(x => x.UnsafePlugin == customRpc.UnsafePlugin);
            _list.Add(customRpc);
            _map[customRpc.InnerNetObjectType].Add(customRpc);

            return customRpc;
        }

        public void Send<TCustomRpc>(InnerNetObject netObject, object data, bool immediately = false)
        {
            UnsafeSend(netObject, List.Single(x => x.GetType() == typeof(TCustomRpc)), data, immediately);
        }

        public void UnsafeSend(InnerNetObject netObject, UnsafeCustomRpc customRpc, object data, bool immediately = false)
        {
            if (netObject == null) throw new ArgumentNullException(nameof(netObject));
            if (customRpc == null) throw new ArgumentNullException(nameof(customRpc));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (customRpc.Id == null)
            {
                throw new InvalidOperationException("Can't send unregistered CustomRpc");
            }

            if (customRpc.ShouldHandleLocally)
            {
                customRpc.UnsafeHandle(netObject, data);
            }

            var writer = immediately switch
            {
                false => AmongUsClient.Instance.StartRpc(netObject.NetId, CallId, SendOption.Reliable),
                true => AmongUsClient.Instance.StartRpcImmediately(netObject.NetId, CallId, SendOption.Reliable, -1)
            };

            writer.Write(customRpc.PluginId);
            writer.Write(customRpc.Id.Value);
            customRpc.UnsafeWrite(writer, data);

            if (immediately)
            {
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else
            {
                writer.EndMessage();
            }
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
                    var customRpcs = PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Map[__instance.GetType()];

                    var pluginId = reader.ReadString();
                    var id = reader.ReadInt32();

                    var customRpc = customRpcs.Single(x => x.PluginId == pluginId && x.Id == id);

                    customRpc.UnsafeHandle(__instance, customRpc.UnsafeRead(reader));

                    return false;
                }

                return true;
            }
        }
    }
}
