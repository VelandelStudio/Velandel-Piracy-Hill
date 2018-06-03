using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VelandelPiracyHill
{
    public class GameManager : Photon.PunBehaviour
    {
        public static GameManager instance;
        public static GameObject localPlayer;
        [SerializeField] private InputField playerNameInputField;
        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            instance = this;

            PhotonNetwork.automaticallySyncScene = true;
        }

        void Start()
        {
            if (!PhotonNetwork.connected)
            {
                PhotonNetwork.ConnectUsingSettings("VPH_v-a.0.0.01");
            }
        }

        public void JoinGame()
        {
            RoomOptions ro = new RoomOptions();
            ro.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom("Default Room", ro, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");

            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.LoadLevel("PUN_GameScene");
            }
        }

        void OnLevelWasLoaded(int levelNumber)
        {
            if (!PhotonNetwork.inRoom)
                return;

            localPlayer = PhotonNetwork.Instantiate(
                "PlayerShip",
                new Vector3(0, 0.4f, 0),
                Quaternion.identity, 0);
            PhotonNetwork.playerName = playerNameInputField.text;
        }
    }
}
