//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Network.Compression;
using emotitron.Utilities.SmartVars;
using emotitron.Utilities.BitUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	public enum RotationType { Quaternion, Euler }
	[System.Serializable]

	public class RotationElement : TransformElement, IRotationElement
	{
		// Common
		public RotationType rotationType;

		[XYZSwitchMask]
		public IncludedAxes includedAxes;

		public RotationType RotationType { get { return rotationType; } }
		private XType xtype;

		/// <summary>
		/// Indexer returns if supplied axisId is enabled
		/// </summary>
		public bool this[int axisId]
		{
			get
			{
				// If this is a quat, all axes are true
				if (rotationType == RotationType.Quaternion)
					return true;

				return ((1 << axisId) & (int)includedAxes) != 0;
			}
		}

		// Quaterion only
		[Range(16, 64)]
		public int totalBitsForQuat = 40;

		[System.Serializable]
		public class AxisRanges
		{
			public float res;
			public bool limitRange;
			public float min;
			public float max;

			// extrapolated values
			public int bits;
			public float range;
			public float mult;
			public float unmult;
			public float wrap;

			public AxisRanges(float r, bool lr, float minv, float maxv)
			{
				res = r;
				limitRange = lr;
				min = minv;
				max = maxv;
			}
		}

		public AxisRanges[] axes = new AxisRanges[3]
		{
			new AxisRanges( .1f, true, -85, 85 ),
			new AxisRanges( .2f, true, -180, 180 ),
			new AxisRanges( .3f, true, -180, 180 )
		};

	public bool useLocal = true;

		//private static int[] maxValue = new int[23]
		//{ 0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095, 8191, 16383,
		//	32767, 65535, 131071, 262143, 524287, 1048575, 2097151, 4194303 };

		public override void Initialize(NetworkSyncTransform _nst)
		{
			base.Initialize(_nst);

			if (!Application.isPlaying || rotationType == RotationType.Euler)
				InitializeRanges();

			xtype = (rotationType == RotationType.Quaternion) ? XType.Quaternion : (XType)includedAxes;

			// Free up memory at runtime start of this rotation isn't making use of Eulers.
			//else
			//	if (Application.isPlaying)
			//	axes = null;
		}

		public void InitializeRanges()
		{
			// Clean up the ranges
			for (int axisId = 0; axisId < 3; axisId++)
			{
				AxisRanges ar = axes[axisId];

				// If this axis is unused, mark as zero and move on.
				if (!this[axisId])
				{
					ar.bits = 0;
					continue;
				}

				if (axes[axisId].limitRange)
				{
					if (ar.max < ar.min)
						ar.max += 360;
					// If the range is greater than 360, get the max down into range. Likely user selected bad min/max values.
					if (ar.max - ar.min > 360)
						ar.max -= 360;
				}

				// use the range as determined by the min/max - unless we are not limiting range, then use 180/360/360
				ar.range = 
					ar.limitRange ? ar.max - ar.min : 
					(axisId == 0) ? 180f : 360f;

				// Get the bits required to transmit the max possible value
				ar.bits = this[axisId] ?  Mathf.Max(0, BitTools.BitsNeededForMaxValue((uint)(Mathf.CeilToInt(ar.range / ar.res)))) : 0;
				
				// Do the heavier division work here so only one multipy per encode/decode is needed
				ar.mult = ar.bits.MaxUintValueForBits() / ar.range;
				ar.unmult = ar.range / ar.bits.MaxUintValueForBits();
				ar.wrap = ar.range + (360 - ar.range) / 2;
			}
		}

		// Constructor
		public RotationElement()
		{
			rotationType = RotationType.Quaternion;
			includedAxes = (IncludedAxes)7;
			elementType = ElementType.Rotation;
		}

		// Shorthand to the rotation that accounts for local vs global rotation
		public override GenericX Localized
		{
			get
			{
				if (rotationType == RotationType.Quaternion)
					return (useLocal) ? gameobject.transform.localRotation : gameobject.transform.rotation;
				else
					return (useLocal) ? gameobject.transform.localEulerAngles : gameobject.transform.eulerAngles;
			}
			set
			{
				if (useLocal)
					Apply(value, gameobject);
				else
					gameobject.transform.rotation = value;
			}
		}

		// Untested
		/// <summary>
		/// Return a compressedelement struct for this localized element.
		/// </summary>
		public override CompressedElement Compress()
		{
			return Compress(Localized);
		}

		public override CompressedElement Compress(GenericX uncompressed)
		{
			if (rotationType == RotationType.Quaternion)
			{
				return QuatCompress.CompressToULong(uncompressed, totalBitsForQuat);
			}
			else
			{
				return new CompressedElement(
					(includedAxes.IsX()) ? CompressFloat(uncompressed[0], 0) : 0,
					(includedAxes.IsY()) ? CompressFloat(uncompressed[1], 1) : 0,
					(includedAxes.IsZ()) ? CompressFloat(uncompressed[2], 2) : 0
					);
			}
		}

		public override GenericX Decompress(CompressedElement comp)
		{
			if (rotationType == RotationType.Quaternion)
			{
				return QuatCompress.DecompressToQuat(comp.quat, totalBitsForQuat);
			}
			else
			{
				return new GenericX(
					(includedAxes.IsX()) ? DecompressFloat(comp[0], 0) : 0,
					(includedAxes.IsY()) ? DecompressFloat(comp[1], 1) : 0,
					(includedAxes.IsZ()) ? DecompressFloat(comp[2], 2) : 0
					);
			}
		}
		public override bool Write(ref UdpBitStream bitstream, Frame frame)
		{
			// Base class does some forceUpdate checking, keep it around.
			bool forceUpdate = IsUpdateForced(frame);
			ElementFrame e = frames[frame.frameid];

			e.compXform = Compress();
			e.xform = Localized;
			CompressedElement newComp = e.compXform;

			if (rotationType == RotationType.Quaternion)
			{
				// For frames between forced updates, we need to first send a flag bit for if this element is being sent
				if (!forceUpdate)
				{
					bool hasChanged = newComp.quat != lastSentCompressed.quat && sendCullMask.OnChanges();
					bitstream.WriteBool(hasChanged);

					// if no changes have occured we are done.
					if (!hasChanged)
						return false;
				}

				bitstream.WriteULong(newComp.quat, totalBitsForQuat);

				lastSentCompressed.quat = newComp.quat;
				lastSentTransform = e.xform;

				return true;
			}

			else
			{
				// For frames between forced updates, we need to first send a flag bit for if this element is being sent
				if (!forceUpdate)
				{
					bool hasChanged = !CompressedElement.Compare(newComp, lastSentCompressed) && sendCullMask.OnChanges();
					bitstream.WriteBool(hasChanged);

					// if no changes have occured we are done.
					if (!hasChanged)
						return false;
				}
				
				for (int axis = 0; axis < 3; axis++)
				{
					if (includedAxes.IsXYZ(axis))
					{
						bitstream.WriteUInt(newComp[axis], axes[axis].bits);
						lastSentCompressed[axis] = newComp[axis];
					}
				}

				return true;
			}
		}

		//public bool IsValueInRange(float value, int axisId)
		//{
		//	float min = axes[axisId].min;
		//	float max = axes[axisId].max;

		//	if (value < min)
		//	{
		//		value += 360;
		//	}
		//	else if (value > max)
		//	{
		//		value -= 360;
		//	}
		//	return (value >= min && value <= max);
		//}

		float[] vals = new float[3];
		uint[] cvals = new uint[3];
		public override bool Read(ref UdpBitStream bitstream, Frame frame, Frame currentFrame)
		{
			ElementFrame e = frames[frame.frameid];

			bool forcedUpdate = IsUpdateForced(frame);
			bool applyToGhost = ShouldApplyToGhost(frame);
			bool isCurrentFrame = frame == currentFrame;

			// Only read for the sent bit if not forced, there is no check bit for forced updates (since all clients and server know it is required)
			bool hasChanged = forcedUpdate || bitstream.ReadBool();

			if (!hasChanged)
			{
				// Leave the transform as is if this is the current friend and hasn't changed - it has already been extrapolated and is mid-lerp
				// So leave it alone. Otherwise set it to GenericX.NULL just to make debugging easier. Eventually can remove this.
				if (!isCurrentFrame)
				{
					e.xform = GenericX.NULL;
					e.compXform = CompressedElement.zero;
				}
				return false;
			}

			if (rotationType == RotationType.Quaternion)
			{
				e.compXform = bitstream.ReadULong(totalBitsForQuat);
				e.xform = e.compXform.quat.DecompressToQuat(totalBitsForQuat);
			}
			else
			{
				
				for (int axisId = 0; axisId < 3; axisId++)
				{
					bool useAxis = includedAxes.IsXYZ(axisId);
					cvals[axisId] = (useAxis) ? bitstream.ReadUInt(axes[axisId].bits) : 0;
					vals[axisId] = (useAxis) ? cvals[axisId] * axes[axisId].unmult + axes[axisId].min : 0;
				}

				e.xform = new GenericX(vals[0], vals[1], vals[2], (XType)includedAxes);
				e.compXform = new CompressedElement(cvals[0], cvals[1], cvals[2]);
			}

			if (applyToGhost)
				Apply(e.xform, rewindGO); // e.transform.ApplyRotation(rewindGO.transform, useLocal);

			return hasChanged;
		}

		public override void MirrorToClients(ref UdpBitStream outstream, Frame frame, bool hasChanged)
		{
			// Write the used flag (if this is not a forced update) and determine if an update needs to be written.
			if (WriteUpdateFlag(ref outstream, frame, hasChanged) == false)
				return;

			ElementFrame e = frames[frame.frameid];

			if (rotationType == RotationType.Quaternion)
				outstream.WriteULong(e.compXform, totalBitsForQuat);
			else
				for (int i = 0; i < 3; i++)
					if (includedAxes.IsXYZ(i))
						outstream.WriteUInt(e.compXform[i], axes[i].bits);

			lastSentCompressed = e.compXform;
		}
		

		public override void UpdateInterpolation(float t)
		{
			if (!hasReceivedInitial)
				return;

			if(xtype == XType.Quaternion)
				Apply(Quaternion.Slerp(frames[snapshotFrameId].xform, frames[targetFrameId].xform, t));
			else
				Apply(Quaternion.Slerp(frames[snapshotFrameId].xform, frames[targetFrameId].xform, t).eulerAngles);
		}

		/// <summary>
		/// Apply a rotation to a gameobject, respecting this elements useLocal and axis restrictions
		/// </summary>
		public override void Apply(GenericX rot, GameObject targetGO)
		{
			int type = (int)xtype;

			if (useLocal)
			{
				if (rot.type == XType.Quaternion)
					targetGO.transform.localRotation = rot;
				else
					targetGO.transform.localRotation =
						Quaternion.Euler (
							((type & 1) != 0) ? rot.x : targetGO.transform.localEulerAngles.x,
							((type & 2) != 0) ? rot.y : targetGO.transform.localEulerAngles.y,
							((type & 4) != 0) ? rot.z : targetGO.transform.localEulerAngles.z
							);
			}
			else
			{
				if (rot.type == XType.Quaternion)
					targetGO.transform.rotation = rot;
				else
					targetGO.transform.eulerAngles =
						new Vector3(
							((type & 1) != 0) ? rot.x : targetGO.transform.eulerAngles.x,
							((type & 2) != 0) ? rot.y : targetGO.transform.eulerAngles.y,
							((type & 4) != 0) ? rot.z : targetGO.transform.eulerAngles.z
							);
			}


			//rot.ApplyRotation(targetGO.transform, useLocal);
		}

		public override GenericX Extrapolate(GenericX curr, GenericX prev)
		{
			if (curr.type == XType.NULL)
			{
				Debug.Log("Extrap pos element NULL !! Try to eliminate these Davin");
				return Localized;
			}
			if (rotationType == RotationType.Quaternion)
			{
				return new GenericX(
					(extrapolation == 0) ? (Quaternion)curr : QuaternionUtils.ExtrapolateQuaternion(prev, curr, 1 + extrapolation),	curr.type);
			}
			else
			{
				if (extrapolation == 0)
					return curr;

				Quaternion extrapolated = QuaternionUtils.ExtrapolateQuaternion(prev, curr, 1 + extrapolation);

				// Test for the rare nasty (NaN, Nan, Nan, NaN) occurance... and deal with it
				if (float.IsNaN(extrapolated[0]))
					return curr;

				return new GenericX(extrapolated.eulerAngles, curr.type);
			}
		}


		public float ClampAxis(float value, int axisId)
		{
			float adjusted = value - axes[axisId].min;

			if (adjusted < 0)
				adjusted += 360;
			else if (adjusted > 360)
				adjusted -= 360;

			if (adjusted > axes[axisId].range && adjusted > axes[axisId].wrap)
				return axes[axisId].min;

			if (adjusted > axes[axisId].range && adjusted < axes[axisId].wrap)
				return axes[axisId].max;

			return value;
		}

		public Vector3 ClampAxes(Vector3 value)
		{
			return new Vector3(
				ClampAxis(value[0], 0),
				ClampAxis(value[1], 1),
				ClampAxis(value[2], 2)
				);
		}

		public uint CompressFloat(float f, int axisId)
		{
			float adjusted = f - axes[axisId].min;

			if (adjusted < 0)
				adjusted += 360;
			else if (adjusted > 360)
				adjusted -= 360;

			// if f is out of range - clamp it
			if (adjusted > axes[axisId].range && adjusted > axes[axisId].wrap)
				return 0;

			if (adjusted > axes[axisId].range && adjusted < axes[axisId].wrap)
				return (uint)axes[axisId].bits.MaxUintValueForBits();

			// Clamp values TODO: probably shoud generate a warning if this happens.
			return (uint)(adjusted * axes[axisId].mult);
		}

		private float DecompressFloat(uint val, int axisId)
		{
			return axes[axisId].unmult + axes[axisId].min;
		}


		public override GenericX Lerp(GenericX start, GenericX end, float t)
		{
			if (xtype == XType.Quaternion)
				return Quaternion.Slerp(start, end, t);
			else
				return Quaternion.Slerp(start, end, t).eulerAngles;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(RotationElement))]
	[CanEditMultipleObjects]

	public class RotationElementDrawer : TransformElementDrawer
	{
		RotationElement re;
		
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(r, property, label);
			re = Utilities.GUIUtilities.PropertyDrawerUtility.GetActualObjectForSerializedProperty<RotationElement>(fieldInfo, property);

			SerializedProperty drawerHeight = property.FindPropertyRelative("drawerHeight");

			if (!noUpdates)
			{
				SerializedProperty rotationType = property.FindPropertyRelative("rotationType");
				SerializedProperty includedAxes = property.FindPropertyRelative("includedAxes");

				SerializedProperty useLocal = property.FindPropertyRelative("useLocal");
				SerializedProperty totalBitsForQuat = property.FindPropertyRelative("totalBitsForQuat");

				EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), rotationType);
				currentLine += LINEHEIGHT;

				// Local Rotation is only adjustable if this is a quaternion type and not the root object - local requires a parent to be relative to
				if (!(isRoot.boolValue && rotationType.intValue == (int)RotationType.Quaternion))
				{
					EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), useLocal, new GUIContent("Lcl Rotation"));
					currentLine += LINEHEIGHT;

				}
				currentLine += 8;

				if (rotationType.intValue == (int)RotationType.Quaternion)
				{
					EditorGUI.PropertyField(new Rect(left, currentLine, realwidth, LINEHEIGHT), totalBitsForQuat, new GUIContent("Total Bits Used:"));
					currentLine += LINEHEIGHT + margin;
					string helpstr = "Some reference values for bit rates:\n" + "40 bits = avg err ~0.04° / max err ~0.1°\n" + "48 bits = avg err ~0.01° / max err ~0.07°";
					EditorGUI.HelpBox(new Rect(left, currentLine, realwidth, LINEHEIGHT * 4), helpstr, MessageType.None);
					currentLine += LINEHEIGHT * 4 + margin;
				}
				else
				{
					for (int axisId = 0; axisId < 3; axisId++)
						DrawAxis(axisId, includedAxes);
				}
			}

			EditorGUI.indentLevel = savedIndentLevel;

			// Record the height of this instance of drawer
			currentLine += margin * 2;
			drawerHeight.floatValue = currentLine - r.yMin;

			//TODO lets not run this ever refresh... should be testing for changes
			if (re.rotationType == RotationType.Euler)
				re.InitializeRanges();
			
			EditorGUI.EndProperty();

		}

		public bool DrawAxis(int axisId, SerializedProperty includedAxes)
		{
			float headerheight = 18;

			Color color = (axisId == 0) ? red : (axisId == 1) ? green : blue;
			string label = (axisId == 0) ? "X (Pitch)" : (axisId == 1) ? "Y (Yaw)" : "Z (Roll)";

			RotationElement.AxisRanges vals = re.axes[axisId];

			float rangeheight = ((IncludedAxes)includedAxes.intValue).IsXYZ(axisId) ? LINEHEIGHT * 2 + 9 : 0; 

			EditorGUI.DrawRect(new Rect(left - 1, currentLine - 3, realwidth - 3 + 2, headerheight + rangeheight + 2), Color.black);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight + rangeheight), color);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight), color * .9f);

			EditorGUI.LabelField(new Rect(left + margin + 18, currentLine -1, 100, LINEHEIGHT), new GUIContent("Use " + label), "BoldLabel");

			bool use = EditorGUI.Toggle(new Rect(left + margin + 0, currentLine - 1, 32, LINEHEIGHT), GUIContent.none, ((IncludedAxes)includedAxes.intValue).IsXYZ(axisId));
			includedAxes.intValue = BitTools.SetBitInInt(includedAxes.intValue, axisId, use);
			
			int bitcount = re.axes[axisId].bits;

			GUI.Label(new Rect(left, currentLine - 1, realwidth - 16, LINEHEIGHT), "Bits: " + bitcount, "MiniLabelRight");

			currentLine += LINEHEIGHT + 4;

			if (use)
			{
				float usedRange = 360f;

				GUI.Label(new Rect(left + 22, currentLine, realwidth, LINEHEIGHT), "Angle Accuracy");
				float floatFieldWidth = 36;
				float resFieldLeft = left + realwidth - 60;
				re.axes[axisId].res = EditorGUI.FloatField(new Rect(resFieldLeft, currentLine, floatFieldWidth, LINEHEIGHT), re.axes[axisId].res);
				EditorGUI.LabelField(new Rect(resFieldLeft + floatFieldWidth, currentLine, realwidth, LINEHEIGHT), "°");

				currentLine += 19;


				vals.limitRange = EditorGUI.Toggle(new Rect(left + margin, currentLine, 32, LINEHEIGHT), GUIContent.none, vals.limitRange);
				if (vals.limitRange)
				{
					vals.min = EditorGUI.FloatField(new Rect(left + 30, currentLine, floatFieldWidth, LINEHEIGHT), vals.min); 
					vals.max = EditorGUI.FloatField(new Rect(left + realwidth - 60, currentLine, floatFieldWidth, LINEHEIGHT), vals.max);

					float minLimit = (axisId == 0) ? -90 : -360;
					float maxLimit = (axisId == 0) ? 90 : 360;

					EditorGUI.MinMaxSlider(new Rect(left + 86, currentLine, realwidth - 80 - 73, LINEHEIGHT), ref vals.min, ref vals.max, minLimit, maxLimit);

					usedRange = vals.max - vals.min;

					if (usedRange > 360)
						vals.max = Mathf.Min(vals.min + 360, 360);

					vals.min = Mathf.Max((int)vals.min, -360);
					vals.max = Mathf.Min((int)vals.max, 360);

				}
				else
				{
					EditorGUI.LabelField(new Rect(margin + 30, currentLine, realwidth - 30, LINEHEIGHT), new GUIContent("Set Range"));
				}

				currentLine += 20;
			}

			// axis bottom padding
			currentLine += 8;
			return use;
		}
	}


#endif

}

