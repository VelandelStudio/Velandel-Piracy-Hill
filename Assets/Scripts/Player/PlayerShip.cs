using UnityEngine;
using System.Collections;
using emotitron.Network.NST.Sample;

namespace VelandelPiracyHill
{
    public class PlayerShip : Photon.PunBehaviour
    {
        [HideInInspector] public Camera[] PlayerCams;
        [HideInInspector] public Rigidbody[] Rbs;
        [HideInInspector] public NSTSampleController Controller;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            PlayerCams = GetComponentsInChildren<Camera>();
            Rbs = GetComponentsInChildren<Rigidbody>();
            Controller = GetComponent<NSTSampleController>();

            if (!photonView.isMine)
            {
                for(int i = 0; i < PlayerCams.Length; i++)
                {
                    PlayerCams[i].gameObject.SetActive(false);
                }
            
                for (int i = 0; i < PlayerCams.Length; i++)
                {
                    Rbs[i].gameObject.SetActive(false);
                }

                Controller.enabled = false;
            }
        }
    }
}