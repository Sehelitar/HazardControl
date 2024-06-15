using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HazardControl.Behaviors
{
    internal class ShockableTurret : NetworkBehaviour, IShockableWithGun
    {
        private DateTime? startedAt = null;
        private Coroutine disabledTurret = null;

        public bool CanBeShocked() => true;
        public float GetDifficultyMultiplier() => .25f;
        public NetworkObject GetNetworkObject() => NetworkObject;
        public Vector3 GetShockablePosition() => gameObject.transform.position;
        public Transform GetShockableTransform() => gameObject.transform;

        public void ShockWithGun(PlayerControllerB shockedByPlayer)
        {
            RoundManager.Instance.FlickerLights(true);
            if (!GetComponent<Turret>().turretActive || !shockedByPlayer.IsOwner)
                return;
        
            startedAt = DateTime.Now;
            GetComponent<Turret>().ToggleTurretEnabled(false);
        }

        public void StopShockingWithGun()
        {
            if (!startedAt.HasValue)
                return;
            
            var diff = DateTime.Now - startedAt.Value;
            if(disabledTurret != null)
                StopCoroutine(disabledTurret);
            disabledTurret = StartCoroutine(ReenableIn((int)(diff.TotalSeconds*10f)));
            startedAt = null;
        }

        private IEnumerator ReenableIn(int waitFor)
        {
            yield return new WaitForSeconds(waitFor);
            RoundManager.Instance.FlickerLights(true);
            GetComponent<Turret>().ToggleTurretEnabled(true);
        }
    }
}
