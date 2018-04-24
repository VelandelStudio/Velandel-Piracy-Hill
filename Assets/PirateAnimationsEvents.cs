using emotitron.Network.NST;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class PirateAnimationsEvents : Photon.MonoBehaviour
    {
        [SerializeField] private GameObject cannonBallSlot;
        private string cannonID;
        private string pirateID;
        private Transform cannonBallSlotParent;

        private PhotonView photonView;
        private void Awake()
        {
            cannonID = transform.parent.GetComponent<NSTPositionElement>().positionElement.name;
            pirateID = GetComponent<NSTPositionElement>().positionElement.name;

            cannonBallSlotParent = cannonBallSlot.transform.parent;

            photonView = transform.root.GetComponent<PhotonView>();
            enabled = photonView.isMine;
        }

        #region AnimationEvents		
        public void StartReloadingCannon()
        {
            photonView.RPC("RPC_StartReloadingCannon", PhotonTargets.All, cannonID);
        }

        public void EndReloadingCannon()
        {
            photonView.RPC("RPC_EndReloadingCannon", PhotonTargets.All, cannonID);
        }

        public void PickUpNewCannonBall()
        {
            photonView.RPC("RPC_PickUpNewCannonBall", PhotonTargets.All, pirateID, cannonBallSlot.name);
        }

        public void DropCannonBall()
        {
            photonView.RPC("RPC_DropCannonBall", PhotonTargets.All, pirateID, cannonBallSlot.name);
        }

        public void PickUpCannonBall()
        {
            photonView.RPC("RPC_PickUpCannonBall", PhotonTargets.All, pirateID, cannonBallSlot.name, cannonBallSlotParent.name);
        }

        public void DestroyHeldObject()
        {
            photonView.RPC("RPC_DestroyHeldObject", PhotonTargets.All,pirateID, cannonBallSlot.name);
        }

        public void NotifyCannonLoaded()
        {
            photonView.RPC("RPC_NotifyCannonLoaded", PhotonTargets.All, cannonID);
        }
        #endregion
    }
}
