//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	[AddComponentMenu("NST/NST Rotation Element")]
	[HelpURL("https://docs.google.com/document/d/1SOm5aZHBed0xVlPk8oX2_PsQ50KTJFgcr8dDXmtdwh8/edit#bookmark=id.iqa3qmcsbjxz")]

	public class NSTRotationElement : NSTElementComponent, INSTTransformElement
	{
		public RotationElement rotationElement = new RotationElement() { name = "Unnamed" };

		// Methods for the ITransformElement interface
		public GameObject SrcGameObject { get { return gameObject; } }
		public override TransformElement TransElement { get { return rotationElement; } }
	}

	// Need to have this editor or the property drawer freaks out trying to draw the RotationElement.
#if UNITY_EDITOR

	[CustomEditor(typeof(NSTRotationElement))]
	[CanEditMultipleObjects]
	public class NSTRotationElementEditor : NSTElementComponentEditor
	{

	}
#endif
}
