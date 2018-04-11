//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	// ver 3

	/// <summary>
	/// The UNET version of this interface for the NSTMaster - unifying code to work with both UNET and Photon.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(NetworkIdentity))]
	[NetworkSettings(sendInterval = .000001f)]

	public class MasterNetAdapter : NetworkBehaviour //, INSTMasterAdapter
	{
		[HideInInspector]
		public static bool networkStarted;

		public static MasterNetAdapter single;
		public const string ADAPTER_NAME = "UNET";
		
		public const NetworkLibrary NET_LIB = NetworkLibrary.UNET;
				
		/// <summary>
		/// Attribute for getting the NET_LIB value, without throwing warnings about unreachable code.
		/// </summary>		
		public static NetworkLibrary NetLib { get { return NET_LIB; } }

		public const NetworkModel NET_MODEL = NetworkModel.ServerClient;

		// TODO this likely needs an actual test
		public static int MasterClientId = 0;

		// Interfaced fields
		public NetworkLibrary NetLibrary { get { return NET_LIB; } }
		public static NetworkLibrary NetworkLibrary { get { return NET_LIB; } }

		public static bool Connected { get { return NetworkServer.active || NetworkClient.active; } }
		public static bool ServerIsActive { get { return NetworkServer.active; } }
		public static bool ClientIsActive { get { return NetworkClient.active; } }
		public static bool NetworkIsActive { get { return NetworkClient.active || NetworkServer.active; } }

		public const short LowestMsgTypeId = MsgType.Highest;
		public const short HighestMsgTypeId = short.MaxValue;
		public const short DefaultMsgTypeId = 190;

		public bool IsRegistered { get { return isRegistered; } set { isRegistered = value; } }

		#region Callback Interfaces

		[HideInInspector] public static List<Component> iNetEvents = new List<Component>();
		[HideInInspector] public static List<Component> iOnConnect = new List<Component>();
		[HideInInspector] public static List<Component> iOnStartLocalPlayer = new List<Component>();
		[HideInInspector] public static List<Component> iOnNetworkDestroy = new List<Component>();
		[HideInInspector] public static List<Component> iOnJoinRoom = new List<Component>();
		[HideInInspector] public static List<Component> iOnJoinRoomFailed = new List<Component>();

		public static void RegisterCallbackInterfaces(Component obj)
		{
			MasterNetCommon.RegisterCallbackInterfaces(obj);
		}

		public static void UnregisterCallbackInterfaces(Component obj)
		{
			MasterNetCommon.UnregisterCallbackInterfaces(obj);
		}

		#endregion

		// Statics
		private static NetworkWriter writer = new NetworkWriter();
		private static short masterMsgTypeId;
		private static bool isRegistered;



		private void Awake()
		{
			if (!EnforceSingleton())
				return;
		}

		// Run RegisterHandlers again in Start in case the adapter was added late and OnStartServer and OnStartClient never ran.
		private void Start()
		{
			networkStarted = NetworkServer.active || NetworkClient.active;
			RegisterHanders();

		}

		/// <summary>
		/// Returns true if this is the singleton, false if we had to destroy it.
		/// </summary>
		private bool EnforceSingleton()
		{

			if (single && single != this)
			{
				Destroy(this);
				return false;
			}

			single = this;
			return true;

		}

		public override void OnStartServer()
		{
			RegisterHanders();

			networkStarted = true;

			foreach (INetEvents cb in iNetEvents)
				cb.OnConnect(ServerClient.Server);

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Server);
		}
		public override void OnStartClient()
		{
			RegisterHanders();

			networkStarted = true;

			foreach (INetEvents cb in iNetEvents)
				cb.OnConnect(ServerClient.Client);

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Client);
		}

		public override void OnNetworkDestroy()
		{
			networkStarted = false;

			if (iNetEvents != null)
				foreach (INetEvents cb in iNetEvents)
					cb.OnNetworkDestroy();

			if (iOnNetworkDestroy != null)
				foreach (IOnNetworkDestroy cb in iOnNetworkDestroy)
					cb.OnNetworkDestroy();

			isRegistered = false;
			NetworkServer.UnregisterHandler(HeaderSettings.Single.masterMsgTypeId);
			NetworkManager.singleton.client.UnregisterHandler(masterMsgTypeId);

		}

		private void RegisterHanders()
		{
			if (IsRegistered)
				return;

			masterMsgTypeId = HeaderSettings.Single.masterMsgTypeId;

			if (NetworkServer.active)
			{
				NetworkServer.RegisterHandler(masterMsgTypeId, ReceiveUpdate);
				isRegistered = true;
			}

			else if (NetworkClient.active)
			{
				NetworkManager.singleton.client.RegisterHandler(masterMsgTypeId, ReceiveUpdate);
				isRegistered = true;
			}
		}

		
		/// <summary>
		///  Updates over the network arrive here - AFTER the Update() runs (not tested for all platforms... thanks unet for the great docs.) 
		///  The incoming bitstream is read
		/// </summary>
		/// <param name="msg"></param>
		private static void ReceiveUpdate(NetworkMessage msg)
		{
			
			UdpBitStream bitstream = new UdpBitStream(msg.reader.ReadBytesNonAlloc(NSTMaster.bitstreamByteArray, msg.reader.Length), msg.reader.Length);
			UdpBitStream outstream = new UdpBitStream(NSTMaster.outstreamByteArray);

			NSTMaster.ReceiveUpdate(ref bitstream, ref outstream, NetworkServer.active, msg.conn.connectionId);

			// Write a clone message and pass it to all the clients if this is the server receiving
			if (NetworkServer.active) // && msg.conn == nst.NI.clientAuthorityOwner)
			{
				writer.StartMessage(msg.msgType);
				writer.WriteUncountedByteArray(outstream.Data, outstream.BytesUsed);
				writer.SendPayloadArrayToAllClients(msg.msgType);
				if (NetworkServer.connections[0] != null)
					NetworkServer.connections[0].FlushChannels();
			}
		}

		public static void SendUpdate(ref UdpBitStream bitstream, ref UdpBitStream outstream)
		{
			// Send the bitstream to the UNET writer
			writer.StartMessage(masterMsgTypeId);
			writer.WriteUncountedByteArray(NSTMaster.bitstreamByteArray, bitstream.BytesUsed);
			writer.FinishMessage();

			// if this is the server - send to all.
			if (NetworkServer.active)
			{
				writer.SendPayloadArrayToAllClients(masterMsgTypeId, Channels.DefaultUnreliable);
				//NetworkServer.connections[0].FlushChannels();

				// If this is the server as client, run the ReceiveUpdate since local won't get this run.
				//if (NetworkClient.active)
					NSTMaster.ReceiveUpdate(ref bitstream, ref outstream, false, 0);
				
			}
			// if this is a client send to server.
			else
			{
				NetworkManager.singleton.client.SendWriter(writer, Channels.DefaultUnreliable);
				//NetworkManager.singleton.client.connection.FlushChannels();
			}
		}

		#region UNET Specific methods

		public static Transform UNET_GetPlayerSpawnPoint()
		{
			return NetworkManager.singleton.GetStartPosition();
		}
		
		public static void UNET_RegisterStartPosition(Transform tr)
		{
			NetworkManager.RegisterStartPosition(tr);
		}

		public static void UNET_UnRegisterStartPosition(Transform tr)
		{
			NetworkManager.UnRegisterStartPosition(tr);
		}

		public static GameObject UNET_GetRegisteredPlayerPrefab()
		{
			if (NetworkManager.singleton == null)
				NetworkManager.singleton = FindObjectOfType<NetworkManager>();

			if (NetworkManager.singleton != null)
			{
				return NetworkManager.singleton.playerPrefab;
			}
			return null;
		}

		#endregion

		#region PUN Specific relays

		public static bool PUN_AutoJoinLobby{ get { return false; } set { } }
		public static bool PUN_AutomaticallySyncScene{ get { return false; } set { } }
		public static bool PUN_Connected { get { return false; } }
		public static void PUN_ConnectUsingSettings(string gameversion)	{}
		public static void PUN_JoinRandomRoom() { }
		public static void PUN_LoadLevel(string scenename) { }
		public static void PUN_CreateRoom(string roomname, int maxPlayer) { }

		#endregion


		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = Instantiate(prefab, position, rotation, parent);
			NetworkServer.Spawn(go);
			return go;
		}


