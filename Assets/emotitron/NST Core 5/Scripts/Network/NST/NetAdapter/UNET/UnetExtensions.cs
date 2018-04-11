//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{

	/// <summary>
	/// These are all of the abstractions for Unet.
	/// </summary>
	public static class UnetBitstreamSerializers
	{
		public static byte[] reusableByteArray = new byte[64]; // long enough to hold a ulong buffer
		private static NetworkWriter reusablewriter = new NetworkWriter(reusableByteArray);

		//public static void SendBitstreamToAllClients(ref UdpBitStream bitstream, short msgType, int channel = Channels.DefaultUnreliable)
		//{
		//	reusablewriter.StartMessage(msgType);
		//	reusablewriter.WriteUncountedByteArray(bitstream.Data, bitstream.BytesUsed);
		//	reusablewriter.FinishMessage();
		//	//reusablewriter.SendPayloadArrayToAllClients(msgType);

		//	foreach (NetworkConnection nc in NetworkServer.connections)
		//	{
		//		if (nc == null)
		//			continue;

		//		nc.SendWriter(reusablewriter, channel);
		//	}
		//}

		public static void SendPayloadArrayToAllClients(this NetworkWriter writer, short msgType, int channel = Channels.DefaultUnreliable)
		{
			reusablewriter.StartMessage(msgType);
			for (int i = 4; i < writer.Position; i++)
			{
				reusablewriter.Write(writer.AsArray()[i]);
			}
			reusablewriter.FinishMessage();

			foreach (NetworkConnection nc in NetworkServer.connections)
			{
				if (nc == null)
					continue;

				nc.SendWriter(reusablewriter, channel);
			}
		}

		/// <summary>
		/// Write a byte array to the UNET writer without it adding a 16bit tally. Of course
		/// this only is use if the size is fixed - since the reader won't know how many bytes to read on its own.
		/// </summary>
		/// <param name="bytesToWrite">num of bytes in the array to actually write. -1 (default) will write all.</param>
		/// <returns></returns>
		public static void WriteUncountedByteArray(this NetworkWriter writer, byte[] bytes, int bytesToWrite = -1)
		{
			int numOfBytes = (bytesToWrite == -1) ? bytes.Length : bytesToWrite;
			for (int i = 0; i < numOfBytes; i++)
			{
				writer.Write(bytes[i]);
			}
		}

		// Alternative to reading in a Byte array with the UNET reader - which allocates a new Array to do it. This SHOULD produce less garbage.

		/// <summary>
		/// Alternative to HLAPI NetworkReader ReadBytes(), which creates a new byte[] every time. This reuses a byte[] and reads the bytes in
		/// one at a time, hopefully eliminating GC.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="count"></param>
		/// <returns>Returns the same byte array as the on in the arguments.</returns>
		public static byte[] ReadBytesNonAlloc(this NetworkReader reader, byte[] targetbytearray, int count = -1)
		{
			if (count == -1)
				count = reader.Length;

			// TODO does this write 0s once the reader is empty? It needs to.
			for (int i = 0; i < count; i++)
			{
				targetbytearray[i] = reader.ReadByte();
			}
			return targetbytearray;
		}
		// if no reusablearray was provided, use this functions own.
		/// <summary>
		/// If you don't provide a target byte[] array, then the 16 capacity one built into this class will be used. Be sure to make use of the contents
		/// immediately though, as it may get reusued again.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="count"></param>
		/// <returns>Returns the reusable byte array.</returns>
		public static byte[] ReadBytesNonAlloc(this NetworkReader reader, int count)
		{
			return ReadBytesNonAlloc(reader, reusableByteArray, count);
		}

		public static void SendBitstreamToThisConn(this NetworkConnection conn, ref UdpBitStream bitstream, int channel = Channels.DefaultUnreliable)
		{
			reusablewriter.StartMessage(HeaderSettings.single.masterMsgTypeId);
			reusablewriter.WriteUncountedByteArray(bitstream.Data, bitstream.BytesUsed);
			reusablewriter.FinishMessage();
			conn.SendWriter(reusablewriter, channel);
		}

		public static UdpBitStream ReadAllIntoBitstream(this NetworkReader reader)
		{
			int length = reader.Length;
			return new UdpBitStream(reader.ReadBytesNonAlloc(reusableByteArray, reader.Length), length);
		}

//#if UNITY_EDITOR

//		public static NetworkManager GetNetworkManagerSingleton()
//		{
//			if (NetworkManager.singleton == null)
//			{
//				List<NetworkManager> found = FindObjects.FindObjectsOfTypeAllInScene<NetworkManager>();
//				if (found.Count > 0)
//				{
//					NetworkManager.singleton = found[0];
//				}
//				else
//				{
//					DebugX.LogWarning(!DebugX.logWarnings ? null : ("No NetworkManager in scene. Adding one now."));

//					GameObject nmGo = GameObject.Find("Network Manager");

//					if (nmGo == null)
//						nmGo = new GameObject("Network Manager");

//					NetworkManager.singleton = nmGo.AddComponent<NetworkManager>();

//					// Copy over the player prefab from our PUN launcher if there is one.
//					PUNSampleLauncher punl = Object.FindObjectOfType<PUNSampleLauncher>();
//					if (punl && punl.playerPrefab)
//					{
//						Debug.Log("Copying PlayerPrefab from " + typeof(PUNSampleLauncher).Name + " to NetworkManager for you.");
//						NetworkManager.singleton.playerPrefab = punl.playerPrefab;
//					}
//				}
//			}

//			// Add a HUD if that is also missing
//			if (NetworkManager.singleton.gameObject.GetComponent<NetworkManagerHUD>() == null)
//				NetworkManager.singleton.gameObject.AddComponent<NetworkManagerHUD>();


//			return NetworkManager.singleton;
//		}

//#endif
	}
}



