using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VelandelPiracyHill
{
    public class MainMenu : Photon.PunBehaviour
    {
        static MainMenu instance;
        GameObject ui;
        Button joinGameButton;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            ui = transform.FindAnyChild<Transform>("UI").gameObject;
            joinGameButton = transform.FindAnyChild<Button>("JoinGameButton");

            joinGameButton.interactable = false;
            ui.SetActive(true);
        }

        public override void OnConnectedToMaster()
        {
            joinGameButton.interactable = true;
        }

        void OnLevelWasLoaded(int level)
        {
            ui.SetActive(!PhotonNetwork.inRoom);
        }
    }
}