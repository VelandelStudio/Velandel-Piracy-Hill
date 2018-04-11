//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	public enum NetworkLibrary { UNET, PUN }

	public enum NetworkModel { ServerClient = 1, MasterClient, PeerToPeer }
	public enum DefaultAuthority { ServerAuthority = 1, OwnerAuthority }
	public enum AuthorityModel { GlobalDefault, ServerAuthority, OwnerAuthority }

#if UNITY_EDITOR
	[HelpURL(HELP_URL)]
#endif
	public class NetLibrarySettings : SettingsScriptableObject<NetLibrarySettings>
	{
		
		public NetworkLibrary networkLibrary;
		public DefaultAuthority defaultAuthority;

		public override string SettingsName { get { return "Network Library Settings"; } }

#if UNITY_EDITOR

		public const string HELP_URL = "https://docs.google.com/document/d/1SOm5aZHBed0xVlPk8oX2_PsQ50KTJFgcr8dDXmtdwh8/edit#bookmark=id.c0t8i8v9ghji";
		public override string HelpURL { get { return HELP_URL; } }

		public override bool DrawGui(object target, bool isAsset, bool includeScriptField = true)
		{
			expandeds[target] =  base.DrawGui(target, isAsset, includeScriptField);

			bool success = NetAdapterTools.ChangeLibraries();

			// if the change failed, set the enum back to the current lib
			if (!success)
				Single.networkLibrary = MasterNetAdapter.NetworkLibrary;

			return expandeds[target];
		}
#endif

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NetLibrarySettings))]
	public class NetLibrarySettingsEditor : SettingsSOBaseEditor<NetLibrarySettings>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			NetLibrarySettings.Single.DrawGui(target, false, true);
		}
	}
#endif

}

