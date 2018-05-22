using UnityEngine;

namespace VelandelPiracyHill
{
    public class PirateAnimationsEvents : Photon.MonoBehaviour
    {
        [SerializeField] private GameObject cannonBallSlot;
        private GameObject cannon;
        private Transform cannonBallSlotParent;

        private PhotonView myView;
        private void Awake()
        {
            myView = transform.root.GetComponent<PhotonView>();

            cannon = transform.parent.parent.gameObject;
            cannonBallSlotParent = cannonBallSlot.transform.parent;

            enabled = myView.isMine;
        }

        #region AnimationEvents		
        public void StartReloadingCannon()
        {
            myView.RPC("RPC_StartReloadingCannon", PhotonTargets.All, cannon.name);
        }

        public void EndReloadingCannon()
        {
            myView.RPC("RPC_EndReloadingCannon", PhotonTargets.All, cannon.name);
        }

        public void PickUpNewCannonBall()
        {
            myView.RPC("RPC_PickUpNewCannonBall", PhotonTargets.All, gameObject.name, cannonBallSlot.name);
        }

        public void DropCannonBall()
        {
            myView.RPC("RPC_DropCannonBall", PhotonTargets.All, gameObject.name, cannonBallSlot.name);
        }

        public void PickUpCannonBall()
        {
            myView.RPC("RPC_PickUpCannonBall", PhotonTargets.All, gameObject.name, cannonBallSlot.name, cannonBallSlotParent.name);
        }

        public void DestroyHeldObject()
        {
            myView.RPC("RPC_DestroyHeldObject", PhotonTargets.All, gameObject.name, cannonBallSlot.name);
        }

        public void NotifyCannonLoaded()
        {
            myView.RPC("RPC_NotifyCannonLoaded", PhotonTargets.All, cannon.name);
        }
        #endregion
    }
}
