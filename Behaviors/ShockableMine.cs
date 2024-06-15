using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HazardControl.Behaviors
{
    internal class ShockableMine : NetworkBehaviour, IShockableWithGun
    {
        private bool mineShocked = false;
        private GrabbableObject shockedBy = null;

        public bool CanBeShocked() {
            var mine = GetComponent<Landmine>();
            return mine.mineActivated && !mine.hasExploded;
        }
        public float GetDifficultyMultiplier() => .25f; 
        public NetworkObject GetNetworkObject() => NetworkObject;
        public Vector3 GetShockablePosition() => gameObject.transform.position;
        public Transform GetShockableTransform() => gameObject.transform;

        public void ShockWithGun(PlayerControllerB shockedByPlayer)
        {
            mineShocked = true;
            RoundManager.Instance.FlickerLights();

            if (!shockedByPlayer.IsOwner)
                return;
            
            shockedBy = shockedByPlayer.currentlyHeldObjectServer;
            StartCoroutine(AutoStopShocking());
        }

        public void StopShockingWithGun()
        {
            if (!mineShocked)
                return;
            
            mineShocked = false;
            RoundManager.Instance.FlickerLights();
        }

        private IEnumerator AutoStopShocking()
        {
            yield return new WaitForSeconds(.5f);
            RoundManager.Instance.FlickerLights(true);
            GetComponent<Landmine>().TriggerMineOnLocalClientByExiting(); // BOOM
            ((PatcherTool)shockedBy)?.StopShockingAnomalyOnClient();
        }
    }
}
