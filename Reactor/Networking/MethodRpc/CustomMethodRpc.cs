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
        public static readonly List<CustomMethodRpc> AllCustomMethodRpcs = new();

        internal static bool SkipNextSend;
        
        public CustomMethodRpc(BasePlugin plugin, MethodInfo method, uint id, SendOption option, RpcLocalHandling localHandling) : base(plugin, id)
        {
            Method = method;
            LocalHandling = localHandling;
            SendOption = option;
            
            AllCustomMethodRpcs.Add(this);
        }

        public MethodInfo Method { get; }
        public override Type InnerNetObjectType { get; } = typeof(PlayerControl);
        public override RpcLocalHandling LocalHandling { get; }
        public override SendOption SendOption { get; }

        public override void UnsafeWrite(MessageWriter writer, object data)
        {
            var args = (object[]) data;
            RpcSerializer.SendMassage(writer,args);
        }

        public override object UnsafeRead(MessageReader reader)
        {
            var args = RpcSerializer.Deserialize(reader,Method.GetParameters().Select(x => x.ParameterType).ToArray());
            return args;
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object data)
        {
            SkipNextSend = true;
            
            var result = Method.Invoke(null,(object[]) data);
            if (Method.ReturnParameter.ParameterType == typeof(IEnumerator))
            {
                Coroutines.Start((IEnumerator) result);
            }
        }

        public void Send(object[] args)
        {
            UnsafeSend(PlayerControl.LocalPlayer,args);
        }
    }
}
