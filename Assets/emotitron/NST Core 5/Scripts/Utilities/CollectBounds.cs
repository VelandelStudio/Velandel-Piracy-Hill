//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Utilities
{
	public static class BoundsTools
	{
		public enum BoundsType { Both, MeshRenderer, Collider }

		private static List<MeshFilter> reusableSearchMeshFilter = new List<MeshFilter>();
		private static List<MeshRenderer> reusableSearchMeshRend = new List<MeshRenderer>();
		private static List<Collider> reusableSearchColliders = new List<Collider>();
		private static List<Collider> reusableValidColliders = new List<Collider>();

		/// <summary>
		/// Collect the bounds of the indicated types (MeshRenderer and/or Collider) on the object and all of its children, and returns bounds that are a sum of all of those.
		/// </summary>
		/// <param name="go">GameObject to start search from.</param>
		/// <param name="factorIn">The types of bounds to factor in.</param>
		/// <param name="includeChildren">Whether to search all children for bounds.</param>
		/// <returns></returns>
		public static Bounds CollectMyBounds(GameObject go, BoundsType factorIn, out int numOfBoundsFound, bool includeChildren = true, bool includeInactive = false)
		{
			// if we are ignoring inactive, an inactive parent is already a null. Quit here.
			if (!go.activeInHierarchy && !!includeInactive)
			{
				numOfBoundsFound = 0;
				return new Bounds();
			}

			bool bothtype = factorIn == BoundsType.Both;
			bool rendtype = bothtype || factorIn == BoundsType.MeshRenderer;
			bool colltype = bothtype || factorIn == BoundsType.Collider;

			// Clear the reusables so they have counts of zero
			reusableSearchMeshFilter.Clear();
			reusableSearchMeshRend.Clear();
			reusableSearchColliders.Clear();
			reusableValidColliders.Clear();

			int myBoundsCount = 0;

			// Find all of the MeshRenderers and Colliders (as specified)
			if (rendtype)
			{
				if (go.activeInHierarchy)
				{
					if (includeChildren)
						go.GetComponentsInChildren(includeInactive, reusableSearchMeshFilter);
					else
						go.GetComponents(reusableSearchMeshFilter);
				}
			}

			if (colltype)
			{
				if (go.activeInHierarchy)
				{
					if (includeChildren)
						go.GetComponentsInChildren(includeInactive, reusableSearchColliders);
					else
						go.GetComponents(reusableSearchColliders);
				}
			}

			// Add any MeshRenderer attached to the found MeshFilters to their own list.
			// We want the MeshRenderer for its bounds, but only if there is a MeshFilter, otherwise there is a risk of a 0,0,0
			for (int i = 0; i < reusableSearchMeshFilter.Count; i++)
			{
				MeshRenderer mr = reusableSearchMeshFilter[i].GetComponent<MeshRenderer>();

				if (mr && (mr.enabled || includeInactive))
					reusableSearchMeshRend.Add(mr);
			}

			// Collect only the valid colliders (ignore inactive if not includeInactive)
			for (int i = 0; i < reusableSearchColliders.Count; i++)
			{
				if (reusableSearchColliders[i].enabled || includeInactive)
					reusableValidColliders.Add(reusableSearchColliders[i]);
			}

			// Make sure we found some bounds objects, or we need to quit.
			numOfBoundsFound = reusableSearchMeshRend.Count + reusableValidColliders.Count;
			// No values means no bounds will be found, and this will break things if we try to use it.
			if (numOfBoundsFound == 0)
			{
				return new Bounds();
			}

			// Get a starting bounds. We need this because the default of centered 0,0,0 will break things if the map is
			// offset and doesn't encapsulte the world origin.
			Bounds compositeBounds = (reusableSearchMeshRend.Count > 0) ? reusableSearchMeshRend[0].bounds : reusableValidColliders[0].bounds;

			// Encapsulate all outer found bounds into that. We will be adding the root to itself, but no biggy, this only runs once.
			for (int i = 0; i < reusableSearchMeshRend.Count; i++)
			{
				myBoundsCount++;
				compositeBounds.Encapsulate(reusableSearchMeshRend[i].bounds);
			}

			for (int i = 0; i < reusableValidColliders.Count; i++)
			{
				myBoundsCount++;
				compositeBounds.Encapsulate(reusableValidColliders[i].bounds);
			}

			return compositeBounds;

		}

		public static Bounds CollectMyBounds(GameObject go, BoundsType factorIn, bool includeChildren = true)
		{
			int dummy;
			return CollectMyBounds(go, factorIn, out dummy, includeChildren);
		}

	}
}

