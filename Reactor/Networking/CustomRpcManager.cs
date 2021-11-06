using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            if (customRpc.IsSingleton)
            {
                typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);
            }

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
        private static T? _instance;

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
