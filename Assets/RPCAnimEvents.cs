using emotitron.Network.NST;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class RPCAnimEvents : Photon.PunBehaviour
    {

        [SerializeField] private GameObject cannonBallAnim;
        [SerializeField] private Transform CannonsSides;

        private void Awake()
        {
            enabled = photonView.isMine;
        }

        #region Pirate and Cannon shoots

        [PunRPC]
        private void RPC_StartReloadingCannon(string cannonID)
        {
            Animator cannonAnim = TransformExtensions.FindAnyChild<Transform>(CannonsSides, cannonID).GetComponent<Animator>();
            cannonAnim.SetTrigger("StartReloading");
        }

        [PunRPC]
        private void RPC_EndReloadingCannon(string cannonID)
        {
            Animator cannonAnim = TransformExtensions.FindAnyChild<Transform>(CannonsSides, cannonID).GetComponent<Animator>();
            cannonAnim.SetTrigger("EndReloading");
        }

        [PunRPC]
        private void RPC_PickUpNewCannonBall(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = TransformExtensions.FindAnyChild<Transform>(CannonsSides, pirateID);
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Instantiate(cannonBallAnim, cannonBallSlot.transform);
        }

        [PunRPC]
        private void RPC_DropCannonBall(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = TransformExtensions.FindAnyChild<Transform>(CannonsSides, pirateID);
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            cannonBallSlot.transform.SetParent(pirate);
        }

        [PunRPC]
        private void RPC_PickUpCannonBall(string pirateID, string cannonBallSlotName, string cannonBallSlotParentName)
        {
            Transform pirate = TransformExtensions.FindAnyChild<Transform>(CannonsSides, pirateID);
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Transform cannonBallSlotParent = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotParentName);

            cannonBallSlot.transform.SetParent(cannonBallSlotParent);
        }

        [PunRPC]
        private void RPC_DestroyHeldObject(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = TransformExtensions.FindAnyChild<Transform>(CannonsSides, pirateID);
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Destroy(cannonBallSlot.GetChild(0).gameObject);
        }

        [PunRPC]
        private void RPC_NotifyCannonLoaded(string cannonID)
        {
            Animator cannonAnim = TransformExtensions.FindAnyChild<Transform>(CannonsSides, cannonID).GetComponent<Animator>();
            cannonAnim.SetBool("CannonLoaded", true);
        }

        [PunRPC]
        public void RPC_StartShooting(string pirateID, string cannonID)
        {
            Debug.Log(cannonID);
            Animator cannonAnim = TransformExtensions.FindAnyChild<Transform>(CannonsSides, cannonID).GetComponent<Animator>();
            Animator pirateAnim = TransformExtensions.FindAnyChild<Transform>(cannonAnim.transform, pirateID).GetComponent<Animator>();
            pirateAnim.SetTrigger("StartShooting");
            cannonAnim.GetComponentInChildren<ParticleSystem>().Play();
        }

        #endregion
    }
}
