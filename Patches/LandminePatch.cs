using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace HazardControl.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        private static bool PreOnTriggerEnter(ref Landmine __instance)
        {
            // [Game bugfix] Disable mine trip sound when it's disarmed
            return __instance.mineActivated;
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPostfix]
        private static void OnTriggerEnter(ref Landmine __instance, Collider other)
        {
            // Already handled cases by original function
            if (__instance.hasExploded || __instance.pressMineDebounceTimer > 0.0 ||
                other.CompareTag("Player") || other.CompareTag("PhysicsProp") || other.tag.StartsWith("PlayerRagdoll") ||
                !NetworkManager.Singleton.IsServer || !Plugin.GameConfig.EnemiesTriggerMines.Value
            )
                return;

            // Layer 19 = Enemies
            if (other.gameObject.layer != 19)
                return;
            
            // Mine pressed by enemy
            __instance.pressMineDebounceTimer = 0.5f;
            __instance.PressMineServerRpc();
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPostfix]
        private static void OnTriggerExit(ref Landmine __instance, Collider other)
        {
            // Already handled cases by original function
            if (__instance.hasExploded || !__instance.mineActivated || 
                other.CompareTag("Player") || other.CompareTag("PhysicsProp") || other.tag.StartsWith("PlayerRagdoll") ||
                !NetworkManager.Singleton.IsServer || !Plugin.GameConfig.EnemiesTriggerMines.Value
            )
                return;

            // Layer 19 = Enemies
            if (other.gameObject.layer == 19)
            {
                // Trigger explosion
                __instance.TriggerMineOnLocalClientByExiting();
            }
        }

        [HarmonyPatch("ToggleMineEnabledLocalClient")]
        [HarmonyPostfix]
        private static void ToggleMineEnabledLocalClient(ref Landmine __instance)
        {
            // [Game bugfix] Disable mine blinking animation when it's disabled
            if(__instance.mineAnimator.enabled != __instance.mineActivated)
                __instance.mineAnimator.enabled = __instance.mineActivated;
        }
    }
}
