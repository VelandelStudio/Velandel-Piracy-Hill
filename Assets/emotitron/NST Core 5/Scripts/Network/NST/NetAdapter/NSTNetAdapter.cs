﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// This class contains the abstracted methods for different networking libraries. 
	/// This adapter is for UNET.
	/// </summary>
	[DisallowMultipleComponent]
	[NetworkSettings(sendInterval = 0)]
	[AddComponentMenu("")]
	[RequireComponent(typeof(NetworkIdentity))]

	public class NSTNetAdapter : NetworkBehaviour
	{
		public const string ADAPTER_NAME = "UNET";
		public static NetworkLibrary NetLibrary { get { return NetworkLibrary.UNET; } }

		NetworkIdentity ni;
		NSTSettings nstSettings;

		// callback interfaces... collected on awake from all children on this gameobject, and can be subcribed to as well.
		[HideInInspector] public List<INetEvents> iNetEvents = new List<INetEvents>();
		[HideInInspector] public List<IOnConnect> iOnConnect = new List<IOnConnect>();
		[HideInInspector] public List<IOnStartLocalPlayer> iOnStartLocalPlayer = new List<IOnStartLocalPlayer>();
		[HideInInspector] public List<IOnNetworkDestroy> iOnNetworkDestroy = new List<IOnNetworkDestroy>();
		[HideInInspector] public List<IOnStartAuthority> iOnStartAuthority = new List<IOnStartAuthority>();
		[HideInInspector] public List<IOnStopAuthority> iOnStopAuthority = new List<IOnStopAuthority>();
		[HideInInspector] public List<IOnStart> iOnStart = new List<IOnStart>();

		public bool IsServer { get { return isServer; } }
		public bool IsLocalPlayer { get { return isLocalPlayer; } }
		public bool IsMine { get { return hasAuthority; } }
		
		public uint NetId { get { return ni.netId.Value; } }
		//public int ClientId { get { return (ni.clientAuthorityOwner == null) ? -1 : ni.clientAuthorityOwner.connectionId; } }
		public int ClientId { get { return ni.clientAuthorityOwner.connectionId; } }

		[SyncVar]
		private uint _nstIdSyncvar;
		public uint NstIdSyncvar { get { return _nstIdSyncvar; } set { _nstIdSyncvar = value; } }
		
		[HideInInspector][NonSerialized]
		public AuthorityModel cachedAuthModel;

		public bool IAmActingAuthority
		{
			get {
				if (cachedAuthModel == AuthorityModel.ServerAuthority)
					if (NetworkServer.active)
						return true;

				if (cachedAuthModel == AuthorityModel.OwnerAuthority)
					if (hasAuthority)
						return true;

				return false;
			}
		}

		public void CollectCallbackInterfaces()
		{
			GetComponentsInChildren(true, iNetEvents);
		}

		void Awake()
		{
			cachedAuthModel = (AuthorityModel)NetLibrarySettings.Single.defaultAuthority;

			ni = GetComponent<NetworkIdentity>();
			CollectCallbackInterfaces();
		}

		public override void OnStartServer()
		{
			foreach (INetEvents cb in iNetEvents)
				cb.OnConnect(ServerClient.Server);

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Server);
		}

		public override void OnStartClient()
		{
			foreach (INetEvents cb in iNetEvents)
				cb.OnConnect(ServerClient.Client);

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Client);
			
		}

		public override void OnStartLocalPlayer()
		{
			foreach (INetEvents cb in iNetEvents)
				cb.OnStartLocalPlayer();

			foreach (IOnStartLocalPlayer cb in iOnStartLocalPlayer)
				cb.OnStartLocalPlayer();
		}

		public void Start()
		{
			DebugX.LogError("You appear to have a NetworkIdentity on instantiated object '" + name + "', but that object has NOT been network spawned. " +
				"Only use NetworkSyncTransform and NetworkIdentity on objects you intend to spawn normally from the server using NetworkServer.Spawn(). " +
					"(Projectiles for example probably don't need to be networked objects).", ni.netId.Value == 0, true);

			// If this is an invalid NST... abort startup and shut it down.
			if (ni.netId.Value == 0)
			{
				Destroy(GetComponent<NetworkSyncTransform>());
				return;
			}


			foreach (INetEvents cb in iNetEvents)
				cb.OnStart();

			foreach (IOnStart cb in iOnStart)
				cb.OnStart();
		}

		public override void OnNetworkDestroy()
		{
			if (iNetEvents != null)
				foreach (INetEvents cb in iNetEvents)
					cb.OnNetworkDestroy();

			if (iOnNetworkDestroy != null)
				foreach (IOnNetworkDestroy cb in iOnNetworkDestroy)
					cb.OnNetworkDestroy();
		}

		public override void OnStartAuthority()
		{
			if (iNetEvents != null)
				foreach (INetEvents cb in iNetEvents)
					cb.OnStartAuthority();

			if (iOnNetworkDestroy != null)
				foreach (IOnStartAuthority cb in iOnStartAuthority)
					cb.OnStartAuthority();
		}

		public override void OnStopAuthority()
		{
			if (iNetEvents != null)
				foreach (INetEvents cb in iNetEvents)
					cb.OnStopAuthority();

			if (iOnNetworkDestroy != null)
				foreach (IOnStopAuthority cb in iOnStopAuthority)
					cb.OnStopAuthority();
		}

		/// <summary>
		/// Get the RTT in seconds for the owner of this network object. Only valid on Server.
		/// </summary>
		public float GetRTT()
		{
			return MasterRTT.GetRTT(ni.clientAuthorityOwner.connectionId);

			//NetworkConnection conn = NI.clientAuthorityOwner;
			//byte error = 0;
			//return (conn == null || conn.hostId == -1) ? 0 :
			//	.001f * NetworkTransport.GetCurrentRTT(NI.clientAuthorityOwner.hostId, NI.clientAuthorityOwner.connectionId, out error);
		}
		
		/// <summary>
		/// Get the RTT to the player who owns this NST
		/// </summary>
		public static float GetRTT(NetworkSyncTransform nstOfOwner)
		{
			return nstOfOwner.na.GetRTT();
		}

		public void SendBitstreamToOwner(ref UdpBitStream bitstream)
		{
			ni.clientAuthorityOwner.SendBitstreamToThisConn(ref bitstream, Channels.DefaultUnreliable);
		}

		/// <summary>
		/// Remove the adapter and the NetworkIdentity/View from an object
		/// </summary>
		/// <param name="nst"></param>
		public static void RemoveAdapter(NetworkSyncTransform nst)
		{
			NetworkIdentity ni = nst.GetComponent<NetworkIdentity>();
			NSTNetAdapter na = nst.GetComponent<NSTNetAdapter>();

			if (na)
				DestroyImmediate(na);

			if (ni)
				DestroyImmediate(ni);
		}

