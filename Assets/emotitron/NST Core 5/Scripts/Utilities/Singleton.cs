//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace emotitron
{
	
	

	public abstract class Singleton<T> : MonoBehaviour where T : Component
	{
		public static T single;

		/// <summary>
		/// Slower get for the field single, but will look for it in the scene if null. Use this for editor references to single - but not for hotpath references.
		/// Unlike EnsureExistsInScene, Single will not create a singleton if it is missing.
		/// </summary>
		public static T Single { get { return (single != null) ? single : FindObjectOfType<T>(); } }

	
		protected static bool isShuttingDown;

		protected virtual void Awake()
		{
			isShuttingDown = false;

			if (single != null && single != this)
			{
				DebugX.LogWarning("Enforcing " + typeof(T) + " singleton. Multiples found.");
				Destroy(gameObject);
			}
			single = this as T;
		}

		protected virtual void OnApplicationQuit()
		{
			isShuttingDown = true;
		}
#if UNITY_EDITOR

		public static T EnsureExistsInScene(GameObject go, bool isExpanded = true)
		{
			// if we already have set the singleton... use it.
			if (single != null)
				return single;

			// if not, try to find the object in scene and return it.
			single = FindObjectOfType<T>();

			if (single != null)
				return single;

			// Don't attempt to make a new one if we are shutting down a game.
			if (isShuttingDown && Application.isPlaying)
				return single;

			DebugX.LogWarning(!DebugX.logWarnings ? null : ("<b>No " + (typeof(T)) + " found in scene. Adding one with default settings.</b> You probably want to edit the settings yourself."));

			single = go.AddComponent<T>();


			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(single, isExpanded);

			return single;
		}

		/// <summary>
		/// Call this at the awake of other NST components to make sure this Singleton exists in the scene. If not creates
		/// one on an object with the name given, and returns it.
		/// </summary>
		public static T EnsureExistsInScene(string goName, bool isExpanded = true)
		{
			// if we already have set the singleton... use it.
			if (single != null)
				return single;

			// if not, try to find the object in scene and return it.
			single = FindObjectOfType<T>();

			if (single != null)
				return single;

			// Don't attempt to make a new one if we are shutting down a game.
			if (isShuttingDown && Application.isPlaying)
				return single;

			
			// See if the gameobject this normally go on exists, and the component is just missing
			//GameObject go = GameObject.Find(goName);

			GameObject go = PrefabUtility.InstantiatePrefab(Resources.Load(goName, typeof(GameObject))) as GameObject;

			// The prefab no longer exists (deletedy by user? Shame on you)
			if (go == null)
			{
				go = new GameObject(goName);
				single = go.AddComponent<T>();
			}

			else
				single = go.GetComponent<T>();

			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(single, isExpanded);

			return single;
		}
#endif


	}
}

