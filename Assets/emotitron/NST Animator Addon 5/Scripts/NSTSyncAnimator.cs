//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.SmartVars;
using emotitron.Network.Compression;
using emotitron.Utilities.BitUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// NST Add-on for syncing the Unity Animator in conjunction with the Network Sync Transform.
	/// </summary>
	[RequireComponent(typeof(NetworkSyncTransform))]
	[DisallowMultipleComponent]
	[AddComponentMenu("NST/NST Sync Animator")]

	public class NSTSyncAnimator : NSTComponent, INstAwake, INstOnInterpolate, INstBitstreamInjectSecond, /*INstOnRcvUpdate,*/ INstOnStartInterpolate /*INstOnEndInterpolate*/ /*, INstOnInterpolate*/
	{
		[SerializeField] private Animator animator;
		//public ApplyTiming applyUpdate = ApplyTiming.OnEndInterpolate;

		[Tooltip("A very small number that determines how small a change in float parameters may as well be considered no change. Only play with this if you are either seeing SyncAnimator traffic when there should be none, or if it seems to be dropping animator updates.")]
		public float floatThreshold = .0000001f;

		[Range(0, 16)]
		public int keyframeEvery = 10;
		[Range(0, 16)]
		[Tooltip("Offsetting the keyframe to an arbitrary number helps keep ALL of the keyframes used by NST from firing on the same updates.")]
		public int keyframeOffset = 3;

		[Tooltip("Convert any float parameters into Half Floats (16bit) to reduce network traffic.")]
		public bool useHalfFloats = true;

		[Tooltip("Indicates whether the server should force its values for the animator when a teleport occurs, or if they should be left alone " +
			"and remain as the owner indicated them. Teleport Override generally is used for things like respawn, " +
			"where the server may want to ‘reset’ aspects of an object to their starting state. Anything marked as Teleport Override, " +
			"you will want to set on the server prior to initiating the teleport - as it will replicate the state on the server to all clients.")]
		public bool teleportOverride;

		private ulong validMask;

		// cached stuff
		private int frameCount;
		private int paramCount;
		private AnimatorControllerParameterType[] paramType;
		private int[] nameHashes;

		private SmartVar[] lastSentParams;
		private SmartVar[][] parameters;
		private SmartVar[] defValue;

		// Initialize this after the NST wakes up
		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();

			if (animator == null)
				animator = GetComponent<Animator>();

			frameCount = nst.frameCount;
			paramCount = animator.parameters.Length;

			lastSentParams = new SmartVar[paramCount];
			nameHashes = new int[paramCount];
			defValue = new SmartVar[paramCount];

			paramType = new AnimatorControllerParameterType[paramCount];
			// Cache some of the readonly parameter attributes
			for (int pid = 0; pid < paramCount; ++pid)
			{
				paramType[pid] = animator.parameters[pid].type;
				nameHashes[pid] = animator.parameters[pid].nameHash;

				if (paramType[pid] == AnimatorControllerParameterType.Int)
					defValue[pid] = animator.parameters[pid].defaultInt;
				else if (paramType[pid] == AnimatorControllerParameterType.Float)
					defValue[pid] = animator.parameters[pid].defaultFloat;
				else
					defValue[pid] = animator.parameters[pid].defaultBool;
			}

			//TODO the extra offtick frame may never be used.
			parameters = new SmartVar[frameCount + 1][];

			for (int fid = 0; fid < frameCount + 1; ++fid)
			{
				parameters[fid] = new SmartVar[paramCount];
			}
		}

		// Callback from NST... inject animator parameters to stream
		public void NSTBitstreamOutgoingSecond(Frame frame, ref UdpBitStream bitstream)
		{
			WriteParameters(ref bitstream, frame.frameid);
		}

		// Callback from NST... extract animator parameters from stream
		public void NSTBitstreamIncomingSecond(Frame frame, Frame currFrame, ref UdpBitStream bitstream, ref UdpBitStream outstream, bool isServer, bool waitingForTeleportConfirm)
		{
			// Reject any incoming updates if we are waiting for a confirmation.
			if (waitingForTeleportConfirm)
			{
				// TODO: Outgoing needs to be stripped from this method to handle teleport correctly! Copy from NSTElementsEngine
				ReadParameters(nst, currFrame.frameid, ref bitstream, ref outstream, isServer);
			}
			else
			{
				ReadParameters(nst, frame.frameid, ref bitstream, ref outstream, isServer);
			}
		}

		private bool IsKey(int frameid)
		{
			if (keyframeEvery == 0)
				return false;

			return (frameid % keyframeEvery == 0);
		}

		void WriteParameters(ref UdpBitStream bitstream, int frameid)
		{
			bool iskey = IsKey(frameid);
			int paramcounter = 0;

			for (int pid = 0; pid < paramCount; ++pid)
			{
				int nameHash = nameHashes[pid];
				AnimatorControllerParameterType type = paramType[pid];

				if (type == AnimatorControllerParameterType.Int)
				{
					int val = animator.GetInteger(nameHash);

					if (iskey || lastSentParams[paramcounter].TypeCode == SmartVarTypeCode.None || val != lastSentParams[paramcounter])
					{
						if (!iskey)
							bitstream.WriteBool(true);

						bitstream.WriteInt(val);
						lastSentParams[paramcounter] = val;
					}
					else
					{
						if (!iskey)
							bitstream.WriteBool(false);
					}
					paramcounter++;
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					float val = animator.GetFloat(nameHash);

					if (iskey || lastSentParams[paramcounter].TypeCode == SmartVarTypeCode.None || Mathf.Abs(val - lastSentParams[paramcounter]) > floatThreshold)
					{
						if (!iskey)
							bitstream.WriteBool(true);

						if (useHalfFloats)
							bitstream.WriteHalf(val);
						else
							bitstream.WriteFloat(val);

						lastSentParams[paramcounter] = val;
					}
					else
					{
						if (!iskey)
							bitstream.WriteBool(false);
					}

					paramcounter++;
				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					bool val = animator.GetBool(nameHash);
					bitstream.WriteBool(val);
					lastSentParams[paramcounter++] = val;
				}

				else if (type == AnimatorControllerParameterType.Trigger)
				{
					bool val = animator.GetBool(nameHash);
					bitstream.WriteBool(val);
					lastSentParams[paramcounter++] = val;
				}
			}
		}

		void ReadParameters(NetworkSyncTransform nst, int frameid, ref UdpBitStream instream, ref UdpBitStream outstream, bool isServer)
		{
			bool iskey = IsKey(frameid);

			SmartVar[] parms = parameters[frameid];

			for (int pid = 0; pid < paramCount; ++pid)
			{
				AnimatorControllerParameterType type = paramType[pid];

				if (type == AnimatorControllerParameterType.Int)
				{
					bool used = iskey ? true : instream.ReadBool();
					if (!iskey && isServer)
						outstream.WriteBool(used);

					if (used)
					{
						int val = instream.ReadInt();
						parms[pid] = val;

						if (isServer)
							outstream.WriteInt(val);
					}
					else
					{
						parms[pid] = SmartVar.None;
					}
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					bool used = iskey ? true : instream.ReadBool();
					if (!iskey && isServer)
						outstream.WriteBool(used);

					if (used)
					{
						if (useHalfFloats)
						{
							float val = instream.ReadHalf();
							parms[pid] = val;

							if (isServer)
								outstream.WriteHalf(val);
						}
						else
						{
							float val = instream.ReadFloat();
							if (isServer)
								outstream.WriteFloat(val);
						}
					}
					else
					{
						parms[pid] = SmartVar.None;
					}
				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					bool val = instream.ReadBool();
					parms[pid] = val;

					if (isServer)
						outstream.WriteBool(val);
				}

				else if (type == AnimatorControllerParameterType.Trigger)
				{
					bool val = instream.ReadBool();
					parms[pid] = val;

					if (isServer)
						outstream.WriteBool(val);
				}
			}

			// Mark this frame as valid
			validMask = validMask | ((ulong)1 << frameid);

		}

		private void ApplyParameters(int frameid)
		{
			if (na.IsMine)
				return;

			SmartVar[] parms = parameters[frameid];

			for (int pid = 0; pid < paramCount; ++pid)
			{
				SmartVar val = parms[pid];
				AnimatorControllerParameterType type = paramType[pid];
				SmartVarTypeCode vtype = val.TypeCode;

				if (vtype == SmartVarTypeCode.None)
					continue;

				if (type == AnimatorControllerParameterType.Int)
				{
					animator.SetInteger(nameHashes[pid], val);
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					animator.SetFloat(nameHashes[pid], val);
				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					animator.SetBool(nameHashes[pid], val);
				}

				else if (type == AnimatorControllerParameterType.Trigger)
				{
					animator.SetBool(nameHashes[pid], val);
				}
			}
		}

		//public void OnRcv(Frame frame)
		//{
		//	if (applyUpdate == ApplyTiming.OnReceiveUpdate)
		//		ApplyParameters(frame.frameid);
		//}

		private int snapFrame;
		private int targFrame;


		public void OnStartInterpolate(Frame frame, bool lateArrival = false, bool midTeleport = false)
		{
			//if (applyUpdate == ApplyTiming.OnReceiveUpdate)
			//	return;

			snapFrame = targFrame;
			targFrame = frame.frameid;
			int frameid = frame.frameid;
			SmartVar[] snaps = parameters[snapFrame];
			SmartVar[] targs = parameters[targFrame];

			bool isValid = (validMask & ((ulong)1 << frameid)) != 0;

			// If this frame is already invalid (never arrived) - extrapolate some kind of value
			// Currently dropped frames use the default value for the parameter (rather than say repeating the last rcvd value)
			// Eventually would like to make this a per parameter option, but that will be a pretty large undertakng.
			if (!isValid)
			{
				for (int pid = 0; pid < paramCount; ++pid)
				{
					// Float lerps back toward default value on lost frames as a loss handling compromise currently.
					if (paramType[pid] == AnimatorControllerParameterType.Float)
						targs[pid] = (snaps[pid].TypeCode == SmartVarTypeCode.Float) ? 
							Mathf.Lerp(defValue[pid], snaps[pid], 0.5f) : (float)defValue[pid];  //snaps[pid]

					else if (paramType[pid] == AnimatorControllerParameterType.Int)
						targs[pid] = defValue[pid];  //snaps[pid]

					else
						targs[pid] = snaps[pid];

				}
			}

			// Set this frame as no longer valid (has be used)
			else
			{
				validMask = validMask & ~((ulong)1 << frameid);
			}

			//if (applyUpdate == ApplyTiming.OnStartInterpolate)
			//	ApplyParameters(frame.frameid);
		}

		//public void OnEndInterpolate(Frame frame)
		//{
		//	snapshotFrame = targetFrame;
		//	targetFrame = frame.frameid;
		//	int frameid = frame.frameid;
		//	SmartVar[] snaps = parameters[targetFrame];
		//	SmartVar[] targs = parameters[targetFrame];

		//	//if (applyUpdate == ApplyTiming.OnEndInterpolate)
		//	//	ApplyParameters(frame.frameid);
		//}

		[Header("Interpolate")]
		public bool intInterpolate = false;
		public bool floatInterpolate = true;

		public void OnInterpolate(float t)
		{
			//for (int i = 0; i < paramCount; i++)
			//{
			//	ApplyParameters(frameCount);
			//}

			SmartVar[] snapParams = parameters[snapFrame];
			SmartVar[] targParams = parameters[targFrame];

			for (int pid = 0; pid < paramCount; ++pid)
			{
				AnimatorControllerParameterType type = paramType[pid];

				SmartVar snap = snapParams[pid];
				SmartVar targ = targParams[pid];

				// Don't try to interpolate if this is flagged as not changed/empty
				if (snap.TypeCode == SmartVarTypeCode.None || targ.TypeCode == SmartVarTypeCode.None)
					continue;

				if (type == AnimatorControllerParameterType.Int)
				{
					int value = (intInterpolate) ?
						(int)Mathf.Lerp(snap, targ, t) : (int)snap;

					animator.SetInteger(nameHashes[pid], value);
				}

				else if (type == AnimatorControllerParameterType.Float)
				{
					float value = (floatInterpolate) ?
						(float)Mathf.Lerp((float)snap, (float)targ, t) : (float)snap;

					animator.SetFloat(nameHashes[pid], value);
				}

				else if (type == AnimatorControllerParameterType.Bool)
				{
					bool value = snap;

					animator.SetBool(nameHashes[pid], value);
				}

				else if (type == AnimatorControllerParameterType.Trigger)
				{
					bool value = snap;
					animator.SetBool(nameHashes[pid], value);
				}
			}
		}
	}


	// Need to have this editor or the property drawer freaks out.
#if UNITY_EDITOR

	[CustomEditor(typeof(NSTSyncAnimator))]
	[CanEditMultipleObjects]
	public class NSTSyncAnimatorEditor : NSTHeaderEditorBase
	{
		public override void OnEnable()
		{
			headerName = HeaderAnimatorAddonName;
			headerColor = HeaderAnimatorAddonColor;
			base.OnEnable();
		}
	}
#endif


}


