//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Network.Compression;
using emotitron.Utilities.SmartVars;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{

	[System.Serializable]
	public class ScaleElement : TransformElement, IScaleElement
	{
		public enum UniformAxes { NonUniform = 0, XY = 3, XZ = 5, YZ = 6, XYZ = 7 }
		public bool IsUniformAxis(int axisId)
		{
			return uniformAxes != 0 &&
				(((int)uniformAxes & (1 << axisId)) != 0);
		}


		public Compression compression;
		//public bool uniformScale;
		public UniformAxes uniformAxes = 0;
		public IncludedAxes includedAxes;

		// Cached values
		private int startAxis;
		private int endAxis;
		private XType xtype;

		/// <summary>
		/// Indexer returns if supplied axisId is enabled
		/// </summary>
		public bool this[int axisId]
		{
			get { return
				uniformAxes == 0 ? 
				((1 << axisId) & (int)includedAxes) != 0 : 
				IsUniformAxis(axisId);
			}
		}

		private bool[] isAxisSerialized;
		private bool[] isAxisUsed;

		// indexer for the above
		[HideInInspector]
		public FloatRange[] axisRanges;
		public FloatRange[] AxisRanges { get { return axisRanges; } }

		// Constructor
		public ScaleElement()
		{
			includedAxes = (IncludedAxes)7; // 7 = xyz true
			elementType = ElementType.Scale;
			compression = Compression.LocalRange;
			//uniformScale = true;
			axisRanges = new FloatRange[3]
			{
				new FloatRange(0, 0, 10, 100),
				new FloatRange(1, 0, 10, 100),
				new FloatRange(2, 0, 10, 100)
			};
		}
		public override GenericX Localized
		{
			// Can modify this in the future to handle local/global scale if need be.
			get { return new GenericX(gameobject.transform.localScale, xtype); }
			set { Apply(value, gameobject); }
		}

		public override void Initialize(NetworkSyncTransform _nst)
		{
			base.Initialize(_nst);

			// If this is a uniform scale, make sure all axes have the same compression settings as X
			FloatRange ar = axisRanges[0];
			if (uniformAxes != 0)
				for (int i = 1; i < 3; i++)
					axisRanges[i].SetRange(ar.Min, ar.Max, ar.Resolution);

			// Ensure all axes have properly set encode/decode initialization
			for (int i = 0; i < 3; i++)
				axisRanges[i].ApplyValues();

			lastSentCompressed = Compress();
			lastSentTransform = Localized;

			// for uniform yz we need to use the y axis... all others x will do
			startAxis = (uniformAxes == UniformAxes.YZ) ? 1 : 0;
			endAxis = ((uniformAxes == 0) ? 2 : startAxis);
			// cached tests for whether axis is read/written
			isAxisSerialized = new bool[3]
			{
				((1 << 0 & (int)includedAxes) != 0 && uniformAxes == 0) || (uniformAxes != UniformAxes.YZ && uniformAxes > 0),
				((1 << 1 & (int)includedAxes) != 0 && uniformAxes == 0) || uniformAxes == UniformAxes.YZ,
				((1 << 2 & (int)includedAxes) != 0 && uniformAxes == 0)
			};

			// cache the xtype axes given to genericx
			xtype = (uniformAxes == 0) ? (XType)uniformAxes : (XType)includedAxes;
		}

		/// <summary>
		/// Return a compressedelement struct for this localized element.
		/// </summary>
		public override CompressedElement Compress()
		{
			return Compress(Localized);
		}

		public override CompressedElement Compress(GenericX uncompressed)
		{
			if (compression == Compression.HalfFloat)
				return new CompressedElement(
					(this[0]) ? SlimMath.HalfUtilities.Pack(uncompressed[0]) : (ushort)0,
					(this[1]) ? SlimMath.HalfUtilities.Pack(uncompressed[1]) : (ushort)0,
					(this[2]) ? SlimMath.HalfUtilities.Pack(uncompressed[2]) : (ushort)0
					);
			if (compression == Compression.LocalRange)
				return new CompressedElement(
					(this[0]) ? axisRanges[0].Encode(uncompressed[0]) : 0,
					(this[1]) ? axisRanges[1].Encode(uncompressed[1]) : 0,
					(this[2]) ? axisRanges[2].Encode(uncompressed[2]) : 0
					);

			return new CompressedElement(
				(this[0]) ? uncompressed[0] : 0f,
				(this[1]) ? uncompressed[1] : 0f,
				(this[2]) ? uncompressed[2] : 0f
				);
		}

		public override GenericX Decompress(CompressedElement comp)
		{
			// TODO likely change changed localized to just zeros and save some ms
			if (compression == Compression.HalfFloat)
				return new GenericX(
					(this[0]) ? SlimMath.HalfUtilities.Unpack((ushort)comp[0]) : 0,
					(this[1]) ? SlimMath.HalfUtilities.Unpack((ushort)comp[1]) : 0,
					(this[2]) ? SlimMath.HalfUtilities.Unpack((ushort)comp[2]) : 0
					);

			else if (compression == Compression.LocalRange)
				return new GenericX(
					(this[0]) ? axisRanges[0].Decode(comp[0]) : 0,
					(this[1]) ? axisRanges[1].Decode(comp[1]) : 0,
					(this[2]) ? axisRanges[2].Decode(comp[2]) : 0
					);
			else
				return new GenericX(
					(this[0]) ? comp.GetFloat(0) : 0,
					(this[1]) ? comp.GetFloat(1) : 0,
					(this[2]) ? comp.GetFloat(2) : 0
					);
		}

		public override bool Write(ref UdpBitStream bitstream, Frame frame)
		{
			ElementFrame e = frames[frame.frameid];
			e.compXform = Compress();
			e.xform = Localized;

			CompressedElement newComp = e.compXform;

			bool forceUpdate = IsUpdateForced(frame);

			// For frames between forced updates, we need to first send a flag bit for if this element is being sent
			if (!forceUpdate)
			{
				bool hasChanged = !CompressedElement.Compare(newComp, lastSentCompressed) && sendCullMask.OnChanges();
				bitstream.WriteBool(hasChanged);
				// if no changes have occured we are done.
				if (!hasChanged)
				{
					return false;
				}
			}
			//DebugText.Log("haschanged " + startAxis + " " + endAxis);

			// Write each used axis to the stream, using the applicable number of bits for the compression type selected
			for (int axis = startAxis; axis <= endAxis; axis++)
				if (isAxisSerialized[axis])
				{
					bitstream.WriteUInt(newComp[axis],
					(compression == Compression.HalfFloat) ? 16 :
					(compression == Compression.LocalRange) ? axisRanges[axis].Bits :
					32);
				}

			lastSentCompressed = newComp;
			lastSentTransform = e.xform;
			return true;
		}

		public override bool Read(ref UdpBitStream bitstream, Frame frame, Frame currentFrame)
		{
			ElementFrame e = frames[frame.frameid];

			bool forcedUpdate = IsUpdateForced(frame);
			bool applyToGhost = ShouldApplyToGhost(frame);
			bool isCurrentFrame = frame == currentFrame;

			// Only read for the sent bit if not forced, there is no check bit for forced updates (since all clients and server know it is forced)
			bool hasChanged = forcedUpdate || bitstream.ReadBool();

			if (!hasChanged)
			{
				// Leave the transform as is if this is the current friend and hasn't changed - it has already been extrapolated and is mid-lerp
				// So leave it alone. Otherwise sete it to GenericX.NULL just to make debugging easier. Eventually can remove this.
				if (!isCurrentFrame)
				{
					e.xform = GenericX.NULL;
					e.compXform = CompressedElement.zero;
				}
				return false;
			}
			uint uniformread = 0;

			if (compression == Compression.HalfFloat)
			{
				if (uniformAxes != 0)
					uniformread = bitstream.ReadUInt(16);
				else
					e.compXform = new CompressedElement(
						isAxisSerialized[0] ? bitstream.ReadUInt(16) : 0,
						isAxisSerialized[1] ? bitstream.ReadUInt(16) : 0,
						isAxisSerialized[2] ? bitstream.ReadUInt(16) : 0);
			}

			else if (compression == Compression.LocalRange)
			{
				if (uniformAxes != 0)
					uniformread = bitstream.ReadUInt(axisRanges[0].Bits);
				else
					e.compXform = new CompressedElement(
						isAxisSerialized[0] ? bitstream.ReadUInt(axisRanges[0].Bits) : 0,
						isAxisSerialized[1] ? bitstream.ReadUInt(axisRanges[1].Bits) : 0,
						isAxisSerialized[2] ? bitstream.ReadUInt(axisRanges[2].Bits) : 0);
			}

			else
			{
				if (uniformAxes != 0)
					uniformread = bitstream.ReadUInt();
				else
					e.compXform = new CompressedElement(
						isAxisSerialized[0] ? bitstream.ReadUInt() : 0,
						isAxisSerialized[1] ? bitstream.ReadUInt() : 0,
						isAxisSerialized[2] ? bitstream.ReadUInt() : 0);
			}

			// if this is a uniform scale, apply the one value to all applicable axes
			if (uniformAxes != 0)
			{
				e.compXform = new CompressedElement(
					IsUniformAxis(0) ? uniformread : 0,
					IsUniformAxis(1) ? uniformread : 0,
					IsUniformAxis(2) ? uniformread : 0);
			}

			e.xform = Decompress(e.compXform);

			if (applyToGhost)
				Apply(e.xform, rewindGO);

			return true;
		}

		public override void MirrorToClients(ref UdpBitStream outstream, Frame frame, bool hasChanged)
		{
			// Write the used flag (if this is not a forced update) and determine if an update needs to be written.
			if (WriteUpdateFlag(ref outstream, frame, hasChanged) == false)
				return;

			ElementFrame e = frames[frame.frameid];

			// Only writes first (x) axis if uniform is being used. 
			for (int axis = startAxis; axis <= endAxis; axis++)
				if (isAxisSerialized[axis])
				{
					outstream.WriteUInt(e.compXform[axis],
						(compression == Compression.HalfFloat) ? 16 :
						(compression == Compression.LocalRange) ? axisRanges[axis].Bits :
						32);
				}
		}

		//TODO make more efficient for uniform scales
		public float ClampAxis(float value, int axisId)
		{
			FloatRange ar = axisRanges[(uniformAxes != 0) ? 0 : axisId];
			// Only range compression has ranges at all.
			return (compression == Compression.LocalRange) ?
				ar.Clamp(value) : value;
		}

		//TODO make more efficient for uniform scales
		public Vector3 ClampAxes(Vector3 value)
		{
			return new Vector3(
				ClampAxis(value[0], 0),
				ClampAxis(value[1], 1),
				ClampAxis(value[2], 2)
				);
		}

		protected float[] xyz = new float[3];

		public override void UpdateInterpolation(float t)
		{
			if (!hasReceivedInitial)
				return;

			Apply(Vector3.Lerp(frames[snapshotFrameId].xform, frames[targetFrameId].xform, t));
		}

		public override void Apply(GenericX scale, GameObject targetGO)
		{
			//Debug.Log("scale " + scale);
			targetGO.transform.localScale = new Vector3(
				(this[0]) ? scale[0] : targetGO.transform.localScale[0],
				(this[1]) ? scale[1] : targetGO.transform.localScale[1],
				(this[2]) ? scale[2] : targetGO.transform.localScale[2]);
		}

		public override GenericX Extrapolate(GenericX curr, GenericX prev)
		{
			// This should only happen at startup as it is now.
			if (curr.type == XType.NULL)
			{
				return Localized;
			}

			return
				(extrapolation == 0) ? curr :
				curr + (curr - prev) * extrapolation;
		}

		public override GenericX Lerp(GenericX start, GenericX end, float t)
		{
			return Vector3.Lerp(start, end, t);
		}

	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(ScaleElement))]
	[CanEditMultipleObjects]

	public class ScaleElementEditor : TransformElementDrawer
	{
		SerializedProperty uniformAxes;
		SerializedProperty compression;
		ScaleElement se;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(r, property, label);

			SerializedProperty drawerHeight = property.FindPropertyRelative("drawerHeight");
			se = PropertyDrawerUtility.GetActualObjectForSerializedProperty<ScaleElement>(fieldInfo, property);

			if (!noUpdates)
			{
				compression = property.FindPropertyRelative("compression");
				uniformAxes = property.FindPropertyRelative("uniformAxes");
				//SerializedProperty includedAxes = property.FindPropertyRelative("includedAxes");
				SerializedProperty axisRanges = property.FindPropertyRelative("axisRanges");
				SerializedProperty xrange = axisRanges.GetArrayElementAtIndex(0);
				SerializedProperty yrange = axisRanges.GetArrayElementAtIndex(1);
				SerializedProperty zrange = axisRanges.GetArrayElementAtIndex(2);

				EditorGUI.PropertyField(new Rect(left, currentLine, r.width, LINEHEIGHT), compression);
				currentLine += LINEHEIGHT + 2;
				EditorGUI.PropertyField(new Rect(left, currentLine, r.width, LINEHEIGHT), uniformAxes);

				if (se.uniformAxes != 0)
				{
					DrawAxis(se, 0, xrange, se.axisRanges[0], true, "Uniform Scale", gray);
				}
				else
				{
					bool x = DrawAxis(se, 0, xrange, se.axisRanges[0], false, "Use X", red);
					bool y = DrawAxis(se, 1, yrange, se.axisRanges[1], false, "Use Y", green);
					bool z = DrawAxis(se, 2, zrange, se.axisRanges[2], false, "Use Z", blue);

					se.includedAxes = (IncludedAxes)((x ? 1 : 0) | (y ? 2 : 0) | (z ? 4 : 0));
				}
				
			}

			// revert to original indent level.
			EditorGUI.indentLevel = savedIndentLevel;

			// Record the height of this instance of drawer
			currentLine += LINEHEIGHT + margin * 2 + margin;
			drawerHeight.floatValue = currentLine - r.yMin;

			EditorGUI.EndProperty();

		}

		private bool DrawAxis(ScaleElement se, int axis, SerializedProperty range, FloatRange axisRange, bool isUniformScale, string name, Color color)
		{
			currentLine += LINEHEIGHT + 8;

			bool showrange = 
				isUniformScale ||
				(se.includedAxes.IsXYZ(axis) && ((Compression)compression.intValue == Compression.LocalRange));

			float headerheight = 18;

			float rangeheight = showrange ? LINEHEIGHT + 8 : 0;
			EditorGUI.DrawRect(new Rect(left - 1, currentLine - 3, realwidth - 3 + 2, headerheight + rangeheight + 2), Color.black);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight + rangeheight), color);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight), color * .9f);

			float labelLeft = left + margin + (isUniformScale ? 0: 18);
			EditorGUI.LabelField(new Rect(labelLeft, currentLine, 100, LINEHEIGHT), new GUIContent(name), lefttextstyle);

			bool includeAxis = se.includedAxes.IsXYZ(axis);
			if (!isUniformScale)
			{
				includeAxis = EditorGUI.Toggle(new Rect(left + margin + 0, currentLine - 1, 32, LINEHEIGHT), GUIContent.none, includeAxis);
			}

			int bits = (includeAxis || isUniformScale) ?
				compression.enumValueIndex == 1 ? 16 :
				compression.enumValueIndex == 2 ? axisRange.GetBitsForRangeAndRez() :
				32 :
				0;

			GUI.Label(new Rect(left, currentLine - 1, realwidth - 16, LINEHEIGHT), "Bits: " + bits, "MiniLabelRight");

			if (showrange)
			{
				currentLine += LINEHEIGHT + 4;
				EditorGUI.PropertyField(new Rect(left + 10, currentLine, realwidth - 16, LINEHEIGHT), range, GUIContent.none); //new GUIContent(compression.enumValueIndex.ToString()));
				currentLine += 4;

			}

			return includeAxis;
		}
	}
#endif
}

