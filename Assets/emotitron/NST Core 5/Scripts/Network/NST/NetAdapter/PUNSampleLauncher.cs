using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	// Future interface for making a standard implementation across Libraries...
	public interface IPlayerPrefabDefinition
	{
		GameObject PlayerPrefab { get; set; }
	}

	/// <summary>
	/// This is a very basic PUN implementation I have supplied to make it easy to quicky get started.
	/// It doesn't make use of a lobby so it only uses one scene, which eliminates the need to add any
	/// scenes to the build. Your actual game using PUN likely will want to have multiple scenes and you
	/// will want to replace all of this code with your own.
	/// </summary>
	public class PUNSampleLauncher : Singleton<PUNSampleLauncher>, IOnConnect, IOnJoinRoom, IOnJoinRoomFailed, IPlayerPrefabDefinition
	{
		[Tooltip("The prefab to use for representing the player")]
		public GameObject playerPrefab;
		public GameObject PlayerPrefab { get { return playerPrefab; } set { playerPrefab = value; } }

		/// <summary>
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		/// </summary>
		string _gameVersion = "1";

#if UNITY_EDITOR
		private void Reset()
		{
			// On creation, see if there is a UNET network manager and copy the playerprefab from that.
			playerPrefab = null;
			NetAdapterTools.CopyPlayerPrefab();
		}
#endif

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		protected override void Awake()
		{
			// This test allows this component to be used in UNET scenes without breaking anything.
			if (MasterNetAdapter.NetLib != NetworkLibrary.PUN)
			{
				Debug.LogWarning("Not using Photon PUN. Destroying " + typeof(PUNSampleLauncher).Name + " on GameObject " + name);
				Destroy(this);

				return;
			}

			// Destroy any UNET stuff in the scene if we aren't running unet.
			if (MasterNetAdapter.NetLib != NetworkLibrary.UNET)
			{
				UnityEngine.Networking.NetworkManagerHUD nmh = GetComponent<UnityEngine.Networking.NetworkManagerHUD>();
				if (nmh)
					Destroy(nmh);

				UnityEngine.Networking.NetworkManager nm = GetComponent<UnityEngine.Networking.NetworkManager>();
				if (nm)
					Destroy(nm);
			}
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			MasterNetAdapter.PUN_AutoJoinLobby = false;

			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			MasterNetAdapter.PUN_AutomaticallySyncScene = true;
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			Connect();
		}

		/// <summary>
		/// We use interfaces to get messages from the network adapters, so we need to register this MB to make its interfaces
		/// known to the MasterNetAdapter.
		/// </summary>
		protected void OnEnable()
		{
			MasterNetAdapter.RegisterCallbackInterfaces(this);
		}

		protected void OnDisable()
		{
			MasterNetAdapter.UnregisterCallbackInterfaces(this);
		}

		public void OnConnect(ServerClient svrclnt)
		{
			MasterNetAdapter.PUN_JoinRandomRoom();
		}

		public void OnJoinRoom()
		{
			SpawnLocalPlayer();
		}

		public void OnJoinRoomFailed()
		{
			DebugX.Log("Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
			MasterNetAdapter.PUN_CreateRoom(null, 8);
		}

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (MasterNetAdapter.PUN_Connected)
			{
				MasterNetAdapter.PUN_JoinRandomRoom();
			}
			else
			{
				MasterNetAdapter.PUN_ConnectUsingSettings(_gameVersion);
			}
		}

		private void SpawnLocalPlayer()
		{
			// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
			if (MasterNetAdapter.NetLib == NetworkLibrary.PUN)
			{
				Transform tr = NSTSamplePlayerSpawn.GetRandomSpawnPoint();
				Vector3 pos = (tr) ? tr.position : Vector3.zero;
				Quaternion rot = (tr) ? tr.rotation : Quaternion.identity;

				MasterNetAdapter.Spawn(playerPrefab, pos, rot, null);
			}
		}

#if UNITY_EDITOR

		[MenuItem("Window/NST/Add PUN Bootstrap", false, 1)]

		public static void AddPUNLauncher()
		{
			if (Single)
				return;

			EnsureExistsInScene("NST PUN Launcher", true);
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(PUNSampleLauncher))]
	[CanEditMultipleObjects]
	public class PUNSampleLauncherEditor : NSTSampleHeader
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Sample PUN launcher code that creates a PUN room and spawns players.", MessageType.None);
		}
	}

#endif

}