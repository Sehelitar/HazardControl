using Unity.Netcode;
using UnityEngine;

namespace HazardControl.Behaviors
{
    [RequireComponent(typeof(InteractTrigger))]
    internal class DisarmMine : NetworkBehaviour
    {
        private Landmine mine;
        private InteractTrigger mineTrigger;

        public void Awake()
        {
            mineTrigger = GetComponent<InteractTrigger>();
            mineTrigger.enabled = true;
            mineTrigger.interactable = false;
            mineTrigger.touchTrigger = false;
            mineTrigger.disableTriggerMesh = false;
            mineTrigger.disabledHoverTip = "Armed";
            mineTrigger.hoverTip = mineTrigger.disabledHoverTip;
        }

        private void Update()
        {
            mine ??= transform.parent?.GetComponent<Landmine>() ?? transform.parent?.GetComponentInChildren<Landmine>();
            if (!mine)
                return;
            if(mine.hasExploded)
            {
                mineTrigger.disabledHoverTip = "";
            }
            else if (mine.mineActivated)
            {
                if (GameNetworkManager.Instance is null || GameNetworkManager.Instance.localPlayerController is null)
                    return;
                mineTrigger.disabledHoverTip = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer is null ||
                    GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.itemId != 14
                    ? "Armed" : (!StartOfRound.Instance.localPlayerUsingController ? "Disarm: [ LMB ]" : "Disarm: [R-trigger]");
            }
            else
            {
                mineTrigger.disabledHoverTip = "Disarmed";
            }
            mineTrigger.hoverTip = mineTrigger.disabledHoverTip;
        }
    }
}
