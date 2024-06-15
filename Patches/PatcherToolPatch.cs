using HarmonyLib;

namespace HazardControl.Patches
{
    [HarmonyPatch(typeof(PatcherTool))]
    internal class PatcherToolPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(ref PatcherTool __instance)
        {
            __instance.anomalyMask |= 1 << 21; // Layer 21 = Map Hazard
        }
    }
}
