//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	[AddComponentMenu("NST/NST Scale Element")]
	[HelpURL("https://docs.google.com/document/d/1SOm5aZHBed0xVlPk8oX2_PsQ50KTJFgcr8dDXmtdwh8/edit#bookmark=id.kpwrna5qrwae")]

	public class NSTScaleElement : NSTElementComponent, INSTTransformElement
	{
		public ScaleElement scaleElement = new ScaleElement() { name = "Unnamed" };

		// Methods for the ITransformElement interface
		public GameObject SrcGameObject { get { return gameObject; } }
		public override TransformElement TransElement { get { return scaleElement; } }
	}

	// Need to have this editor or the property drawer freaks out.
#if UNITY_EDITOR

	[CustomEditor(typeof(NSTScaleElement))]
	[CanEditMultipleObjects]
	public class NSTScaleElementEditor : NSTElementComponentEditor
	{

	}
#endif

}

