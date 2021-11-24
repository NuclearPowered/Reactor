using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Reactor.Networking.Serialization;

namespace Reactor.Networking.MethodRpc
{
    public class MethodRpc : UnsafeCustomRpc
    {
        public delegate object HandleDelegate(InnerNetObject innerNetObject, object[] args);

        public MethodRpc(BasePlugin plugin, MethodInfo method, uint id, SendOption option, RpcLocalHandling localHandling, bool sendImmediately) : base(plugin, id)
        {
            Method = method;
            LocalHandling = localHandling;
            SendOption = option;
            SendImmediately = sendImmediately;

            if (!method.IsStatic)
            {
                throw new NotImplementedException("Instance method rpc support is postponed until unhollower v0.5.0");
            }

            var parameters = method.GetParameters();

            if (method.IsStatic && parameters.Length == 0)
            {
                throw new ArgumentException("Static method rpc requires at least one argument", nameof(method));
            }

            var innerNetObjectType = parameters.First().ParameterType;

            if (!typeof(InnerNetObject).IsAssignableFrom(innerNetObjectType))
            {
                throw new ArgumentException("First argument of a static method rpc has to be an InnerNetObject", nameof(method));
            }

            InnerNetObjectType = innerNetObjectType;

            Handle = Hook(method, parameters);
        }

        public MethodInfo Method { get; }
        public HandleDelegate Handle { get; }

        protected internal override bool IsSingleton => false;

        public override Type InnerNetObjectType { get; }
        public override RpcLocalHandling LocalHandling { get; }
        public override SendOption SendOption { get; }
        public bool SendImmediately { get; }

        public override void UnsafeWrite(MessageWriter writer, object? data)
        {
            var args = (object[]) data!;
            MessageSerializer.Serialize(writer, args);
        }

        public override object UnsafeRead(MessageReader reader)
        {
            var args = MessageSerializer.Deserialize(reader, this);
            return args;
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
        {
            var args = (object[]) data!;
            var result = Handle(innerNetObject, args);

            if (result is IEnumerator enumerator)
            {
                Coroutines.Start(enumerator);
            }
        }

        public void Send(InnerNetObject innerNetObject, object[] args)
        {
            UnsafeSend(innerNetObject, args, SendImmediately);
        }

        private static readonly MethodInfo _sendMethod = AccessTools.Method(typeof(MethodRpc), nameof(Send));

        /// <summary>
        /// Hooks the <paramref name="method"/> rpc with a dynamic method that sends it
        /// </summary>
        private HandleDelegate Hook(MethodInfo method, ParameterInfo[] parameters)
        {
            var detour = new Detour(method, GenerateSender());

            // Used as target when hooking, sends the method rpc
            DynamicMethod GenerateSender()
            {
                var dynamicMethod = new DynamicMethod($"Sender<{method.GetID(simple: true)}>", method.ReturnType, parameters.Select(x => x.ParameterType).ToArray());

                foreach (var parameter in parameters)
                {
                    dynamicMethod.DefineParameter(parameter.Position, ParameterAttributes.None, parameter.Name);
                }

                var il = dynamicMethod.GetILGenerator();

                // :yeefuckinhaw:
                // Black magic from stackoverflow, they also said that you shouldn't do that ;)
                // Resort to Dictionary<MethodInfo, MethodRpc> if this turns out to be unreliable
                {
                    var handle = GCHandle.Alloc(this);
                    var ptr = GCHandle.ToIntPtr(handle);

                    if (IntPtr.Size == 4)
                    {
                        il.Emit(OpCodes.Ldc_I4, ptr.ToInt32());
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I8, ptr.ToInt64());
                    }

                    il.Emit(OpCodes.Conv_I);

                    // TODO Figure out why this always throws NullReferenceException on mono
                    // il.Emit(OpCodes.Ldobj, typeof(MethodRpc));

                    // Workaround for ^, speed seems to be the same
                    il.Emit(OpCodes.Call, AccessTools.Method(typeof(GCHandle), nameof(GCHandle.FromIntPtr)));
                    var temp = il.DeclareLocal(typeof(GCHandle));
                    il.Emit(OpCodes.Stloc, temp);
                    il.Emit(OpCodes.Ldloca, temp);
                    il.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(GCHandle), nameof(GCHandle.Target)));
                }

                il.Emit(OpCodes.Ldarg_0);

                il.Emit(OpCodes.Ldc_I4, parameters.Length - 1);
                il.Emit(OpCodes.Newarr, typeof(object));

                for (var i = 1; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i - 1);
                    il.Emit(OpCodes.Ldarg, i);

                    var parameterType = parameter.ParameterType;
                    if (parameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, parameterType);
                    }

                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Call, _sendMethod);

                if (method.ReturnType != typeof(void))
                {
                    il.Emit(OpCodes.Ldnull);
                }

                il.Emit(OpCodes.Ret);

                return dynamicMethod;
            }

            // Proxy translating object array to trampoline method args, used by MethodRpc to invoke original handling
            HandleDelegate GenerateHandler(DynamicMethod trampoline)
            {
                var dynamicMethod = new DynamicMethod($"Handler<{method.GetID(simple: true)}>", typeof(object), new[] { typeof(InnerNetObject), typeof(object[]) });
                dynamicMethod.DefineParameter(0, ParameterAttributes.None, "innerNetObject");
                dynamicMethod.DefineParameter(1, ParameterAttributes.None, "args");

                var il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, InnerNetObjectType);

                for (var i = 1; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4, i - 1);
                    il.Emit(OpCodes.Ldelem_Ref);

                    var parameterType = parameter.ParameterType;
                    il.Emit(parameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
                }

                il.Emit(OpCodes.Call, trampoline);

                if (method.ReturnType == typeof(void))
                {
                    il.Emit(OpCodes.Ldnull);
                }

                il.Emit(OpCodes.Ret);

                return dynamicMethod.CreateDelegate<HandleDelegate>();
            }

            return GenerateHandler((DynamicMethod) detour.GenerateTrampoline());
        }
    }
}
