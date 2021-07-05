using System;
using System.Collections.Generic;
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
        public byte id;
        public SendOption option;
        public RpcLocalHandling localHandling;

        public MethodRpcAttribute( byte id, SendOption option = SendOption.Reliable, RpcLocalHandling localHandling = RpcLocalHandling.Before)
        {
            this.id = id;
            this.option = option;
            this.localHandling = localHandling;
        }
        
        public static void Register(Assembly assembly, BasePlugin plugin)
        {
            Logger<ReactorPlugin>.Info($"Registering Method Rpc for {assembly.GetName()}");
            Dictionary<int, MethodInfo> serializeMethods = new();

            MethodInfo serializeMethod;
            int index = 0;

            while ((serializeMethod = RpcSerializationHandels.GetHandleFor(index)) != null)
            {
                serializeMethods.Add(index, serializeMethod);
                Logger<ReactorPlugin>.Debug($"Registered Rpc Serialization Handle for {index} args");
                index++;
            }
            
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

                var seializer = serializeMethods[method.GetParameters().Length];
                if (seializer == null)
                {
                    Logger<ReactorPlugin>.Error($"Unsupported Args count, max is {RpcSerializationHandels.GetMaxArgs()}");
                    continue;
                }

                var attribute = method.GetCustomAttribute<MethodRpcAttribute>();
                if (attribute.localHandling == RpcLocalHandling.Before)
                {
                    PluginSingleton<ReactorPlugin>.Instance.Harmony.Patch(method, new HarmonyMethod(seializer));
                }
                else if (attribute.localHandling == RpcLocalHandling.After)
                {
                    PluginSingleton<ReactorPlugin>.Instance.Harmony.Patch(method, postfix: new HarmonyMethod(seializer));
                }
                var customRpc = new CustomMethodRpc(plugin, method, attribute.id, attribute.option, attribute.localHandling);
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

        [MethodRpc(123)]
        static void Hello()
        {
            
        }
    }
}
