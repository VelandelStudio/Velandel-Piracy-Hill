using UnityEngine;
using emotitron.Network.NST;

namespace VelandelPiracyHill
{
    public class CannonAnimationsEvents : Photon.MonoBehaviour
    {
        private PhotonView myView;
        private string cannonID;
        private string pirateID;

        private void Awake()
        {
            cannonID = GetComponent<NSTPositionElement>().positionElement.name;
            pirateID = GetComponentInChildren<PirateAnimationsEvents>().GetComponent<NSTPositionElement>().positionElement.name;
            myView = transform.root.GetComponent<PhotonView>();
            enabled = myView.isMine;
        }

        public void StartShooting()
        {
            myView.RPC("RPC_StartShooting", PhotonTargets.All, pirateID, cannonID);
        }
    }
}