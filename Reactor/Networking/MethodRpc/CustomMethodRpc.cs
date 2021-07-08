using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using Hazel;
using InnerNet;

namespace Reactor.Networking.MethodRpc
{
    public class CustomMethodRpc : UnsafeCustomRpc
    {
        internal static bool SkipNextSend;
        internal static Dictionary<MethodBase, CustomMethodRpc> allMethodRPCsFast = new();

        private Func<object[], object> Invoker;
        public Type[] Parameters { get; }
        public Dictionary<Type, FieldInfo[]> StructBook { get; } = new();

        public CustomMethodRpc(BasePlugin plugin, MethodInfo method, uint id, SendOption option,
            RpcLocalHandling localHandling) : base(plugin, id)
        {
            Method = method;
            LocalHandling = localHandling;
            SendOption = option;

            var methodParameters = method.GetParameters();

            List<Type> allArgs = new List<Type>();
            foreach (var arg in methodParameters)
            {
                var argType = arg.ParameterType;
                if (argType.IsValueType && !argType.IsPrimitive && !StructBook.ContainsKey(arg.ParameterType))
                {
                    var feilds = argType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .ToArray();
                    StructBook.Add(argType, feilds);
                }

                allArgs.Add(argType);

            }

            Parameters = allArgs.ToArray();

            Invoker = RpcPrefixHandle.GenerateCaller(Method);

            allMethodRPCsFast.Add(Method, this);

        }

        public MethodInfo Method { get; }
        public override Type InnerNetObjectType { get; } = typeof(PlayerControl);
        public override RpcLocalHandling LocalHandling { get; } = RpcLocalHandling.None;
        public override SendOption SendOption { get; }

        public override void UnsafeWrite(MessageWriter writer, object data)
        {
            var args = (object[]) data;
            RpcSerializer.Serialize(writer, this, args);
        }

        public override object UnsafeRead(MessageReader reader)
        {
            var args = RpcSerializer.Deserialize(reader, this);
            return args;
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object data)
        {
            SkipNextSend = true;

            var result = Invoker((object[]) data);
            if (result != null)
            {
                Coroutines.Start((IEnumerator) result);
            }
        }

        public void Send(object[] args)
        {
            UnsafeSend(PlayerControl.LocalPlayer, args);
        }
    }
}
