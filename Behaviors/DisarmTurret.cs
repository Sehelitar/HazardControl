using Unity.Netcode;
using UnityEngine;

namespace HazardControl.Behaviors
{
    [RequireComponent(typeof(InteractTrigger))]
    internal class DisarmTurret : NetworkBehaviour
    {
        private Turret turret;
        private InteractTrigger turretTrigger;

        public void Awake()
        {
            turretTrigger = GetComponent<InteractTrigger>();
            turretTrigger.enabled = true;
            turretTrigger.interactable = false;
            turretTrigger.touchTrigger = false;
            turretTrigger.disableTriggerMesh = false;
            turretTrigger.disabledHoverTip = "";
            turretTrigger.hoverTip = turretTrigger.disabledHoverTip;
        }

        private void Update()
        {
            turret ??= transform.parent?.GetComponent<Turret>() ?? transform.parent?.GetComponentInChildren<Turret>();
            if (!turret)
                return;
            if (turret.turretActive)
            {
                if (GameNetworkManager.Instance is null || GameNetworkManager.Instance.localPlayerController is null)
                    return;
                turretTrigger.disabledHoverTip = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer is null ||
                    GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.itemId != 14
                    ? "" : (!StartOfRound.Instance.localPlayerUsingController ? "Deactivate: [ LMB ]" : "Deactivate: [R-trigger]");
            }
            else
            {
                turretTrigger.disabledHoverTip = "Deactivated";
            }
            turretTrigger.hoverTip = turretTrigger.disabledHoverTip;
        }
    }
}
