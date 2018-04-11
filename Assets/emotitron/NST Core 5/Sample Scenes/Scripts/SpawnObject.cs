﻿using UnityEngine;
using emotitron.Network.NST;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatic spawn points for testing. This makes use of the NSTMasterBehaviour - removing the need for a NetworkIdentity to
/// detect network starts.
/// </summary>
[ExecuteInEditMode]
public class SpawnObject : NSTMasterBehaviour, IOnConnect, IOnJoinRoom
{
	public GameObject prefab;

#if UNITY_EDITOR
	void Awake()
	{
		if (!Application.isPlaying)
			needsEditorModePostAwakeCheck = true;
	}

	bool needsEditorModePostAwakeCheck;

	void Update()
	{
		if (Application.isPlaying)
			return;

		if (EditorApplication.isCompiling)
			return;

		if (!NetAdapterTools.DependenciesHaveBeenAddedEverywhere)
			return;

		// wait an update cycle before adding to spawnlist
		if (needsEditorModePostAwakeCheck)
			AddToSpawnList();

		needsEditorModePostAwakeCheck = false;
	}

	public void AddToSpawnList()
	{
		if (prefab)
			//if (PrefabUtility.GetPrefabType(prefab) != PrefabType.Prefab)
			//{
			//	Debug.Log("GameObject Prefab required for " + typeof(SpawnObject).Name + ".");
			//	prefab = null;
			//}
			//else
			//{
				NSTNetAdapter.AddAsRegisteredPrefab(prefab, false, false);
			//}
	}
	
#endif

	// Callback used by UNET
	public void OnConnect(ServerClient svrclnt)
	{
		if (svrclnt == ServerClient.Server)
		{
			MasterNetAdapter.Spawn(prefab, transform.position, transform.rotation, null);
		}
	}

	// Callback used by PUN
	public void OnJoinRoom()
	{
		if (MasterNetAdapter.ServerIsActive)
			MasterNetAdapter.Spawn(prefab, transform.position, transform.rotation, null);
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(SpawnObject))]
	[CanEditMultipleObjects]
	public class SpawnObjectEditor : NSTSampleHeader
	{
		private SpawnObject _target;

		public override void OnEnable()
		{
			headerName = HeaderAnimatorAddonName;
			headerColor = HeaderAnimatorAddonColor;
			base.OnEnable();

			_target = (SpawnObject)target;

			_target.AddToSpawnList();
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			if (EditorGUI.EndChangeCheck())
			{
				_target.AddToSpawnList();
			}
		}
	}
#endif

}
