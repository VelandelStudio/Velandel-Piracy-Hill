using emotitron.Network.NST;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class RPCAnimEvents : Photon.PunBehaviour
    {

        [SerializeField] private GameObject cannonBallAnim;
        private NSTElementsEngine nstElementEngine;
        private void Awake()
        {
            nstElementEngine = GetComponent<NSTElementsEngine>();
            enabled = photonView.isMine;
        }

        #region Pirate and Cannon shoots

        [PunRPC]
        private void RPC_StartReloadingCannon(string cannonID)
        {
            Animator cannonAnim = nstElementEngine.elementLookup[cannonID].gameobject.GetComponent<Animator>();
            cannonAnim.SetTrigger("StartReloading");
        }

        [PunRPC]
        private void RPC_EndReloadingCannon(string cannonID)
        {
            Animator cannonAnim = nstElementEngine.elementLookup[cannonID].gameobject.GetComponent<Animator>();
            cannonAnim.SetTrigger("EndReloading");
        }

        [PunRPC]
        private void RPC_PickUpNewCannonBall(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = nstElementEngine.elementLookup[pirateID].gameobject.transform;
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Instantiate(cannonBallAnim, cannonBallSlot.transform);
        }

        [PunRPC]
        private void RPC_DropCannonBall(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = nstElementEngine.elementLookup[pirateID].gameobject.transform;
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            cannonBallSlot.transform.SetParent(pirate);
        }

        [PunRPC]
        private void RPC_PickUpCannonBall(string pirateID, string cannonBallSlotName, string cannonBallSlotParentName)
        {
            Transform pirate = nstElementEngine.elementLookup[pirateID].gameobject.transform;
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Transform cannonBallSlotParent = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotParentName);

            cannonBallSlot.transform.SetParent(cannonBallSlotParent);
        }

        [PunRPC]
        private void RPC_DestroyHeldObject(string pirateID, string cannonBallSlotName)
        {
            Transform pirate = nstElementEngine.elementLookup[pirateID].gameobject.transform;
            Transform cannonBallSlot = TransformExtensions.FindAnyChild<Transform>(pirate, cannonBallSlotName);
            Destroy(cannonBallSlot.GetChild(0).gameObject);
        }

        [PunRPC]
        private void RPC_NotifyCannonLoaded(string cannonID)
        {
            Animator cannonAnim = nstElementEngine.elementLookup[cannonID].gameobject.GetComponent<Animator>();
            cannonAnim.SetBool("CannonLoaded", true);
        }

        [PunRPC]
        public void RPC_StartShooting(string pirateID, string cannonID)
        {
            Animator pirateAnim = nstElementEngine.elementLookup[pirateID].gameobject.GetComponent<Animator>();
            Animator cannonAnim = nstElementEngine.elementLookup[cannonID].gameobject.GetComponent<Animator>();
            cannonAnim.SetBool("CannonLoaded", false);
            pirateAnim.SetTrigger("StartShooting");
            cannonAnim.GetComponentInChildren<ParticleSystem>().Play();
        }

        #endregion
    }
}
