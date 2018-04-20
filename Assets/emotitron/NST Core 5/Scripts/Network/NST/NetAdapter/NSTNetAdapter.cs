﻿//Copyright 2018, Davin Carten, All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	public struct PlayerInitData
	{
		public uint nstId;
	}

	/// <summary>
	/// This class contains the abstracted methods for different networking libraries. 
	/// This adapter is for Photon PUN.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(PhotonView))]

	public class NSTNetAdapter : Photon.PunBehaviour //, INstNetAdapter
	{
		public const string ADAPTER_NAME = "PUN";
		public static NetworkLibrary NetLibrary { get { return NetworkLibrary.PUN; } }

		PhotonView pv;
		NetworkSyncTransform nst;
		NSTSettings nstSettings;

		// callback interfaces... collected on awake from all children on this gameobject, and can be subcribed to as well.
		[HideInInspector] public List<INetEvents> iNetEvents = new List<INetEvents>();
		//[HideInInspector] public List<IOnStartServer> iOnStartServer = new List<IOnStartServer>();
		//[HideInInspector] public List<IOnStartClient> iOnStartClient = new List<IOnStartClient>();
		[HideInInspector] public List<IOnConnect> iOnConnect = new List<IOnConnect>();
		[HideInInspector] public List<IOnStartLocalPlayer> iOnStartLocalPlayer = new List<IOnStartLocalPlayer>();
		[HideInInspector] public List<IOnNetworkDestroy> iOnNetworkDestroy = new List<IOnNetworkDestroy>();
		[HideInInspector] public List<IOnStartAuthority> iOnStartAuthority = new List<IOnStartAuthority>();
		[HideInInspector] public List<IOnStopAuthority> iOnStopAuthority = new List<IOnStopAuthority>();
		[HideInInspector] public List<IOnStart> iOnStart = new List<IOnStart>();

		public bool IsServer { get { return MasterNetAdapter.ServerIsActive; } }
		public bool IsLocalPlayer { get { return pv.isMine; } } // isLocalPlayer; } }
		public bool IsMine { get { return pv.isMine; } }

		public uint NetId { get { return (uint)pv.viewID; } }
		public int ClientId { get { return pv.ownerId; } }

		private uint _nstIdSyncvar;
		public uint NstIdSyncvar { get { return _nstIdSyncvar; } set {  _nstIdSyncvar = value; } }

		// cached values
		public AuthorityModel authorityModel;

		///// <summary> Does this client have authority over all aspects of this networked object (rather than just movement). Determined by the authority model
		///// selected in MasterSettings.</summary>

		public bool IAmActingAuthority
		{
			get {

				if (authorityModel == AuthorityModel.ServerAuthority)
					if (PhotonNetwork.isMasterClient)
						return true;

				if (authorityModel == AuthorityModel.OwnerAuthority)
					if (pv.isMine)
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
			pv = GetComponent<PhotonView>();
			nst = GetComponent<NetworkSyncTransform>();

			if (pv.viewID == 0)
				DebugX.LogError("You appear to have an 'NetworkSyncTransform' on instantiated object '" + name + "', but that object has NOT been network spawned. " +
					"Only use NST on objects you intend to spawn normally from the server using PhotonNetwork.Instantiate(). (Projectiles for example probably don't need to be networked objects).", true, true);

			authorityModel = (AuthorityModel)NetLibrarySettings.Single.defaultAuthority;

			CollectCallbackInterfaces();

		}
		private void Start()
		{
			foreach (INetEvents cb in iNetEvents)
				cb.OnStart();

			foreach (IOnStart cb in iOnStart)
				cb.OnStart();
		}

		public override void OnConnectedToMaster()
		{
			foreach (INetEvents cb in iNetEvents)
				cb.OnConnect(ServerClient.Master);

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Master);
		}

		//public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		//{

		//}

		// Detect changes in ownership
		public override void OnOwnershipTransfered(object[] viewAndPlayers)
		{
			Debug.Log(pv.viewID + " <b>OnOwnershipTransfered</b> " + PhotonNetwork.isMasterClient + " " + PhotonNetwork.isNonMasterClientInRoom);

			PhotonView changedView = viewAndPlayers[0] as PhotonView;

			if (changedView != pv)
				return;

			if (changedView.isMine)
			{

				if (iNetEvents != null)
					foreach (INetEvents cb in iNetEvents)
						cb.OnStartAuthority();

				if (iOnNetworkDestroy != null)
					foreach (IOnStartAuthority cb in iOnStartAuthority)
						cb.OnStartAuthority();
			}
			else
			{
				if (iNetEvents != null)
					foreach (INetEvents cb in iNetEvents)
						cb.OnStopAuthority();

				if (iOnNetworkDestroy != null)
					foreach (IOnStopAuthority cb in iOnStopAuthority)
						cb.OnStopAuthority();
			}
		}
		
		// TODO this generates a little garbage
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			// If this is the first nst this client has spawned, call it the local player
			if (pv.isMine && !NSTTools.localPlayerNST)
				NSTTools.localPlayerNST = nst;

			if (pv.isMine)// info.photonView.isMine)
			{

				foreach (INetEvents cb in iNetEvents)
					cb.OnStartLocalPlayer();

				foreach (IOnStartLocalPlayer cb in iOnStartLocalPlayer)
					cb.OnStartLocalPlayer();

			}
		}
		
		public override void OnDisconnectedFromPhoton()
		{
			if (iNetEvents != null)
				foreach (INetEvents cb in iNetEvents)
					cb.OnNetworkDestroy();

			if (iOnNetworkDestroy != null)
				foreach (IOnNetworkDestroy cb in iOnNetworkDestroy)
					cb.OnNetworkDestroy();
		}

		public void SendBitstreamToOwner(ref UdpBitStream bitstream)
		{
			Debug.LogError("Not Implemented");
		}

		/// <summary>
		/// Remove the adapter and the NetworkIdentity/View from an object
		/// </summary>
		/// <param name="nst"></param>
		public static void RemoveAdapter(NetworkSyncTransform nst)
		{
			NSTNetAdapter na = nst.GetComponent<NSTNetAdapter>();
			PhotonView pv = nst.GetComponent<PhotonView>();

			if (na)
				DestroyImmediate(na);

			if (pv)
				DestroyImmediate(pv);
		}


#if UNITY_EDITOR

		/// <summary>
		/// Add a network adapter and the NetworkIdenity/NetworkView as needed. PhotonView needs to be added before runtime.
		/// If added at runtime, it may get added AFTER network events fire.
		/// </summary>
		public static void EnsureHasEntityComponentForNetLib(GameObject go, bool playerPrefabCandidate = true)
		{
			go.transform.root.gameObject.EnsureRootComponentExists<PhotonView>(false);
			AddAsRegisteredPrefab(go, playerPrefabCandidate);
		}
		/// <summary>
		/// Tries to register this NST as the player prefab (if there is none currently set), after doing some checks to make sure it makes sense to.
		/// </summary>
		public static void AddAsRegisteredPrefab(GameObject go, bool playerPrefabCandidate, bool silence = false)
		{
			// Doesn't apply to PUN
			PUNSampleLauncher punl = UnityEngine.Object.FindObjectOfType<PUNSampleLauncher>();

			if (punl && !punl.playerPrefab && playerPrefabCandidate)
			{
				DebugX.LogWarning("Adding " + go.name + " as the player prefab to " + typeof(PUNSampleLauncher).Name);
				GameObject parprefab = (GameObject)PrefabUtility.GetPrefabParent(go);
				punl.playerPrefab = parprefab ? parprefab : go;
			}
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
