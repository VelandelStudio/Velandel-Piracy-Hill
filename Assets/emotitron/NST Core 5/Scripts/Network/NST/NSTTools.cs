using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace emotitron.Network.NST
{
	public static class NSTTools
	{

		public static Dictionary<uint, NetworkSyncTransform> nstIdToNSTLookup = new Dictionary<uint, NetworkSyncTransform>();
		public static NetworkSyncTransform[] NstIds;

		public static NetworkSyncTransform localPlayerNST;
		public static List<NetworkSyncTransform> allNsts = new List<NetworkSyncTransform>();
		public static List<NetworkSyncTransform> allNstsWithOfftick = new List<NetworkSyncTransform>();

		// TODO move to helper
		public static void GetNstIdAndSetSyncvar(NetworkSyncTransform nst, NSTNetAdapter na, int assignValue = -1)
		{

			// Server needs to set the syncvar for the NstId
			if (na.IsServer && MasterNetAdapter.NetworkLibrary == NetworkLibrary.UNET)
			{
				if (HeaderSettings.Single.BitsForNstId < 6)
					// If an ID value hasn't been passed, we need to get a free one
					na.NstIdSyncvar = (assignValue != -1) ? (uint)assignValue : (uint)GetFreeNstId();
				else
					na.NstIdSyncvar = na.NetId;
			}
			else
			{
				//TEST should use a method in na to convert the viewid to a smaller nstid
				if (NSTNetAdapter.NetLibrary == NetworkLibrary.PUN)
				{
					int clientId = (int)na.NetId / 1000;
					int entityId = (int)na.NetId % 1000;
					na.NstIdSyncvar = (uint)((clientId << HeaderSettings.single.bitsForPUNClients) | entityId);
				}
			}

			RegisterNstId(na.NstIdSyncvar, nst);
		}

		public static void RegisterNstId(uint nstid, NetworkSyncTransform nst)
		{
			
			// If the nstid array is null - it needs to be created. Leave the nst array null for unlimited - that uses the dictionary.
			if (NstIds == null && HeaderSettings.Single.BitsForNstId < 6)
				NstIds = new NetworkSyncTransform[HeaderSettings.Single.MaxNSTObjects];


			if (HeaderSettings.Single.BitsForNstId < 6)
				NstIds[nstid] = nst;

			else if (!nstIdToNSTLookup.ContainsKey(nstid))
				nstIdToNSTLookup.Add(nstid, nst);

			allNsts.Add(nst);

			if (nst.allowOfftick)
				allNstsWithOfftick.Add(nst);
		}

		public static void UnregisterNstId(NetworkSyncTransform nst, NSTNetAdapter na)
		{
			// Don't try to remove this nst from lookups/arrays if it was never added.
			if (!allNsts.Contains(nst))
				return;

			if (na != null && nstIdToNSTLookup != null && nstIdToNSTLookup.ContainsKey(na.NetId))
				nstIdToNSTLookup.Remove(na.NetId);

			if (na != null && NstIds != null)
				NstIds[na.NstIdSyncvar] = null;

			if (allNsts != null && allNsts.Contains(nst))
				allNsts.Remove(nst);

			if (allNstsWithOfftick != null && allNstsWithOfftick.Contains(nst))
				allNstsWithOfftick.Remove(nst);
		}


		// Public methods for looking up game objects by the NstID
		public static NetworkSyncTransform GetNstFromId(uint id)
		{
//#if UNITY_EDITOR
//			// this test won't be needed at runtime since NSTSettings will already be up and running
//			if (MasterNetAdapter.NetworkLibrary == NetworkLibrary.UNET)
//				NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
//#endif
			// 5 bits (32 objects) is the arbitrary cutoff point for using an array for the lookup. For greater numbers the dictionary is used instead.
			if (HeaderSettings.single.BitsForNstId > 5)
			{
				return (nstIdToNSTLookup.ContainsKey(id)) ? nstIdToNSTLookup[id] : null;
			}
			else
			{
				return (NstIds == null || id >= NstIds.Length) ? null : NstIds[(int)id];
			}
		}

		// Save the last new dictionary opening found to avoid retrying to same ones over and over when finding new free keys.
		private static int nstDictLastCheckedPtr;
		public static int GetFreeNstId()
		{
			if (HeaderSettings.single.BitsForNstId < 6)
			{
				// If the nstid array is null - this has been called before any nsts have initialized. Need to create the array.
				if (NstIds == null)
					NstIds = new NetworkSyncTransform[HeaderSettings.single.MaxNSTObjects];

				for (int i = 0; i < NstIds.Length; i++)
				{
					if (NstIds[i] == null)
						return i;
				}
			}
			else
			{
				for (int i = 0; i < 64; i++)
				{
					int offseti = (int)((i + nstDictLastCheckedPtr + 1) % HeaderSettings.single.MaxNSTObjects);
					if (!nstIdToNSTLookup.ContainsKey((uint)offseti) || nstIdToNSTLookup[(uint)offseti] == null)
					{
						nstDictLastCheckedPtr = offseti;
						return offseti;
					}
				}
			}

			Debug.LogError("No more available NST ids. Increase the number Max Nst Objects in NST Settings, or your game will be VERY broken.");
			return -1;
		}

		public static void DestroyAllNSTsInScene()
		{
			DestroyAllNSTsInScene(SceneManager.GetActiveScene());
		}

		public static void DestroyAllNSTsInScene(Scene scene)
		{
			NetworkSyncTransform[] nsts = Resources.FindObjectsOfTypeAll<NetworkSyncTransform>();

			for (int i = 0; i < nsts.Length; i++)
			{
				if (nsts[i].gameObject.scene == scene)
				{
					nsts[i].hasBeenDestroyed = true;
					Object.Destroy(nsts[i].gameObject);
				}
			}
		}
	}
}

