﻿//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Utilities.GUIUtilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
#if UNITY_EDITOR
	[HelpURL(HELP_URL)]
#endif
	public class HeaderSettings : SettingsScriptableObject<HeaderSettings>
	{

	
		[BitsPerRange(1, 32, 
			MasterNetAdapter.NET_LIB == NetworkLibrary.UNET,
			false,
			"Max NST Objects:", true,
			"Set this to the smallest number that works for your project. 1 bit = 2 NST object max, 4 bits = 16 NST objects max, 5 bits = 32 NST objects max, 32 bits = Unlimited"
			
			)]

		[SerializeField]
		private int bitsForNstId = 6;

		[BitsPerRange(1, 16,
			MasterNetAdapter.NET_LIB == NetworkLibrary.PUN,
			false,
			"Max PUN Clients (Cumulative):", true,
			"Set this to the smallest number that works for your project. Note: PUN increments the client number for every new client, and doesn't recycle old ones as players leave - so this number doesn't represent the max"
			)]
		public int bitsForPUNClients = 10;

		[BitsPerRange(1, 16, 
			MasterNetAdapter.NET_LIB == NetworkLibrary.PUN,
			false,
			"Max Net Entities Per Client:", true,
			"Set this to the smallest number that works for your project."
			)]
		public int bitsForPUNEntities = 6;

		/// <summary>
		/// Returns the bits to be used to for the NST ids over the network, accounting for whether PUN or UNET is being used.
		/// </summary>
		public int BitsForNstId
		{
			get
			{
				return (MasterNetAdapter.NetLib == NetworkLibrary.UNET) ? bitsForNstId : bitsForPUNClients + bitsForPUNEntities;
			}
		}

		[HideInInspector]
		public uint MaxNSTObjects;

		[Tooltip("The Master network tick rate in relation to the the FixedUpdate")]
		[Range(1, 10)]
		public int TickEveryXFixed = 3;

		[Tooltip("The PUN EventCode value used for NST Updates. Be sure this doesn't conflict with any EventCodes you are using (if any).")]
		[Range(MasterNetAdapter.LowestMsgTypeId, MasterNetAdapter.HighestMsgTypeId)]
		public short masterMsgTypeId = MasterNetAdapter.DefaultMsgTypeId;

		public override string SettingsName { get { return "Headers Settings"; } }

		public override void Initialize()
		{
			base.Initialize();
			//Calculate the max objects at the current bits for NstId
			Single.MaxNSTObjects = (uint)Mathf.Pow(2, Single.BitsForNstId);
		}

#if UNITY_EDITOR

		public const string HELP_URL = "https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#bookmark=kix.o06mld4mxhks";
		public override string HelpURL { get { return HELP_URL; } }

		public override bool DrawGui(object target, bool asFoldout, bool includeScriptField)
		{
			bool isExpanded = base.DrawGui(target, asFoldout, includeScriptField);
			if (isExpanded)
				EditorGUILayout.HelpBox(
	"Physics Rate: " + Time.fixedDeltaTime.ToString("0.000") + "ms (" + (1 / Time.fixedDeltaTime).ToString("0.0") + " ticks/sec)\n" +
	"Network Rate: " + (Time.fixedDeltaTime * TickEveryXFixed).ToString("0.000") + "ms (" + (1 / (Time.fixedDeltaTime * TickEveryXFixed)).ToString("0.0") + " ticks/sec)\n\n" +
	"You can change the physics rate by changing the Edit/Project Settings/Time/Fixed Step value.", MessageType.None);

			return isExpanded;
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(HeaderSettings))]
	public class HeaderSettingsEditor : SettingsSOBaseEditor<HeaderSettings>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			HeaderSettings.Single.DrawGui(target, false, true);
		}
	}
#endif
}

