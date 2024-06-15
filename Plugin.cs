using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HazardControl;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
public class Plugin : BaseUnityPlugin
{
    
    internal static ManualLogSource Log { get; private set; }
    internal static PluginConfigStruct GameConfig { get; private set; }
    private static Harmony _globalHarmony;
    
    private void Awake()
    {
        Log = Logger;

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        // Apply Evaisa's NetworkPatcher
        PatchNetwork();

        _globalHarmony = new Harmony("HazardControl");
        _globalHarmony.PatchAll();
        
        Config.SaveOnConfigSet = false;
        GameConfig = new PluginConfigStruct
        {
            TurretsKey = Config.Bind("Keys", "TurretsKey", true, @"Allows the use of keys to disable turrets."),
            MinesKey = Config.Bind("Keys", "MinesKey", true, @"Allows the use of keys to disable mines."),
            KeyUseProbability = Config.Bind("Keys", "KeyUseProbability", 80, @"Probability (0-100) to consume a key when used."),
            TurretsZap = Config.Bind("Zap", "TurretsZap", true, @"Allows the use of ZapGun to temporarily disable turrets."),
            MinesZap = Config.Bind("Zap", "MinesZap", true, @"Allows the use of ZapGun to trigger mines."),
            EnemiesTriggerMines = Config.Bind("General", "EnemiesTriggerMines", true, @"Enemies can trigger mines when they walk on it."),
        };
    }
    
    private static void PatchNetwork()
    {
        try
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes =
                        method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length <= 0) continue;
                    Log.LogInfo("Initialize network patch for " + type.FullName);
                    method.Invoke(null, null);
                }
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
}

internal struct PluginConfigStruct
{
    public ConfigEntry<bool> TurretsKey;
    public ConfigEntry<bool> MinesKey;
    public ConfigEntry<int> KeyUseProbability;
    public ConfigEntry<bool> TurretsZap;
    public ConfigEntry<bool> MinesZap;
    public ConfigEntry<bool> EnemiesTriggerMines;
}