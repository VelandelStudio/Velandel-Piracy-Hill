//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// The actual Settings exist in the NSTMasterSettings scriptable object.
	/// </summary>

	
	[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.fq7c7pcliv4e")]
	[AddComponentMenu("NST/NST Settings")]
	[System.Serializable]
	[ExecuteInEditMode]
	public class NSTSettings : Singleton<NSTSettings>
	{
		public const string DEFAULT_GO_NAME = "NST Settings";

#if UNITY_EDITOR
		private bool needsEditorModePostAwakeCheck;
#endif
		protected override void Awake()
		{
#if UNITY_EDITOR
			// Don't run awake if this is not runtime.
			if (!Application.isPlaying)
			{
				needsEditorModePostAwakeCheck = true;
				return;
			}
#endif
			// Initialize all SettingSOs
			var sos = Resources.LoadAll<SettingsScriptableObjectBase>("");

			foreach (var so in sos)
				so.Initialize();

			base.Awake();
			Initialize();
		}

#if UNITY_EDITOR
		private void Update()
		{
			if (Application.isPlaying)
				return;

			if (EditorApplication.isCompiling)
				return;

			if (!needsEditorModePostAwakeCheck)
				return;

			//Destroy the existing Master so it can be readded, to ensure it hasn't been messed up by a library change.
			//NetAdapterTools.RemoveComponentTypeFromScene<NSTMaster>(true);

			//FindMissingScripts.DestroyMissingComponentOnRoot(FindObjectOfType<MasterNetAdapter>().gameObject);
			NetAdapterTools.RemoveUnusedNetworkManager();
			NetAdapterTools.TryToAddDependenciesEverywhere();
			NetAdapterTools.GetNetworkManager(true);
			NetAdapterTools.CopyPlayerPrefabFromPUNtoOthers();
			NetAdapterTools.EnsureNMPlayerPrefabIsLocalAuthority();
			NetAdapterTools.EnsureSceneNetLibDependencies(false);

			needsEditorModePostAwakeCheck = false;
		}
#endif

#if UNITY_EDITOR

		public static NSTSettings EnsureExistsInScene()
		{
			return EnsureExistsInScene(DEFAULT_GO_NAME);
		}
#endif

		private static bool initialized;

		public void Initialize()
		{
			if (initialized)
				return;

			initialized = true;

			// Eliminate any NSTs that are in the scene at startup (they are just trash left by the developer and are not server spawned.
			if (Application.isPlaying && MasterNetAdapter.NetworkLibrary == NetworkLibrary.UNET)
				NSTTools.DestroyAllNSTsInScene();

			/// If enough network lib specific things show up here, I may need to make a new start adapter for network
			/// but for not, just keeping this here.
			// Ensure that UNET is sending our packet immediately.
			if (MasterNetAdapter.NetworkLibrary == NetworkLibrary.UNET)
			{
				// this is here so we can access the NM out of play mode
				if (UnityEngine.Networking.NetworkManager.singleton == null)
					UnityEngine.Networking.NetworkManager.singleton = UnityEngine.Object.FindObjectOfType<UnityEngine.Networking.NetworkManager>();

				if (UnityEngine.Networking.NetworkManager.singleton != null)
					UnityEngine.Networking.NetworkManager.singleton.connectionConfig.SendDelay = 0;
			}

			// Not ideal code to prevent hitching issues with vsync being off - ensures a reasonable framerate is being enforced
			if (QualitySettings.vSyncCount == 0)
			{
				if (Application.targetFrameRate <= 0)
					Application.targetFrameRate = 60;
				else
					Application.targetFrameRate = Application.targetFrameRate;

				DebugX.LogWarning(!DebugX.logWarnings ? null :
					("VSync appears to be disabled, which can cause some problems with Networking. \nEnforcing the current framerate of " + Application.targetFrameRate +
					" to prevent hitching. Enable VSync or set 'Application.targetFrameRate' as desired if this is not the framerate you would like."));
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTSettings))]
	[CanEditMultipleObjects]
	public class NSTSettingsEditor : NSTHeaderEditorBase
	{

		public override void OnEnable()
		{
			headerName = HeaderSettingsName;
			headerColor = HeaderSettingsColor;
			base.OnEnable();


			NetAdapterTools.TryToAddDependenciesEverywhere();
			NetAdapterTools.EnsureSceneNetLibDependencies(true);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			NetLibrarySettings.Single.DrawGui(target, true, false);

			EditorGUILayout.Space();
			HeaderSettings.Single.DrawGui(target, true, false);

			EditorGUILayout.Space();
			WorldCompressionSettings.Single.DrawGui(target, true, false);

			// Use reflection to determine if Rewind Add-on exists, and if so incorporate it into the Settings GUI
			Type t = Type.GetType("emotitron.Network.NST.RewindSettings, Assembly-CSharp");
			if (t != null)
			{
				EditorGUILayout.Space();
				MethodInfo methodInfo = t.GetMethod("StaticDrawGui");
				methodInfo.Invoke(null, new object[1] { target });
			}

			EditorGUILayout.Space();
			HitGroupSettings.Single.DrawGui(target, true, false);

			EditorGUILayout.Space();
			DebuggingSettings.Single.DrawGui(target, true, false);

			// Try should only fail if we are changed network library and the editor is compiling the changes... don't even try to update.
			try
			{
				serializedObject.Update();
			}
			// during lib change, serializedObject becomes null
			catch { return; }

			serializedObject.ApplyModifiedProperties();
		}

		private static void AddMapBounds()
		{
			MeshRenderer[] renderers = Selection.activeGameObject.GetComponents<MeshRenderer>();
			if (renderers.Length == 0)
			{
				Debug.LogWarning("NSTMapBounds added to an item that has no Mesh Renderers in its tree.");
			}
			Selection.activeGameObject.AddComponent<NSTMapBounds>();
		}
	}
#endif
}
