using System;
using UnityEngine;
using emotitron.Network.NST;
using UnityEditor;

namespace emotitron.Network.Compression
{
	[CustomPropertyDrawer(typeof(FloatRange))]
	[CanEditMultipleObjects]

	public class AxisRangeDrawer : PropertyDrawer
	{
		protected float rows = 4;
		protected const int LINEHEIGHT = 16;

		protected float margin = 4;
		protected float realwidth;
		protected float colwidths;
		protected float currentLine;
		protected int savedIndentLevel;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			savedIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 1;

			// Get the actual instance of this FloatRange
			TransformElement par = (TransformElement)Utilities.GUIUtilities.PropertyDrawerUtility.GetParent(property);
			SerializedProperty axis = property.FindPropertyRelative("axis");
			IPositionElement pe = par as IPositionElement;
			IScaleElement se = par as IScaleElement;

			FloatRange ar = (pe != null) ? pe.AxisRanges[axis.intValue] : se.AxisRanges[axis.intValue];

			realwidth = r.width;
			float padding = 4f;
			float lablelWidth = 64f;
			float fieldWidth = 56f;
			float labeloffset = - 82f;
			float fieldOffset = -(fieldWidth * .5f) - padding;

			float col2 = realwidth - r.width * .6666667f;
			float col3 = realwidth - r.width * .3333333f ;
			float colend = realwidth;

			float rowoffset = r.yMin;

			ar.Min = EditorGUI.FloatField(new Rect(col2 + fieldOffset, rowoffset, fieldWidth, 16), GUIContent.none, ar.Min);
			EditorGUI.LabelField(new Rect(col2 + labeloffset, rowoffset, lablelWidth, 16), new GUIContent("min"), "RightLabel");

			ar.Max = EditorGUI.FloatField(new Rect(col3 + fieldOffset, rowoffset, fieldWidth, 16), GUIContent.none, ar.Max);
			EditorGUI.LabelField(new Rect(col3 + labeloffset, rowoffset, lablelWidth, 16), new GUIContent("max"), "RightLabel");

			ar.Resolution = EditorGUI.IntField(new Rect(colend + fieldOffset, rowoffset, fieldWidth, 16), GUIContent.none, ar.Resolution);
			EditorGUI.LabelField(new Rect(colend + labeloffset, rowoffset, lablelWidth, 16), new GUIContent("res"), "RightLabel");

			if (pe != null)
				pe.AxisRanges[axis.intValue].ApplyValues();
			else if (se != null)
				se.AxisRanges[axis.intValue].ApplyValues();

			EditorGUI.indentLevel = savedIndentLevel;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * rows;  
		}
	}
}