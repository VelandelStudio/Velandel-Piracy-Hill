using UnityEngine;
using emotitron.Network.NST;

namespace VelandelPiracyHill
{
    public class CannonAnimationsEvents : Photon.MonoBehaviour
    {
        private PhotonView myView;
        private GameObject pirate;

        private void Awake()
        {
            myView = transform.root.GetComponent<PhotonView>();
            //IDProvider.BuildIDFor(gameObject, myView.isMine);
            pirate = GetComponentInChildren<PirateAnimationsEvents>().gameObject;
            enabled = myView.isMine;
        }

        public void StartShooting()
        {
            myView.RPC("RPC_StartShooting", PhotonTargets.All, pirate.name, gameObject.name);
        }
    }
}