using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Reactor.Networking.MethodRpc
{
    internal class RpcPrefixHandle
    {
        private static readonly MethodInfo _rpcPrefix = typeof(RpcPrefixHandle).GetMethod(nameof(RpcPrefix));
        private static readonly MethodInfo _emptyArray = typeof(Array).GetMethod(nameof(Array.Empty))?.MakeGenericMethod(typeof(object));
        
        public static bool RpcPrefix(MethodBase originalMethod, params object[] list)
        {
            if (CustomMethodRpc.SkipNextSend)
            {
                CustomMethodRpc.SkipNextSend = false;
                return true;
            }

            var methodRpc = CustomMethodRpc.allMethodRPCsFast[originalMethod];
            methodRpc?.Send(list);

            return false;
        }

        public static DynamicMethod GeneratePrefix(MethodBase __originalMethod)
        {
            Logger<ReactorPlugin>.Debug($"Generating Prefix for {__originalMethod.Name}");

            var originalPrm = __originalMethod.GetParameters();

            var prmType = originalPrm.Select(x => x.ParameterType).ToList();
            prmType.Insert(0, typeof(MethodBase));

            DynamicMethod m = new DynamicMethod($"{__originalMethod.Name}Prefix", MethodAttributes.Static,
                CallingConventions.Standard, typeof(bool), prmType.ToArray(), typeof(RpcPrefixHandle), false);

            m.DefineParameter(1, ParameterAttributes.None, "__originalMethod");
            for (var i = 2; i < originalPrm.Length + 2; i++) //0: Return Parameter 1:MethodBase Parameter
            {
                m.DefineParameter(i, ParameterAttributes.None, originalPrm[i - 2].Name);
            }

            #region il_code

            var il = m.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            if (prmType.Count == 1)
            {
                il.Emit(OpCodes.Call,_emptyArray);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, prmType.Count - 1);
                il.Emit(OpCodes.Newarr, typeof(object));
            }

            for (var i = 1; i < prmType.Count; i++) AppendArg(i, prmType[i]);

            void AppendArg(int index, Type type)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, index - 1);
                il.Emit(OpCodes.Ldarg_S, index);

                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Box, type);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Call, _rpcPrefix);
            il.Emit(OpCodes.Ret);

            #endregion

            return m;
        }


        public static Func<object[], object> GenerateCaller(MethodInfo method)
        {
            var methodPrm = method.GetParameters();

            var args = Expression.Parameter(typeof(object[]), "Args");

            List<Expression> elements = new();
            for (int i = 0; i < methodPrm.Length; i++)
            {
                var element = Expression.ArrayAccess(args, Expression.Constant(i, typeof(int)));
                var convert = Expression.Convert(element, methodPrm[i].ParameterType);
                elements.Add(convert);
            }


            Expression invoke = Expression.Call(method, elements);

            var returnTarget = Expression.Label(typeof(object));

            if (method.ReturnParameter.ParameterType == typeof(void))
            {
                invoke = Expression.Block(invoke,
                    Expression.Label(returnTarget, Expression.Default(typeof(object)))
                );
            }
            else
            {
                invoke = Expression.Label(returnTarget, invoke);
            }

            var caller = Expression.Lambda<Func<object[], object>>(invoke, args);
            Logger<ReactorPlugin>.Debug($"Generated caller for {method.Name} -> {caller}");
            return caller.Compile();
        }
    }
}
