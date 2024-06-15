using Unity.Netcode;

namespace HazardControl.Behaviors;

public class HazardControlConfigSync : NetworkBehaviour
{
    private bool configReceived = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
            return;
        
        Plugin.Log.LogDebug("Asking server for updated configuration...");
        RequestConfigSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestConfigSyncServerRpc()
    {
        var config = new HazardControlConfig()
        {
            TurretsKey = Plugin.GameConfig.TurretsKey.Value,
            MinesKey = Plugin.GameConfig.MinesKey.Value,
            KeyUseProbability = Plugin.GameConfig.KeyUseProbability.Value,
            MinesZap = Plugin.GameConfig.MinesZap.Value,
            TurretsZap = Plugin.GameConfig.TurretsZap.Value,
            EnemiesTriggerMines = Plugin.GameConfig.EnemiesTriggerMines.Value
        };
        Plugin.Log.LogDebug("[Sync] A client requested a config update.");
        ConfigSyncClientRpc(config);
    }

    [ClientRpc]
    public void ConfigSyncClientRpc(HazardControlConfig config)
    {
        if (IsServer || configReceived)
            return;
        
        Plugin.GameConfig.TurretsKey.Value = config.TurretsKey;
        Plugin.GameConfig.MinesKey.Value = config.MinesKey;
        Plugin.GameConfig.KeyUseProbability.Value = config.KeyUseProbability;
        Plugin.GameConfig.MinesZap.Value = config.MinesZap;
        Plugin.GameConfig.TurretsZap.Value = config.TurretsZap;
        Plugin.GameConfig.EnemiesTriggerMines.Value = config.EnemiesTriggerMines;
        
        Plugin.Log.LogDebug("Configuration synchronized!");
        configReceived = true;
    }
}

public struct HazardControlConfig : INetworkSerializeByMemcpy
{
    public bool TurretsKey;
    public bool MinesKey;
    public int  KeyUseProbability;
    public bool TurretsZap;
    public bool MinesZap;
    public bool EnemiesTriggerMines;
}