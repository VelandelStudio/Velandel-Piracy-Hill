//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities;
using emotitron.Network.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{

	public enum FactorBoundsOn { EnableDisable, AwakeDestroy}
	/// <summary>
	/// Put this object on the root of a game map. It needs to encompass all of the areas the player is capable of moving to.
	/// The object must contain a MeshRenderer in order to get the bounds.
	/// Used by the NetworkSyncTransform to scale Vector3 position floats into integers for newtwork compression.
	/// </summary>
	//[ExecuteInEditMode]
	[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.4n2gizaw79m0")]
	[AddComponentMenu("Network Sync Transform/NST Map Bounds")]
	public class NSTMapBounds : MonoBehaviour
	{
		//public enum BoundsTools { Both, MeshRenderer, Collider }
		public bool includeChildren = true;

		[Tooltip("Awake/Destroy will consider a map element into the world size as long as it exists in the scene (You may need to wake it though). Enable/Disable only factors it in if it is active.")]
		[HideInInspector]
		public BoundsTools.BoundsType factorIn = BoundsTools.BoundsType.Both;
		
		// sum of all bounds (children included)
		[HideInInspector] public Bounds myBounds;
		[HideInInspector] public int myBoundsCount;

		// All bounds accounted for (in case there are more than one active Bounds objects
		private static Bounds _combinedWorldBounds;
		/// <summary>
		/// Returns the composite _combinedWorldBounds value, unless there are no active bounds (essentially null)
		/// </summary>
		public static Bounds CombinedWorldBounds
		{
			get
			{
				if (activeMapBoundsObjects == null || ActiveBoundsObjCount == 0)
					return WorldCompressionSettings.Single.defaultWorldBounds;

				return _combinedWorldBounds;
			}
		}
		
		private static List<NSTMapBounds> activeMapBoundsObjects = new List<NSTMapBounds>();
		public static int ActiveBoundsObjCount { get { return activeMapBoundsObjects.Count; }  }
		public static bool muteMessages;

		void Awake()
		{
			// When mapobjects are waking up, this likely means we are seeing a map change. Silence messages until Start().
			muteMessages = true;
			CollectMyBounds();
		}

		public void CollectMyBounds()
		{
			myBounds = BoundsTools.CollectMyBounds(gameObject, factorIn, out myBoundsCount, true, false);

			if (myBoundsCount > 0 && enabled)
			{
				if (!activeMapBoundsObjects.Contains(this))
					activeMapBoundsObjects.Add(this);
			}
			else
			{
				if (activeMapBoundsObjects.Contains(this))
					activeMapBoundsObjects.Remove(this);
			}

		}
		private void Start()
		{
			muteMessages = false;
		}

		private void OnEnable()
		{
			FactorInBounds(true);
		}

		private static bool isShuttingDown;

		void OnApplicationQuit()
		{
			muteMessages = true;
			isShuttingDown = true;
		}

		private void OnDisable()
		{
			FactorInBounds(false);
		}

		private void FactorInBounds(bool b)
		{
			if (this == null)
				return;

			if (b)
			{
				if (!activeMapBoundsObjects.Contains(this))
					activeMapBoundsObjects.Add(this);
			}
			else
			{
				activeMapBoundsObjects.Remove(this);
			}

			RecalculateWorldCombinedBounds();

			// Notify affected classes of the world size change.
			//if (isInitialized && Application.isPlaying)
				UpdateWorldBounds(); // isInitialized is to silence startup log messages
		}

		public static void ResetActiveBounds()
		{
			activeMapBoundsObjects.Clear();
		}

		private static bool warnedOnce;
		/// <summary>
		/// Whenever an instance of NSTMapBounds gets removed, the combinedWorldBounds needs to be rebuilt with this.
		/// </summary>
		public static void RecalculateWorldCombinedBounds()
		{
			// dont bother with any of this if we are just shutting down.
			if (isShuttingDown)
				return;

			if (activeMapBoundsObjects.Count == 0)
			{
				DebugX.LogWarning("There are now no active NSTMapBounds components in the scene.", !warnedOnce);
				warnedOnce = true;
				return;
			}

			warnedOnce = false;

			_combinedWorldBounds = activeMapBoundsObjects[0].myBounds;
			for (int i = 1; i < activeMapBoundsObjects.Count; i++)
			{
				_combinedWorldBounds.Encapsulate(activeMapBoundsObjects[i].myBounds);
			}
		}

		public static void UpdateWorldBounds(bool mute = false)
		{
			// No log messages if commanded, if just starting up, or just shutting down.
			WorldVectorCompression.SetWorldRanges(CombinedWorldBounds, muteMessages || mute);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTMapBounds))]
	[CanEditMultipleObjects]
	public class NSTMapBoundsEditor : Network.NST.NSTHelperEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var _target = (NSTMapBounds)target;

			_target.factorIn = (BoundsTools.BoundsType)EditorGUILayout.EnumPopup("Factor In", _target.factorIn);

			EditorGUILayout.HelpBox(
				"Contains " + _target.myBoundsCount + " bound(s) objects:\n" +
				"Center: " + _target.myBounds.center + "\n" +
				"Size: " + _target.myBounds.size, 
				MessageType.None);

			WorldCompressionSettings.Single.DrawGui(target, true, false);
		}

		/// <summary>
		/// Completely refinds and inventories ALL NstMapBounds in the scene, rather than dicking around trying to be efficient. 
		/// This is editor only so just brute force will do... because I don't see an 'efficient' way.
		/// </summary>
		/// <returns></returns>
		public static string WorldBoundsSummary()
		{
			NSTMapBounds.ResetActiveBounds();

			// Find every damn NSTMapBounds in the scene currently and get its bounds
			NSTMapBounds[] all = Object.FindObjectsOfType<NSTMapBounds>();

			foreach (NSTMapBounds mb in all)
				mb.CollectMyBounds();

			NSTMapBounds.RecalculateWorldCombinedBounds();
			NSTMapBounds.UpdateWorldBounds(true);

			string str =
				"World Bounds in current scene:\n" +
				((NSTMapBounds.ActiveBoundsObjCount == 0) ?
					("No Active NSTMapBounds - will use default.\n") :
					("(" + NSTMapBounds.ActiveBoundsObjCount + " NSTMapBound(s) combined):\n")
					) +

				"Center: " + NSTMapBounds.CombinedWorldBounds.center + "\n" +
				"Size: " + NSTMapBounds.CombinedWorldBounds.size + "\n\n" +

				"Root position keyframes will use:";

			for (BitCullingLevel bcl = 0; bcl < BitCullingLevel.DropAll; bcl++ )
				str += "\n\n" +
					"Culling Level: " + System.Enum.GetName(typeof(BitCullingLevel), bcl) + "\n" +
					"x: " + WorldVectorCompression.axisRanges[0].BitsAtCullLevel(bcl) + " bits, " +
					"y: " + WorldVectorCompression.axisRanges[1].BitsAtCullLevel(bcl) + " bits, " +
					"z: " + WorldVectorCompression.axisRanges[2].BitsAtCullLevel(bcl) + " bits, ";

			return str;

		}
	}

#endif
}

