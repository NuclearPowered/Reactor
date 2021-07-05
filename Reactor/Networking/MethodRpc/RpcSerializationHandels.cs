using System.Linq;
using System.Reflection;
using HarmonyLib;
using Reactor.Networking;
using Reactor.Networking.MethodRpc;

namespace Reactor.Networking
{
    internal class RpcSerializationHandels
    {

        public static MethodInfo GetHandleFor(int count)
        {
            return typeof(RpcSerializationHandels).GetRuntimeMethod(nameof(Serialize),
                Enumerable.Repeat(typeof(MethodBase),1).Concat(Enumerable.Repeat(typeof(object), count)).ToArray());
        }

        public static int GetMaxArgs()
        {
            return typeof(RpcSerializationHandels).GetMethods().OrderBy(x => x.GetParameters().Length).Last().GetParameters().Length;
        }

        public static bool RpcPrefix(MethodBase originalMethod, params object[] list)
        {
            if (CustomMethodRpc.SkipNextSend)
            {
                CustomMethodRpc.SkipNextSend = false;
                return true;
            }
            
            CustomMethodRpc methodRpc =
                CustomMethodRpc.AllCustomMethodRpcs.FirstOrDefault(x => x.Method == originalMethod);
            typeof(CustomMethodRpc).GetMethod(nameof(CustomMethodRpc.Send))?.Invoke(methodRpc,new object[]{list});
            return false;
        }

        public static bool Serialize(MethodBase __originalMethod)
        {
            return RpcPrefix(__originalMethod);
        }

        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0)
        {
            return RpcPrefix(__originalMethod,arg0);
        }
        
        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0,[HarmonyArgument(1)] object arg1)
        {
            return RpcPrefix(__originalMethod,new object[]{arg0,arg1});
        }
        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0 , [HarmonyArgument(1)] object arg1, [HarmonyArgument(2)] object arg2)
        {
            return RpcPrefix(__originalMethod,arg0,arg1,arg2);
        }
        
        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0 , [HarmonyArgument(1)] object arg1, [HarmonyArgument(2)] object arg2,[HarmonyArgument(3)] object arg3)
        {
            return RpcPrefix(__originalMethod,arg0,arg1,arg2,arg3);
        }
        
        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0 , [HarmonyArgument(1)] object arg1, [HarmonyArgument(2)] object arg2,[HarmonyArgument(3)] object arg3,[HarmonyArgument(4)] object arg4)
        {
            return RpcPrefix(__originalMethod,arg0,arg1,arg2,arg3,arg4);
        }
        
        public static bool Serialize(MethodBase __originalMethod, [HarmonyArgument(0)] object arg0 , [HarmonyArgument(1)] object arg1, [HarmonyArgument(2)] object arg2,[HarmonyArgument(3)] object arg3,[HarmonyArgument(4)] object arg4,[HarmonyArgument(5)] object arg5)
        {
            return RpcPrefix(__originalMethod,arg0,arg1,arg2,arg3,arg4,arg5);
        }
        
        
    }
}
