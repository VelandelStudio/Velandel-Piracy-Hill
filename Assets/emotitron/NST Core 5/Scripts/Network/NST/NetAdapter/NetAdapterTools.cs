﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.GUIUtilities;
using System.Collections.Generic;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// Utilities for automatically adding the required components to scene and networked objects.
	/// </summary>
	public static class NetAdapterTools
	{
		/// <summary>
		/// Add NSTRewindEngine to the NST if Rewind Addon is present, and authority model calls for rewind.
		/// </summary>
		/// <param name="nst"></param>
		public static void AddRewindEngine(this NetworkSyncTransform nst)
		{
			// Try to add/remove the rewind engine by name (to avoid errors if they aren't installed)
			System.Type t = System.Type.GetType("emotitron.Network.NST.NSTRewindEngine");

			if (t != null)
			{
				if (NetLibrarySettings.Single.defaultAuthority == DefaultAuthority.ServerAuthority)
				{
					if (!nst.GetComponent(t))
					{
						nst.gameObject.AddComponent(t);
#if UNITY_EDITOR
						if (!Application.isPlaying)
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif
					}
				}
			}
		}

#if UNITY_EDITOR

		/// <summary>
		/// Ensures NSTSettings as well as (NSTMaster/MasterAdapter/NetworkIdentity) exist in the scene.
		/// </summary>
		/// <returns></returns>
		public static void EnsureSceneNetLibDependencies(bool immediate = true)
		{
			if (Application.isPlaying)
				return;

			NSTSettings.EnsureExistsInScene();

			// If a post-recompile rebuild of dependencies is pending... do it now.
			TryToAddDependenciesEverywhere();

			if (MasterNetAdapter.NetLib == NetworkLibrary.UNET)
			{
				GetNetworkManager(true);
				CopyPlayerPrefab();
			}

			NSTMaster.EnsureExistsInScene(NSTMaster.DEFAULT_GO_NAME);
			NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
		}
		
		/// <summary>
		/// Ensure all required dependencies are added for this NST to work. Can be called often in edit mode, and should be. Returns false
		/// if a DestroyImmediate was invoked.
		/// </summary>
		/// <param name="nst"></param>
		/// <param name="silence"></param>
		public static void EnsureAllNSTDependencies(this NetworkSyncTransform nst, SerializedObject serializedObject, bool silence = false)
		{
			EnsureSceneNetLibDependencies();

			if (Application.isPlaying)
				return;

			// If user tried to put NST where it shouldn't be... remove it and all of the required components it added.
			if (nst.transform.parent != null)
			{
				DebugX.LogError("NetworkSyncTransform must be on the root of an prefab object.");
				nst.nstElementsEngine = nst.transform.GetComponent<NSTElementsEngine>();

				NSTNetAdapter.RemoveAdapter(nst);

				Object.DestroyImmediate(nst);

				if (nst.nstElementsEngine != null)
				{
					Object.DestroyImmediate(nst.nstElementsEngine);
					EditorUtility.SetDirty(nst.gameObject);
				}
				return;
			}

			nst.nstElementsEngine = NSTElementsEngine.EnsureExistsOnRoot(nst.transform, false);

			nst.na = EditorUtils.EnsureRootComponentExists<NSTNetAdapter>(nst.gameObject, false);

			AddRewindEngine(nst);

			//// Add this NST to the prefab spawn list (and as player prefab if none exists yet) as an idiot prevention
			NSTNetAdapter.AddAsRegisteredPrefab(nst.gameObject, true); //NSTNetAdapter.AddAsRegisteredPrefab(nst.gameObject, silence);
			return;
		}

		/// <summary>
		/// Check if the Net Library selected in mastersettings doesn't match the library of the adapters. Whill change to the library
		/// in MasterSettings if not.
		/// </summary>
		/// <param name="newLib"></param>
		public static bool ChangeLibraries()
		{
			return ChangeLibraries(NetLibrarySettings.Single.networkLibrary);
		}

		/// <summary>
		/// Initiate the Library change process.
		/// </summary>
		/// <param name="newLib"></param>
		public static bool ChangeLibraries(NetworkLibrary newLib)
		{
			// Don't do anything if the adapters already are correct
			if (newLib == MasterNetAdapter.NetworkLibrary && newLib == NSTNetAdapter.NetLibrary)
				return true;

			if (newLib == NetworkLibrary.PUN && !PUN_Exists)
			{
				Debug.LogError("Photon PUN does not appear to be installed (Cannot find the PhotonNetwork assembly). Be sure it is installed from the asset store for this project.");
				return false;
			}

			if (!EditorUtility.DisplayDialog("Change Network Library To " + System.Enum.GetName(typeof(NetworkLibrary), newLib) + "?",
				"Changing libraries is a very messy brute force operation (you may see compile errors and may need to restart Unity). " +
				"Did you really want to do this, or are you just poking at things to see what they do?", "Change Library", "Cancel"))
			{
				return false;
			}

			Debug.Log("Removing current adapters from game objects for Network Library change ...");
			PurgeLibraryReferences();

			// Close and reopen the current scene to remove the bugginess of orphaned scripts.
			var curscene = EditorSceneManager.GetActiveScene();
			var curscenepath = curscene.path;

			if (EditorUtility.DisplayDialog("Save Scene Before Reload?",
				"Scene must be reloaded to complete the purging of old library adapters. Would you like to save this scene?", "Save Scene", "Don't Save"))
				EditorSceneManager.SaveScene(curscene);

			// force a scene close to eliminate weirdness
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

			OverwriteAdapters(newLib);

			EditorUtility.DisplayDialog("Touch Nothing!",
				"Wait for the compiling animation in the bottom right of Unity to stop before doing anything. Touching NST related assets will result in broken scripts and errors.", "I Won't Touch, Promise.");


			// Flag the need for a deep global find of NSTs and NSTMasters in need of adapters
			DebugX.LogWarning("Add dependencies pending. Clicking on any NST related object in a scene " +
				"will trigger the final steps of the transition to " + newLib + ". You may need to select the " +
				"Player Prefab in the scene or asset folder in order to make it the default player object.", true, true);
			DependenciesHaveBeenAddedEverywhere = false;

			return true;
		}


		// This should only be run very rarely. Forced to false after a library change.
		public static bool DependenciesHaveBeenAddedEverywhere;

		/// <summary>
		/// Deep find and add of adapters to all NSTs objects in assets. Deferred actions that need to happen after a compile following library change.
		/// </summary>
		public static void TryToAddDependenciesEverywhere()
		{
			if (DependenciesHaveBeenAddedEverywhere)
				return;

			DebugX.Log("Adding NST Entities in all Assets");
			MasterNetAdapter.AddNstEntityComponentsEverywhere();
			
			// Now that prefabs in assets have been altered, make sure any scene objects revert to those prefabs
			RevertPrefabsInSceneWithComponentType<NetworkSyncTransform>();

			DependenciesHaveBeenAddedEverywhere = true;
		}

		//public static void EnsureNSTMasterConforms()
		//{
		//	GameObject nstMasterPrefab = Resources.Load("NST Master", typeof(GameObject)) as GameObject;
		//	nstMasterPrefab.EnsureRootComponentExists<NSTMaster>();
		//	nstMasterPrefab.EnsureRootComponentExists<MasterNetAdapter>();

		//	if (MasterNetAdapter.NetLib == NetworkLibrary.UNET)
		//		nstMasterPrefab.EnsureRootComponentExists<NetworkIdentity>();
		//	else
		//	{
		//		NetworkIdentity ni = nstMasterPrefab.GetComponent<NetworkIdentity>();
		//		if (ni)
		//			Object.DestroyImmediate(ni);
		//	}
		//}

		//public static T EnsureContainsComponent<T>(this GameObject go) where T : Component
		//{
		//	T comp = go.GetComponent<T>();
		//	if (!comp)
		//		comp = go.AddComponent<T>();
		//	return comp;
		//}

		/// <summary>
		/// Return true if that library successfully was activated, or was already activated. False indicates a fail, to notify the enum
		/// selector to not make the change and stay in the current mode.
		/// </summary>
		/// <param name="netlib"></param>
		/// <returns></returns>
		public static bool OverwriteAdapters(NetworkLibrary newLib)
		{
			// Test to see if this library is already what is active
			if (MasterNetAdapter.NetworkLibrary != newLib)

			// Don't try to change to PUN if the PUN classes are missing (not installed)
			if (newLib == NetworkLibrary.PUN)
				if (!PUN_Exists)
				{
					DebugX.LogError("Photon PUN does not appear to exist in this project. You will need to download Photon PUN from the Unity Asset Store and import it into this project in order to have this option.", true, true);
					return false;
				}

			return CopyUncompiledAdapters(newLib);
		}

		/// <summary>
		/// Test if the photon library appears to exist in this project.
		/// </summary>
		public static bool PUN_Exists
		{
			get
			{
				System.Type type = System.Type.GetType("PhotonNetwork");
				return type != null;
			}

		}

		/// <summary>
		/// Get the path to the named asset (just the first found, there should be no multiples). Will return empty if not found.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		private static string GetPathFromAssetName(string a)
		{
			var id = AssetDatabase.FindAssets(a);
			return (id.Length == 0) ? "" : AssetDatabase.GUIDToAssetPath(id[0]);
		}

		/// <summary>
		/// Overwrites the Adapter.CS files with the selected library specific version.
		/// </summary>
		/// <param name="newLib"></param>
		/// <returns>Returns true if successful.</returns>
		private static bool CopyUncompiledAdapters(NetworkLibrary newLib)// string libSuffix)
		{
			bool success = false;

			string libSuffix = (newLib == NetworkLibrary.UNET) ? "UNET" : "PUN";

			var _MA = GetPathFromAssetName("MasterNetAdapter" + libSuffix);
			var _NA = GetPathFromAssetName("NSTNetAdapter" + libSuffix);
			var MA = GetPathFromAssetName("MasterNetAdapter");
			var NA = GetPathFromAssetName("NSTNetAdapter");

			// fail if any of these files were not found.
			if (_MA == "" || _NA == "" || MA == "" || NA == "")
				return false;

			DebugX.Log("Switching to " + libSuffix + " adapters... recompiling should happen automatically.", true, true);

			if (MasterNetAdapter.NetworkLibrary != newLib)
				success |= AssetDatabase.CopyAsset(_MA, MA);

			if (NSTNetAdapter.NetLibrary != newLib)
				success |= AssetDatabase.CopyAsset(_NA, NA);

			return success;
		}

		/// <summary>
		/// Finds all instances of the network adapters in loaded scenes and the assetdatabse. Used prior to a network library change in order to not
		/// create broken scripts where the old adapters were.
		/// </summary>
		public static void PurgeLibraryReferences()
		{
			PurgeTypeFromEverywhere<NSTNetAdapter>();
			PurgeTypeFromEverywhere<MasterNetAdapter>();
			MasterNetAdapter.PurgeLibSpecificComponents();
		}

		private static void RevertPrefabsInSceneWithComponentType<T>() where T : Component
		{
			T[] found = Object.FindObjectsOfType<T>();

			for (int i = 0; i < found.Length; i++)
			{
				PrefabUtility.RevertPrefabInstance(found[i].gameObject);
			}
		}

		/// <summary>
		/// Find all occurances of SearchT in the assetdatabase, and add Component AddT to those gameobjects. Used for adding Adapters to all instances of NST or
		/// future NS components.
		/// </summary>
		/// <typeparam name="SearchT"></typeparam>
		/// <typeparam name="AddT"></typeparam>
		public static void AddComponentWhereverOtherComponentIsFound<SearchT, AddT>() 
			where SearchT : Component where AddT : Component
		{
			string[] guids = AssetDatabase.FindAssets(string.Format("t:GameObject", typeof(SearchT)));
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if (asset.GetComponent<SearchT>())
				{
					if (!asset.GetComponent<AddT>())
						asset.AddComponent<AddT>();
				}
				Resources.UnloadUnusedAssets();
			}
		}

		public static void AddComponentsWhereverOtherComponentIsFound<SearchT, AddT, Add2T>()
			where SearchT : Component where AddT : Component where Add2T : Component
		{
			string[] guids = AssetDatabase.FindAssets(string.Format("t:GameObject", typeof(SearchT)));
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if (asset.GetComponent<SearchT>())
				{
					if (!asset.GetComponent<AddT>())
						asset.AddComponent<AddT>();

					if (!asset.GetComponent<Add2T>())
						asset.AddComponent<Add2T>();
				}
				Resources.UnloadUnusedAssets();
			}
		}

		/// <summary>
		/// Destroy Component T in all of the assetsDatabase, and then any instances in the current scene and loaded memory. This is root components only,
		/// and will not find components on scene objects in unloaded scenes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void PurgeTypeFromEverywhere<T>(bool removeGO = false) where T : Component
		{
			Debug.Log("<b>Purging all " + typeof(T).Name + " components from scene </b>");
			RemoveComponentTypeFromAllAssets<T>(removeGO);
			RemoveComponentTypeFromScene<T>(removeGO);

		}

		/// <summary>
		/// Destroy all instances of Component T in scene/memory.
		/// </summary>
		public static void RemoveComponentTypeFromScene<T>(bool removeGO = false) where T : Component
		{
			T[] found = Object.FindObjectsOfType<T>();

			for (int i = 0; i < found.Length; i++)
			{
				if (removeGO)
					Object.DestroyImmediate(found[i].gameObject);
				else
					Object.DestroyImmediate(found[i]);
			}
		}

		/// <summary>
		/// Find all instances of component T in the entire assetdatabase, and remove it from those objects. This only works for root components.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private static void RemoveComponentTypeFromAllAssets<T>(bool removeGO = false) where T : Component
		{
			string[] guids = AssetDatabase.FindAssets(string.Format("t:GameObject", typeof(T)));
			//Debug.Log("Found " + guids.Length + " " + typeof(T).Name + " in the asset database.");
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				SerializedObject so = new SerializedObject(asset);

				if (asset != null)
				{
					T comp = (asset as GameObject).GetComponent<T>();
					if (comp)
					{
						Debug.Log("Destroying component " + typeof(T).Name + " on go " + asset);

						if(removeGO)
							Object.DestroyImmediate(comp.gameObject, true);
						else
							Object.DestroyImmediate(comp, true);
					}
				}
				so.Update();
			}
			Resources.UnloadUnusedAssets();
		}

		public static void RemovedUnusedNetworkIdentity(GameObject go)
		{
			// Double check to make sure NI doesn't exist if UNET isn't being used. It disables the object and will break other libs.
			if (MasterNetAdapter.NetLib != NetworkLibrary.UNET)
			{
				NetworkIdentity ni = go.GetComponent<NetworkIdentity>();

				if (ni)
				{
					try { Object.DestroyImmediate(ni); }
					catch { try { Object.Destroy(ni); } catch { } }
				}
			}
		
		}
		
		public static NetworkManager GetNetworkManager(bool createMissing = false)
		{
			if (MasterNetAdapter.NetLib != NetworkLibrary.UNET)
			{
				return null;
			}

			if (NetworkManager.singleton == null)
			{
				List<NetworkManager> found = FindObjects.FindObjectsOfTypeAllInScene<NetworkManager>();

				if (found.Count > 0)
					NetworkManager.singleton = found[0];

				else if (createMissing)
				{
					DebugX.LogWarning(!DebugX.logWarnings ? null : ("No NetworkManager in scene. Adding one now."));

					GameObject nmGo = GameObject.Find("Network Manager");

					if (nmGo == null)
						nmGo = new GameObject("Network Manager");

					NetworkManager.singleton = nmGo.AddComponent<NetworkManager>();

					// If we are creating a missing NM, also create a HUD in case user wants that.
					NetworkManagerHUD hud = nmGo.GetComponent<NetworkManagerHUD>();

					if (!hud)
						nmGo.AddComponent<NetworkManagerHUD>();

					// Copy the playerprefab over from pun if it exists.
				}
			}

			return NetworkManager.singleton;
		}

		public static void CopyPlayerPrefab()
		{
			CopyPlayerPrefabFromPUNtoOthers();
			CopyPlayerPrefabFromUNETtoOthers();
		}

		// TODO this is redudant with NSTNetAdapter code for UNET
		public static void CopyPlayerPrefabFromPUNtoOthers()
		{

			PUNSampleLauncher punl = PUNSampleLauncher.Single;
			if (!punl || punl.playerPrefab == null)
				return;

			NSTNetAdapter.AddAsRegisteredPrefab(punl.playerPrefab, true);

			//// Copy player prefab from PUN launcher to NM
			//if (MasterNetAdapter.NetLib == NetworkLibrary.UNET)
			//	NSTNetAdapter.AddAsRegisteredPrefab(punl.playerPrefab, true);

			//// Copy PUN playerPrefab to UNET
			//NetworkManager nm = GetNetworkManager(MasterNetAdapter.NetLib == NetworkLibrary.UNET);
			//if (nm && !nm.playerPrefab)
			//{
			//	Debug.Log("Copying Player Prefab : <b>'" + punl.playerPrefab.name + "'</b> from " + punl.GetType().Name + " to NetworkManager for you.");

			//	NetworkIdentity ni = punl.playerPrefab.GetComponent<NetworkIdentity>();
			//	if (!ni)
			//		ni = punl.playerPrefab.AddComponent<NetworkIdentity>();

			//	// This seemingly pointless code forces the NI to initialize the assetid. For some dumb reason UNET.
			//	ni.assetId.IsValid();

			//	if (nm.playerPrefab != punl.playerPrefab)
			//	{
			//		nm.playerPrefab = punl.playerPrefab;
			//		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			//	}
			//}
		}

		public static void EnsureNMPlayerPrefabIsLocalAuthority()
		{
			EnsureNMPlayerPrefabIsLocalAuthority(GetNetworkManager());
		}

		public static void EnsureNMPlayerPrefabIsLocalAuthority(NetworkManager nm)
		{
			if (nm && nm.playerPrefab)
			{
				NetworkIdentity ni = nm.playerPrefab.GetComponent<NetworkIdentity>();
				if (ni && !ni.localPlayerAuthority)
				{
					ni.localPlayerAuthority = true;
					Debug.Log("Setting 'NetworkIdentity.localPlayerAuthority = true' on prefab '<b>" + nm.playerPrefab.name + "</b>' for you, now that it is registerd as the Player Prefab with the NetworkManager.");
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
		}

		public static void CopyPlayerPrefabFromUNETtoOthers()
		{
			NetworkManager nm = GetNetworkManager();
			if (!nm || nm.playerPrefab == null)
				return;

			PUNSampleLauncher punl = PUNSampleLauncher.Single;
			
			if (punl && !punl.playerPrefab)
			{
				Debug.Log("Copying Player Prefab : <b>'" + nm.playerPrefab.name + "'</b> from NetworkManager to " + punl.GetType().Name + " for you.");
				punl.playerPrefab = nm.playerPrefab;
			}
		}

		//public static NetworkManager EnsureNetworkManagerExists()
		//{
		//	if (NetworkManager.singleton == null)
		//	{
		//		List<NetworkManager> found = FindObjects.FindObjectsOfTypeAllInScene<NetworkManager>();
		//		if (found.Count > 0)
		//		{
		//			NetworkManager.singleton = found[0];
		//		}
		//		else
		//		{
		//			DebugX.LogWarning(!DebugX.logWarnings ? null : ("No NetworkManager in scene. Adding one now."));

		//			GameObject nmGo = GameObject.Find("Network Manager");

		//			if (nmGo == null)
		//				nmGo = new GameObject("Network Manager");

		//			NetworkManager.singleton = nmGo.AddComponent<NetworkManager>();

		//			// Copy over the player prefab from our PUN launcher if there is one.
		//			PUNSampleLauncher punl = PUNSampleLauncher.Single;
		//			if (punl && punl.playerPrefab)
		//			{
		//				DebugX.Log("Copying Player Prefab : <b>'" +punl.playerPrefab.name + "'</b> from " + typeof(PUNSampleLauncher).Name + " to NetworkManager for you.", true, true);
		//				NetworkManager.singleton.playerPrefab = punl.playerPrefab;
		//			}
		//		}
		//	}

		//	// Add a HUD if that is also missing
		//	if (NetworkManager.singleton.gameObject.GetComponent<NetworkManagerHUD>() == null)
		//		NetworkManager.singleton.gameObject.AddComponent<NetworkManagerHUD>();

		//	return NetworkManager.singleton;
		//}

		public static void RemoveUnusedNetworkManager()
		{
			if (MasterNetAdapter.NetLib != NetworkLibrary.UNET)
			{
				Debug.Log("Removing unused NetworkManager from scene.");
				RemoveComponentTypeFromScene<NetworkManager>(true);
			}
		}

		///// <summary>
		///// Checks to see if there is a NetworkManager, and if so, checks to make sure the assigned playerprefab has its ni set to localplayerauthority.
		///// </summary>
		//public static void EnsurePlayerPrefabIsSetLocalAuthority()
		//{
		//	if (MasterNetAdapter.NetLib == NetworkLibrary.UNET)
		//	{
		//		NetworkManager nm = GetNetworkManager();
		//		if (nm && nm.playerPrefab)
		//		{
		//			NetworkIdentity ni = nm.playerPrefab.GetComponent<NetworkIdentity>();
		//			if (ni)
		//				ni.localPlayerAuthority = true;
		//		}
		//	}
		//}
#endif
	}
}
