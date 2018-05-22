using UnityEngine;

namespace VelandelPiracyHill
{
    public class CannonAnimationsEvents : Photon.MonoBehaviour
    {
        private PhotonView myView;
        private GameObject pirate;

        private void Awake()
        {
            myView = transform.root.GetComponent<PhotonView>();
            pirate = GetComponentInChildren<PirateAnimationsEvents>().gameObject;
            enabled = myView.isMine;
        }

        public void StartShooting()
        {
            myView.RPC("RPC_StartShooting", PhotonTargets.All, pirate.name, gameObject.name);
        }
    }
}