#if UNITY_EDITOR

		///// <summary>
		///// Add the NetworkIdenity/PhotonView to an NST gameobject. Must be added before runtime (thus this is editor only script).
		///// If added at runtime, it may get added AFTER network events fire. Also will attempt to add this NST as a registered prefab
		///// and player prefab. Will also attempt to register the supplied go with the NetworkManager and as the PlayerPrefab if there is none
		///// but one is expected.
		///// </summary>
		//public static void EnsureHasEntityComponentForNetLib(GameObject go, bool playerPrefabCandidate = true)
		//{
		//	if (!Application.isPlaying)
		//		AddAsRegisteredPrefab(go, true, !playerPrefabCandidate, true);
		//}
		/// <summary>
		/// Attempts to add a prefab with NST on it to the NetworkManager spawnable prefabs list, after doing some checks to make 
		/// sure it makes sense to. Will then add as the network manager player prefab if it is set to auto spawwn and is still null.
		/// </summary>
		public static bool AddAsRegisteredPrefab(GameObject go, bool playerPrefabCandidate, bool silence = false)
		{
			if (Application.isPlaying)
				return false;

			// Don't replace an existing playerPrefab
			NetworkManager nm = NetAdapterTools.GetNetworkManager();
			if (!nm || nm.playerPrefab)
				return false;


			PrefabType type = PrefabUtility.GetPrefabType(go);
			GameObject prefabGO = (type == PrefabType.Prefab) ? go : PrefabUtility.GetPrefabParent(go) as GameObject;

			if (!prefabGO)
			{
				if (!silence)
					Debug.Log("You have a NST component on a gameobject '" + go.name + "', which is not a prefab. Be sure to make '" + go.name + "' a prefab, otherwise it cannot be registered with the NetworkManager for network spawning.");
				return false;
			}

			NetworkIdentity ni = prefabGO.GetComponent<NetworkIdentity>();
			// Force the NetworkIdentity to be valid. Bad things happen if we don't do this. UNET suck.
			ni.assetId.IsValid();

			if (!ni)
			{
				if (!silence)
					Debug.Log("There is no NetworkIdentity on '" + go.name + "', so it cannot be registered with the NetworkManager for network spawning.");
				return false;
			}

			if (!nm.spawnPrefabs.Contains(prefabGO))
			{
				Debug.Log("Automatically adding '<b>" + prefabGO.name + "</b>' to the NetworkManager spawn list for you.");

				nm.spawnPrefabs.Add(prefabGO);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			// Set this as the player prefab if there is none yet
			if (nm.playerPrefab == null && nm.autoCreatePlayer && playerPrefabCandidate)
			{
				Debug.Log("Automatically adding '<b>" + prefabGO.name + "</b>' to the NetworkManager as the <b>playerPrefab</b>. If this isn't desired, assign your the correct prefab to the Network Manager, or turn off Auto Create Player in the NetworkManager.");
				nm.playerPrefab = prefabGO;
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			NetAdapterTools.EnsureNMPlayerPrefabIsLocalAuthority(nm);
			return true;
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTNetAdapter))]
	[CanEditMultipleObjects]
	public class NSTNetAdapterEditor : NSTHeaderEditorBase
	{
		public override void OnEnable()
		{
			headerName = HeaderAnimatorAddonName;
			headerColor = HeaderAnimatorAddonColor;
			base.OnEnable();
		}
	}
#endif

}
