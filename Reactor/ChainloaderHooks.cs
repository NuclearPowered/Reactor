using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace Reactor
{
    public static class ChainloaderHooks
    {
        public delegate void PluginLoadHandler(BasePlugin plugin);

        public static event PluginLoadHandler PluginLoad;
        public static event PluginLoadHandler PluginLoaded;

        internal static void OnPluginLoad(BasePlugin plugin)
        {
            PluginLoad?.Invoke(plugin);
        }
        
        internal static void AfterPluginLoad(BasePlugin plugin)
        {
            PluginLoaded?.Invoke(plugin);
        }

        [HarmonyPatch(typeof(IL2CPPChainloader), nameof(IL2CPPChainloader.LoadPlugin))]
        private static class LoadPluginPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(BasePlugin), nameof(BasePlugin.Load))))
                    .Insert(
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ChainloaderHooks), nameof(OnPluginLoad)))
                    )
                    .InstructionEnumeration();
            }

            public static void Postfix(BasePlugin __result)
            {
                AfterPluginLoad(__result);
            }
        }
    }
}
