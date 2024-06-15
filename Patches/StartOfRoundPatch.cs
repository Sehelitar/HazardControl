using System.Linq;
using HarmonyLib;
using HazardControl.Behaviors;
using UnityEngine;

namespace HazardControl.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Awake()
    {
        // Get all GameObjects, including hidden ones
        var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        // Look for turret and landmine
        var hazards = (from o in gameObjects
            where o.name is "Landmine" or "TurretScript" or "TurretContainer"
            select o).ToArray();
        
        foreach(var hazard in hazards)
        {
            switch (hazard.name)
            {
                case "TurretContainer":
                    // If config allows turrets to be disabled
                    if (Plugin.GameConfig.TurretsKey.Value)
                    {
                        var cont = new GameObject("PickableZone", 
                            typeof(InteractTrigger), typeof(DisarmTurret), typeof(BoxCollider))
                        {
                            layer = 9, tag = "InteractTrigger",
                            transform =
                            {
                                position = hazard.transform.position,
                                parent = hazard.transform
                            }
                        };

                        // Box Collider
                        var box = cont.GetComponent<BoxCollider>();
                        box.size = new Vector3(.75f, 2f, 1f);
                        box.center = new Vector3(0f, 1f, 0f);
                        box.excludeLayers = 1 << 21;
                        box.isTrigger = true;
                    }
                    break;
                case "TurretScript":
                    // If config allows turrets to be temporarily disabled by ZapGun
                    if (Plugin.GameConfig.TurretsZap.Value)
                        hazard.gameObject.AddComponent<ShockableTurret>();
                    break;
                case "Landmine":
                    // If config allows mines to be triggered by ZapGun
                    if (hazard.GetComponent<Landmine>() is not null && Plugin.GameConfig.MinesZap.Value)
                        hazard.gameObject.AddComponent<ShockableMine>();
                    
                    // If config allows mines to be disarmed
                    if (hazard.GetComponent<Landmine>() is null && Plugin.GameConfig.MinesKey.Value)
                    {
                        // Add disarm interact trigger + hover tooltip
                        var cont = new GameObject( "PickableZone", 
                            typeof(InteractTrigger), typeof(DisarmMine), typeof(BoxCollider))
                        {
                            layer = 9, tag = "InteractTrigger",
                            transform =
                            {
                                position = hazard.transform.position,
                                parent = hazard.transform
                            }
                        };

                        // Box Collider
                        var box = cont.GetComponent<BoxCollider>();
                        box.size = Vector3.one;
                        box.center = Vector3.zero;
                        box.excludeLayers = 1 << 21;
                        box.isTrigger = true;
                    }
                    break;
            }
        }
        
        StartOfRound.Instance.gameObject.AddComponent<HazardControlConfigSync>();
    }
}