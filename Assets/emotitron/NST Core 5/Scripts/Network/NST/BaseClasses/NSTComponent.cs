﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// Base class of any NST component that belongs on the networked object. 
	/// (Not the base of global objects that exist in the scene, such as NSTSettings, NSTMapBounds or UIZone).
	/// </summary>
	public abstract class NSTComponent : MonoBehaviour, INstAwake
	{
		[HideInInspector] public NetworkSyncTransform nst;
		[HideInInspector] public NSTNetAdapter na;

		public NetworkSyncTransform NST { get { return nst; } }

		public static NSTSettings nstSettings;
		protected bool isShuttingDown = false;

		void Reset()
		{
			nst = transform.root.GetComponent<NetworkSyncTransform>();
			if (nst == null)
				nst = transform.root.gameObject.AddComponent<NetworkSyncTransform>();
		}

		/// <summary>
		/// Replacement for standard Awake that instead of firing when the component first wakes up, fires after the root NetworkSyncTransform
		/// completes its Awake(). This ensures the NST has been initialized first.
		/// </summary>
		public virtual void OnNstPostAwake()
		{
			// If there is no network, then this NST shouldn't exist - is likely just an orphaned prefab instance in the scene and will be deleted at startup.
			if (MasterNetAdapter.ServerIsActive == false && MasterNetAdapter.ClientIsActive == false)
			{
				isShuttingDown = true;
				return;
			}

			nst = transform.root.GetComponent<NetworkSyncTransform>();
			na = nst.na;
		}
	}

#if UNITY_EDITOR
    /// <summary>
    /// All of this just draws the pretty NST header graphic on components. Nothing to see here.
    /// </summary>
    [CustomEditor(typeof(Component))]
    [CanEditMultipleObjects]
    public class NSTHeaderEditorBase : Editor
    {
        	protected const string HeaderDefaultColor = "Orange";
            protected const string HeaderNSTColor = "Red";
            protected const string HeaderSettingsColor = "Blue";
            protected const string HeaderRewindAddonColor = "Gold Addon";
            protected const string HeaderAnimatorAddonColor = "Gold Addon";
            protected const string HeaderElementAddonColor = "Gold Addon";
            protected const string HeaderEngineColor = "Purple";
            protected const string HeaderHelperColor = "Green";
            protected const string HeaderSampleColor = "Gray";

            protected const string HeaderDefaultName = "Tag";
            protected const string HeaderNSTName = "Primary";
            protected const string HeaderSettingsName = "Settings";
            protected const string HeaderAnimatorAddonName = "Animator Add-on";
            protected const string HeaderElementAddonName = "Element Add-on";
            protected const string HeaderRewindAddonName = "Rewind Add-on";
            protected const string HeaderRewindEngineName = "Engine";
            protected const string HeaderElementsEngineName = "Engine";
            protected const string HeaderMasterName = "Primary";
            protected const string HeaderHelperName = "Helper";
            protected const string HeaderTagName = "Tag";
            protected const string HeaderSampleName = "Sample";


            protected string headerName = HeaderDefaultName;
            protected string headerColor = HeaderDefaultColor;

            Texture2D lTexture;
            Texture2D bTexture;
            Texture2D rTexture;

            public virtual void OnEnable()
            {
                /*if (lTexture == null)
                    lTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/emotitron/_Graphics/HeaderName/NST " + headerName + ".png", typeof(Texture2D));

                if (rTexture == null)
                    rTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/emotitron/_Graphics/NST Teapot.png", typeof(Texture2D));

                if (bTexture == null)
                    bTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/emotitron/_Graphics/Background/NST " + headerColor + ".png", typeof(Texture2D));
                    */
    #if UNITY_EDITOR
                // Touching or adding any Component that is part of the NST Library will fire this.
               // NetAdapterTools.EnsureSceneNetLibDependencies(false);
    #endif
            }

            public override void OnInspectorGUI()
            {
                OverlayHeader();
                base.OnInspectorGUI();
            }

            protected void OverlayHeader()
            {
               /* Rect r = EditorGUILayout.GetControlRect(true, 34);

                float vw = r.width + 18;
                float pad = 6;
                GUI.DrawTexture(new Rect(pad, r.yMin + 2, vw - pad * 2, 32), bTexture);
                GUI.DrawTexture(new Rect(vw - 160 - pad, r.yMin + 2, 160, 32), rTexture);
                GUI.DrawTexture(new Rect(pad, r.yMin + 2, 128, 32), lTexture);*/
            }
        }

        [CustomEditor(typeof(Component))]
        [CanEditMultipleObjects]
        public class NSTHelperEditorBase : NSTHeaderEditorBase
        {
            public override void OnEnable()
            {
                headerName = HeaderHelperName;
                headerColor = HeaderHelperColor;
                base.OnEnable();

            }
        }

        [CustomEditor(typeof(Component))]
        [CanEditMultipleObjects]
        public class NSTSampleHeader : NSTHeaderEditorBase
        {
            public override void OnEnable()
            {
                headerName = HeaderSampleName;
                headerColor = HeaderSampleColor;
                base.OnEnable();

            }
        }
#endif
}
