using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HazardControl.Patches
{
    [HarmonyPatch(typeof(KeyItem))]
    internal class KeyItemPatch
    {
        [HarmonyPatch("ItemActivate")]
        [HarmonyPostfix]
        public static void ItemActivate(ref KeyItem __instance, bool used, bool buttonDown = true)
        {
            if (__instance.playerHeldBy == null || !__instance.IsOwner)
                return;

            // Custom raycast to look for map hazards (mines & turrets)
            RaycastHit? matchingHit = null;
            var hitsInfo = Physics.RaycastAll(
                new Ray(__instance.playerHeldBy.gameplayCamera.transform.position, __instance.playerHeldBy.gameplayCamera.transform.forward),
                3f,
                2097152
            );

            foreach(var hit in hitsInfo)
            {
                if (hit.transform.gameObject.layer != 21 ||
                    hit.transform.name is not ("Landmine" or "TurretContainer" or "TurretScript")) continue;
                matchingHit = hit;
                break;
            }
            
            if (!matchingHit.HasValue)
            {
                Plugin.Log.LogDebug($"[Landmine/Turret] Raycast failed, IsOwner={__instance.IsOwner}, IsHeld={__instance.playerHeldBy != null}");
                return;
            }

            var distance = Vector3.Distance(__instance.playerHeldBy.gameplayCamera.transform.position, matchingHit.Value.transform.position);
            Plugin.Log.LogDebug($"[Landmine/Turret] GameObject hit {matchingHit.Value.transform.name} in layer {matchingHit.Value.transform.gameObject.layer} at a distance of {distance}");

            // Landmine found
            var landmine = matchingHit.Value.transform.GetComponentInChildren<Landmine>() ?? matchingHit.Value.transform.GetComponent<Landmine>();
            if(landmine != null && !landmine.hasExploded && landmine.mineActivated)
            {
                landmine.ToggleMine(false);
                landmine.mineAnimator.enabled = false;
                var probability = Random.RandomRangeInt(0, 100);
                if(probability < Plugin.GameConfig.KeyUseProbability.Value)
                    __instance.playerHeldBy.DespawnHeldObject();
                return;
            }

            // Turret found
            var turret = matchingHit.Value.transform.GetComponentInChildren<Turret>() ?? matchingHit.Value.transform.GetComponent<Turret>();
            if (turret != null && turret.turretActive)
            {
                turret.ToggleTurretEnabled(false);
                var probability = Random.RandomRangeInt(0, 100);
                if(probability < Plugin.GameConfig.KeyUseProbability.Value)
                    __instance.playerHeldBy.DespawnHeldObject();
                return;
            }

            Plugin.Log.LogDebug($"[Landmine/Turret] Could not find a landmine or a turret component :( HasExploded={landmine?.hasExploded} GOLayer={matchingHit.Value.transform.gameObject.layer}");
        }
    }
}