#if UNITY_EDITOR
		/// <summary>
		/// Add a NetworkIdentity to the supplied NSTMaster gameobject. Sets localPlayerAuth to false (master isn't a player)
		/// </summary>
		/// <param name="go"></param>
		public static bool AddRequiredEntityComponentToMaster(GameObject go)
		{
			if (!go.GetComponent<NetworkIdentity>())
			{
				NetworkIdentity ni = EditorUtils.EnsureRootComponentExists<NetworkIdentity>(go);
				ni.localPlayerAuthority = false;
				return true;
			}
			return false;
		}

		public static void PurgeLibSpecificComponents()
		{
			NetAdapterTools.PurgeTypeFromEverywhere<NetworkIdentity>();
			NetAdapterTools.PurgeTypeFromEverywhere<NetworkManager>(true);
		}

		public static void AddNstEntityComponentsEverywhere()
		{
			NetAdapterTools.AddComponentsWhereverOtherComponentIsFound<NetworkSyncTransform, NSTNetAdapter, NetworkIdentity>();
		}

		public static void AddLibrarySpecificEntityComponent(GameObject go)
		{
			if (!go.GetComponent<NetworkIdentity>())
				go.AddComponent<NetworkIdentity>().assetId.IsValid();
		}
				

#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(MasterNetAdapter))]
	[CanEditMultipleObjects]
	public class MasterNetAdapterEditor : NSTHeaderEditorBase
	{
		NetworkIdentity ni;

		public override void OnEnable()
		{
			headerColor = HeaderSettingsColor;
			headerName = HeaderMasterName;
			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			MasterNetAdapter _target = (MasterNetAdapter)target;

			NetAdapterTools.EnsureSceneNetLibDependencies();

			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("This is the UNET adapter. To work with Photon PUN, switch the Network Library.", MessageType.None);
			NetLibrarySettings.Single.DrawGui(true, false);
		}
	}

#endif
}
