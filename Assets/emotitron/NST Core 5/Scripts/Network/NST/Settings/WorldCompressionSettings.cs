//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{

#if UNITY_EDITOR
	[HelpURL(HELP_URL)]
#endif

	public class WorldCompressionSettings : SettingsScriptableObject<WorldCompressionSettings>
	{

		[Range(10, 1000)]
		[Tooltip("Indicate the minimum resolution of any axis of compressed root positions (Subdivisions per 1 Unit). Increasing this needlessly will increase your network traffic. Decreasing it too much will result in objects moving in visible rounded increments.")]
		public int minPosResolution = 100;

		[Tooltip("If no NSTMapBounds are found in the scene, this is the size of the world that will be used by the root position compression engine.")]
		public Bounds defaultWorldBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(2000, 100, 2000));

		public override string SettingsName { get { return "World Compression Settings"; } }


#if UNITY_EDITOR

		public const string HELP_URL = "https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#bookmark=kix.59wxxn84tocg";
		public override string HelpURL { get { return HELP_URL; } }

		public override bool DrawGui(object target, bool asFoldout, bool includeScriptField)
		{
			bool isExpanded = base.DrawGui(target, asFoldout, includeScriptField);

			if (isExpanded)
				EditorGUILayout.HelpBox(NSTMapBoundsEditor.WorldBoundsSummary(), MessageType.None);

			return isExpanded;
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(WorldCompressionSettings))]
	public class WorldCompressionSettingsEditor : SettingsSOBaseEditor<WorldCompressionSettings>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			WorldCompressionSettings.Single.DrawGui(target, false, true);
		}
	}
#endif
}

