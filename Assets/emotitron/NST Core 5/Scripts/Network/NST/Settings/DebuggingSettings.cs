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

	public class DebuggingSettings : SettingsScriptableObject<DebuggingSettings>
	{
	
		[Tooltip("Turn this off for your production build to reduce pointless cpu waste.")]
		public bool logWarnings = true;

		[Tooltip("Spam your log with all kinds of info you may or may not care about. Turn this off for your production build to reduce pointless cpu waste.")]
		public bool logTestingInfo = false;

		[Tooltip("Put itemized summaries of update bandwidth usage into the Debug.Log")]
		public bool logDataUse = false;

		public override string SettingsName { get { return "Debugging Settings"; } }

		public override void Initialize()
		{
			// Let the conditional debug know what to show
			DebugX.logInfo = Single.logTestingInfo;
			DebugX.logWarnings = Single.logWarnings;
			DebugX.logErrors = true;
		}

#if UNITY_EDITOR

		public const string HELP_URL = "https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#bookmark=kix.10q089trf9ig";
		public override string HelpURL { get { return HELP_URL; } }

		public override bool DrawGui(object target, bool isAsset, bool includeScriptField = true)
		{
			expandeds[target] = base.DrawGui(target, isAsset, includeScriptField);

			if (expandeds[target])
				EditorGUILayout.HelpBox("All log options are for editor only and will be conditionally purged from all builds. No need to disable these for releases.", MessageType.None);

			return expandeds[target];
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(DebuggingSettings))]
	public class DebuggingSettingsEditor : SettingsSOBaseEditor<DebuggingSettings>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			DebuggingSettings.Single.DrawGui(target, false, true);
		}
	}
#endif
}

