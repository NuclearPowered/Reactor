using System;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;

namespace Reactor.Networking.MethodRpc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodRpcAttribute : Attribute
    {
        private static readonly MethodInfo _rpcPrefixGenerator = typeof(RpcPrefixHandle).GetMethod(
            nameof(RpcPrefixHandle.GeneratePrefix),
            BindingFlags.Static | BindingFlags.Public);
        
        public uint id;
        public SendOption option;
        public RpcLocalHandling localHandling;

        public MethodRpcAttribute( uint id, SendOption option = SendOption.Reliable, RpcLocalHandling localHandling = RpcLocalHandling.Before)
        {
            this.id = id;
            this.option = option;
            this.localHandling = localHandling;
        }

        public static void Register(Assembly assembly, BasePlugin plugin)
        {
            Logger<ReactorPlugin>.Info($"Registering Method Rpc for {assembly.GetName()}");

            var rpcMethods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Where(m => m.GetCustomAttributes(typeof(MethodRpcAttribute), false).Length > 0)
                .ToArray();


            foreach (var method in rpcMethods)
            {
                
                if (!method.IsStatic)
                {
                    Logger<ReactorPlugin>.Warning("Cannot register non static custom method rpc");
                    continue;
                }

                var attribute = method.GetCustomAttribute<MethodRpcAttribute>();
                
                var customRpc = new CustomMethodRpc(plugin, method, attribute.id, attribute.option,
                    attribute.localHandling);
                PluginSingleton<ReactorPlugin>.Instance.Harmony.Patch(method, new HarmonyMethod(_rpcPrefixGenerator));
                try
                {
                    PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(customRpc);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        

        internal static void Initialize()
        {
            ChainloaderHooks.PluginLoad += plugin => Register(plugin.GetType().Assembly, plugin);
        }
    }
}
