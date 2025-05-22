using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Reactor.Networking.Serialization;
using Reactor.Utilities;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Provides a custom rpc for method rpc.
/// </summary>
public class MethodRpc : UnsafeCustomRpc
{
    private delegate object HandleDelegate(InnerNetObject innerNetObject, object[] args);

    private readonly HandleDelegate _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodRpc"/> class.
    /// </summary>
    /// <param name="plugin">The plugin that the rpc is attached to.</param>
    /// <param name="method">The method of the method rpc.</param>
    /// <param name="id">The id of the rpc.</param>
    /// <param name="option">The send option of the rpc.</param>
    /// <param name="localHandling">The local handling method of the rpc.</param>
    /// <param name="sendImmediately">The value indicating whether the rpc should be sent immediately.</param>
    public MethodRpc(BasePlugin plugin, MethodInfo method, uint id, SendOption option, RpcLocalHandling localHandling, bool sendImmediately) : base(plugin, id)
    {
        Method = method;
        LocalHandling = localHandling;
        SendOption = option;
        SendImmediately = sendImmediately;

        var parameters = method.GetParameters();

        if (method.IsStatic)
        {
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
        }
        else
        {
            if (!typeof(InnerNetObject).IsAssignableFrom(method.DeclaringType))
            {
                throw new ArgumentException("Declaring type of an instance method rpc has to be an InnerNetObject", nameof(method));
            }

            InnerNetObjectType = method.DeclaringType;
        }

        _handle = Hook(method, parameters, method.IsStatic);
    }

    /// <summary>
    /// Gets the method of the method rpc.
    /// </summary>
    public MethodInfo Method { get; }

    /// <inheritdoc />
    protected internal override bool IsSingleton => false;

    /// <inheritdoc />
    public override Type InnerNetObjectType { get; }

    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling { get; }

    /// <inheritdoc />
    public override SendOption SendOption { get; }

    /// <summary>
    /// Gets a value indicating whether the method rpc should be sent immediately.
    /// </summary>
    [Obsolete("Non-immediate RPCs were removed in 2025.5.20. All RPCs are immediate. This property will be removed in a future version.")]
    public bool SendImmediately { get; }

    /// <inheritdoc />
    public override void UnsafeWrite(MessageWriter writer, object? data)
    {
        var args = (object[]) data!;
        MessageSerializer.Serialize(writer, args);
    }

    /// <inheritdoc />
    public override object UnsafeRead(MessageReader reader)
    {
        var parameters = Method.GetParameters();
        var args = new object[parameters.Length - 1];

        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            args[i - 1] = reader.Deserialize(parameter.ParameterType);
        }

        return args;
    }

    /// <inheritdoc />
    public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
    {
        var args = (object[]) data!;
        var result = _handle(innerNetObject, args);

        if (result is IEnumerator enumerator)
        {
            Coroutines.Start(enumerator);
        }
    }

    /// <summary>
    /// Sends this method rpc on the specified <paramref name="innerNetObject"/> with the specified <paramref name="args"/>.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="args">The arguments to serialize and send.</param>
    public void Send(InnerNetObject innerNetObject, object[] args)
    {
        UnsafeSend(innerNetObject, args, SendImmediately);
    }

    private static readonly MethodInfo _sendMethod = AccessTools.Method(typeof(MethodRpc), nameof(Send));

    /// <summary>
    /// Hooks the <paramref name="method"/> rpc with a dynamic method that sends it.
    /// </summary>
    private HandleDelegate Hook(MethodInfo method, ParameterInfo[] parameters, bool isStatic)
    {
        var detour = new Detour(method, GenerateSender());

        // Used as target when hooking, sends the method rpc
        DynamicMethod GenerateSender()
        {
            var parameterTypes = parameters.Select(x => x.ParameterType);
            if (!isStatic) parameterTypes = parameterTypes.Prepend(InnerNetObjectType);

            var dynamicMethod = new DynamicMethod($"Sender<{method.GetID(simple: true)}>", method.ReturnType, parameterTypes.ToArray());

            if (!isStatic) dynamicMethod.DefineParameter(0, ParameterAttributes.None, "this");

            foreach (var parameter in parameters)
            {
                dynamicMethod.DefineParameter(parameter.Position + (isStatic ? 0 : 1), ParameterAttributes.None, parameter.Name);
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

            var offset = isStatic ? 1 : 0;

            il.Emit(OpCodes.Ldc_I4, parameters.Length - offset);
            il.Emit(OpCodes.Newarr, typeof(object));

            for (var i = offset; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i - offset);
                il.Emit(OpCodes.Ldarg, i + (isStatic ? 0 : 1));

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

            var offset = isStatic ? 1 : 0;

            for (var i = offset; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i - offset);
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
