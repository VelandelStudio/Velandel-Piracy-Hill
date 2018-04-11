//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.SmartVars;
using UnityEngine.Events;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{
	public class HistoryFrame
	{
		public int frameid;
		public float endTime;
		public Vector3 rootPosition;

		public HistoryFrame(int _frameid, Vector3 _pos, Quaternion _rot)
		{
			frameid = _frameid;
			rootPosition = _pos;
		}
	}

	public class Frame : UnityEvent<Frame>
	{
		public readonly NetworkSyncTransform nst;
		public readonly int frameid;
		public readonly TransformElement.ElementFrame rootRotElementFrame;

		public float packetArriveTime;
		public float appliedTime;
		public float endTime;
		public UpdateType updateType;
		public BitCullingLevel rootBitCullLevel;
		public CompressedElement compPos;
		public State state;

		// Reference to ElementsEngine.transformElements[]
		private readonly TransformElement[] tes;

		public Vector3 rootPos;
		public byte[] customData;
		public int customMsgSize;
		public int customMsgPtr;

		/// <summary>
		/// Sets both the root position and compressed root position values with one call.
		/// </summary>
		public Vector3 RootPos
		{
			get { return rootPos; }
			set
			{
				rootPos = value;
				compPos = value.CompressToWorld();
			}
		}

		/// <summary>
		/// Sets both the root position and compressed root position values with one call.
		/// </summary>
		public CompressedElement CompRootPos
		{
			get { return compPos;  }
			set
			{
				compPos = value;
				rootPos = value.DecompressFromWorld();
			}
		}

		// TODO: Cache the reference to the root rotation so we can eleminate this long dereference
		/// <summary>
		/// Accesses the Root Rotation from the elements engine
		/// </summary>
		public GenericX RootRot
		{
			get { return rootRotElementFrame.xform; }
			set { rootRotElementFrame.xform = value; }
		}

		public CompressedElement CompRootRot
		{
			get { return rootRotElementFrame.compXform; }
			set { rootRotElementFrame.compXform = value; }
		}

		// Construct
		public Frame(NetworkSyncTransform _nst, int _frameid, Vector3 _pos, CompressedElement _compPos, Quaternion _rot) //, PositionElement[] positionElements, RotationElement[] rotationElements)
		{
			nst = _nst;
			rootPos = _pos;
			compPos = _compPos;
			state = nst.State;
			frameid = _frameid;
			customData = new byte[128];  //TODO: Make this size a user setting

			// references
			tes = _nst.nstElementsEngine.transformElements;
			rootRotElementFrame = nst.rootRotationElement.frames[_frameid];
		}

		public void ModifyFrame(UpdateType _updateType, BitCullingLevel _rootSendType, Vector3 _pos, Quaternion _rot, float _packetArrivedTime)
		{
			updateType = _updateType;
			rootBitCullLevel = _rootSendType;

			rootPos = _pos;
			compPos = _pos.CompressToWorld();

			RootRot = _rot;

			CompRootRot = tes[0].Compress(_rot);
			packetArriveTime = _packetArrivedTime;
		}

		
		/// <summary>
		/// Guess the correct upperbits using the supplied frame for its compressedPos as a starting point. Will find the upperbits that result in the least movement from that pos.
		/// If rootPos is emoty, it will entirely copy the position from the supplied previous frame.
		/// </summary>
		public void CompletePosition(Frame prevCompleteframe)
		{
			CompletePosition(prevCompleteframe.compPos);
		}

		public void CompletePosition(CompressedElement prevComplete)
		{
			// no new position is part of this update - copy the old
			if (rootBitCullLevel == BitCullingLevel.DropAll)
				compPos = prevComplete;
			
			else if (rootBitCullLevel > BitCullingLevel.NoCulling)
				compPos = compPos.OverwriteUpperBits(prevComplete, WorldVectorCompression.axisRanges, rootBitCullLevel);

			// now handled by the get set
			rootPos = compPos.DecompressFromWorld();

		}

		/// <summary>
		/// Apply all of the current transforms to this frames stored transforms.
		/// </summary>
		public void CaptureCurrentTransforms()
		{
			updateType = UpdateType.Teleport;
			rootBitCullLevel = BitCullingLevel.NoCulling;

			RootPos = nst.transform.position;

			for (int eid = 0; eid < tes.Length; eid++)
			{
				TransformElement te = tes[eid];

				te.frames[frameid].xform = te.Localized;
				te.frames[frameid].compXform = te.Compress();
			}
		}

		public override string ToString()
		{
			string e = " Changed Elements: ";
			for (int eid = 0; eid < tes.Length; eid++)
				if (tes[eid].frames[frameid].hasChanged)
					e += eid + " ";

			return
				"FrameID: " + frameid + " ut:" + updateType + "  rst:" + rootBitCullLevel + " " + state + "  " +  e + "\n" + 
				"compPos: " + compPos + " pos: " + rootPos + "\n" +
				"compRot: " + CompRootRot + " rot: " + RootRot;
		}
	}
}
