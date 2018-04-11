﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

namespace emotitron.Network.NST.Rewind
{
	/// <summary>
	/// Callbacks from the RewindEngine.
	/// </summary>
	public interface IRewind
	{
		/// <summary>
		///  Used by the rewind engine to order all rewind related objects to populate their history[0] with the state at time of rewind.
		/// </summary>
		void OnRewind(HistoryFrame frameElements, GameObject ghostGO, int startFrameId, int endFrameId, float timeBeforeSnapshot, float remainder, bool applyToGhost);
	}

	public interface IRewindGhostsToFrame
	{
		/// <summary>
		/// Rewind engine callback when objects are to be rewound to the state of things as they are in the supplied frame.
		/// </summary>
		void OnRewindGhostsToFrame(Frame frame);
	}

	
	public interface ICreateRewindGhost
	{
		/// <summary>
		/// Callback from rewind engine every time a GhostGO (parent and children) is created during the ghost creation process.
		/// </summary>
		void OnCreateGhost(GameObject srcGO, GameObject ghostGO);
	}
}
