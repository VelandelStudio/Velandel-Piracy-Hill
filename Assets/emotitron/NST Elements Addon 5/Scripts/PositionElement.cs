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
	public class PositionElement : TransformElement, IPositionElement
	{
		public Compression compression;

		public IncludedAxes includedAxes;

		/// <summary>
		/// Indexer returns if supplied axisId is enabled
		/// </summary>
		public bool this[int axisId]
		{
			get { return ((1 << axisId) & (int)includedAxes) != 0; }
		}
		
		// indexer for the above
		[HideInInspector]
		public FloatRange[] axisRanges;
		public FloatRange[] AxisRanges { get { return axisRanges; } }

		// Constructor
		public PositionElement()
		{
			includedAxes = (IncludedAxes)7; // 7 = xyz true
			elementType = ElementType.Position;
			compression = Compression.LocalRange;

			axisRanges = new FloatRange[3]
			{
				new FloatRange(0, -10, 10, 100),
				new FloatRange(1, -10, 10, 100),
				new FloatRange(2, -10, 10, 100)
			};
		}

		public override GenericX Localized
		{
			// Can modify this in the future to handle local/global positions if need be.
			get { return new GenericX(gameobject.transform.localPosition, (XType)includedAxes); }
			set { Apply(value, gameobject); }
		}

		public override void Initialize(NetworkSyncTransform _nst)
		{
			base.Initialize(_nst);

			// Likely not needed, just a check in case during development nothing got out of sync
			for (int i = 0; i < 3; i++)
				axisRanges[i].ApplyValues();

			lastSentCompressed = Compress();
			lastSentTransform = Localized;
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
					return false;
			}

			// Write each used axis to the stream, using the applicable number of bits for the compression type selected
			for (int axis = 0; axis < 3; axis++)
				if (this[axis])
					bitstream.WriteUInt(newComp[axis],
						(compression == Compression.HalfFloat) ? 16 :
						(compression == Compression.LocalRange) ? axisRanges[axis].Bits : 
						32);

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

			//e.transform = Decompress(e.compTrans);

			if (compression == Compression.HalfFloat)
				e.compXform = new CompressedElement(
					(this[0]) ? bitstream.ReadUInt(16) : 0,
					(this[1]) ? bitstream.ReadUInt(16) : 0,
					(this[2]) ? bitstream.ReadUInt(16) : 0);

			else if (compression == Compression.LocalRange)
				e.compXform = new CompressedElement(
					(this[0]) ? bitstream.ReadUInt(axisRanges[0].Bits) : 0,
					(this[1]) ? bitstream.ReadUInt(axisRanges[1].Bits) : 0,
					(this[2]) ? bitstream.ReadUInt(axisRanges[2].Bits) : 0);

			else
				e.compXform = new CompressedElement(
					(this[0]) ? bitstream.ReadUInt() : 0,
					(this[1]) ? bitstream.ReadUInt() : 0,
					(this[2]) ? bitstream.ReadUInt() : 0);

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
			
			for (int axis = 0; axis < 3; axis++)
				if (this[axis])
				{
					outstream.WriteUInt(e.compXform[axis],
						(compression == Compression.HalfFloat) ? 16 :
						(compression == Compression.LocalRange) ? axisRanges[axis].Bits :
						32);
				}
		}

		public float ClampAxis(float value, int axisId)
		{
			FloatRange ar = axisRanges[axisId];
			// Only range compression has ranges at all.
			return (compression == Compression.LocalRange) ?
				ar.Clamp(value) : value;
		}

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

		public override void Apply(GenericX pos, GameObject targetGO)
		{
			targetGO.transform.localPosition = new Vector3(
				(this[0]) ? pos[0] : targetGO.transform.localPosition[0],
				(this[1]) ? pos[1] : targetGO.transform.localPosition[1],
				(this[2]) ? pos[2] : targetGO.transform.localPosition[2]);
		}

		public override GenericX Extrapolate(GenericX curr, GenericX prev)
		{
			if (curr.type == XType.NULL)
			{
				Debug.Log("Extrap pos element NULL !! Try to eliminate these Davin");
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

	[CustomPropertyDrawer(typeof(PositionElement))]
	[CanEditMultipleObjects]

	public class PositionElementEditor : TransformElementDrawer
	{
		SerializedProperty compression;
		PositionElement pe;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(r, property, label);

			SerializedProperty drawerHeight = property.FindPropertyRelative("drawerHeight");
			pe = PropertyDrawerUtility.GetActualObjectForSerializedProperty<PositionElement>(fieldInfo, property);

			if (!noUpdates)
			{
				compression = property.FindPropertyRelative("compression");
				SerializedProperty includedAxes = property.FindPropertyRelative("includedAxes");
				SerializedProperty axisRanges = property.FindPropertyRelative("axisRanges");
				SerializedProperty xrange = axisRanges.GetArrayElementAtIndex(0);
				SerializedProperty yrange = axisRanges.GetArrayElementAtIndex(1);
				SerializedProperty zrange = axisRanges.GetArrayElementAtIndex(2);
				//SerializedProperty xrange = property.FindPropertyRelative("xrange");
				//SerializedProperty yrange = property.FindPropertyRelative("yrange");
				//SerializedProperty zrange = property.FindPropertyRelative("zrange");

				EditorGUI.PropertyField(new Rect(left, currentLine, r.width, LINEHEIGHT), compression, new GUIContent("Compression"));

				IncludedAxes ia = (IncludedAxes)includedAxes.intValue;

				//bool x = DrawAxis(xrange, pe.xrange, ia.IsX(), "Use X", red);
				//bool y = DrawAxis(yrange, pe.yrange, ia.IsY(), "Use Y", green);
				//bool z = DrawAxis(zrange, pe.zrange, ia.IsZ(), "Use Z", blue);
				bool x = DrawAxis(xrange, pe.axisRanges[0], ia.IsX(), "Use X", red);
				bool y = DrawAxis(yrange, pe.axisRanges[1], ia.IsY(), "Use Y", green);
				bool z = DrawAxis(zrange, pe.axisRanges[2], ia.IsZ(), "Use Z", blue);

				includedAxes.intValue = (x ? 1 : 0) | (y ? 2 : 0) | (z ? 4 : 0);
			}

			// revert to original indent level.
			EditorGUI.indentLevel = savedIndentLevel;

			// Record the height of this instance of drawer
			currentLine += LINEHEIGHT + margin * 2 + margin;
			drawerHeight.floatValue = currentLine - r.yMin;

			EditorGUI.EndProperty();

		}

		private bool DrawAxis(SerializedProperty range, FloatRange axisRange, bool includeAxis, string name, Color color)
		{
			currentLine += LINEHEIGHT + 8;

			bool showrange = includeAxis && ((Compression)compression.intValue == Compression.LocalRange);

			float headerheight = 18;

			float rangeheight = showrange ? LINEHEIGHT + 8 : 0;
			EditorGUI.DrawRect(new Rect(left - 1, currentLine - 3, realwidth - 3 + 2, headerheight + rangeheight + 2), Color.black);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight + rangeheight), color);
			EditorGUI.DrawRect(new Rect(left, currentLine - 2, realwidth - 3, headerheight), color * .9f);

			EditorGUI.LabelField(new Rect(left + margin + 18, currentLine, 100, LINEHEIGHT), new GUIContent(name), lefttextstyle);
			includeAxis = EditorGUI.Toggle(new Rect(left + margin + 0, currentLine - 1, 32, LINEHEIGHT), GUIContent.none, includeAxis );

			int bits = (includeAxis) ?
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

