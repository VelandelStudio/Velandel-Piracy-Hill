//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	[AddComponentMenu("NST/NST Position Element")]
	[HelpURL("https://docs.google.com/document/d/1SOm5aZHBed0xVlPk8oX2_PsQ50KTJFgcr8dDXmtdwh8/edit#bookmark=id.yr3caeaa4lon")]
	public class NSTPositionElement : NSTElementComponent, INSTTransformElement
	{
		public PositionElement positionElement = new PositionElement() { name = "Unnamed" };


		// Methods for the ITransformElement interface
		public GameObject SrcGameObject { get { return gameObject; } }
		public override TransformElement TransElement { get { return positionElement; } }
	}

// Need to have this editor or the property drawer freaks out.
#if UNITY_EDITOR

	[CustomEditor(typeof(NSTPositionElement))]
	[CanEditMultipleObjects]
	public class NSTPositionElementEditor : NSTElementComponentEditor
	{

	}
#endif

}

