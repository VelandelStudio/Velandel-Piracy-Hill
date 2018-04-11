//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.GUIUtilities;

namespace emotitron.Network.NST
{

	[DisallowMultipleComponent]
	public abstract class NSTRootSingleton<T> : NSTComponent where T : Component
	{

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();

			// Remove if this is not on the root
			if (transform != transform.root)
			{
				DebugX.LogError(!DebugX.logErrors ? null : 
					("Removing '" + typeof(T) + "' from child '" + name + "' of gameobject " + transform.root.name + 
					". This component should only exist on the root of a networked object with a NetworkSyncTranform component."));

				Destroy(this);
			}
		}

		public static T EnsureExistsOnRoot(Transform trans, bool isExpanded = true)
		{
			T found;

#if !UNITY_EDITOR
			
			// this is an unspawned NST object in the scene at start, and will be deleted.
			if (!MasterNetAdapter.ServerIsActive && !MasterNetAdapter.ClientIsActive)
			{
				Destroy(trans.root.gameObject);
				return null;
			}

			found = trans.root.GetComponent<T>();

			if (!found)
				trans.root.gameObject.AddComponent(typeof(T));

#else

			found = trans.root.gameObject.EnsureRootComponentExists<T>(isExpanded);
#endif
			return found;
		}
	}
}
