using PicaVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleElement : MonoBehaviour {
    public void Explode(Vector3 contactPoint)
    {
        int layerMask = ~(1 << LayerMask.NameToLayer("Indestructible"));
        Collider[] hitColliders = Physics.OverlapSphere(contactPoint, 0.4f, layerMask, QueryTriggerInteraction.Ignore);
        HashSet<Volume> vols = new HashSet<Volume>();

        for (int i = 0; i < hitColliders.Length; i++)
        {
            vols.UnionWith(hitColliders[i].GetComponentsInParent<Volume>());
        }

        foreach (Volume vol in vols)
        {
            var batch = vol.Explode(contactPoint, 1, 0, Exploder.ExplodeValueFilterOperation.GreaterThanOrEqualTo);
            if (batch.Voxels.Count > 0 && VoxelParticleSystem.Instance != null)
            {
                // Adjust these values to change the speed of the exploding particles
                var minExplodeSpeed = 100f;
                var maxExplodeSpeed = 150f;
                VoxelParticleSystem.Instance.SpawnBatch(batch, pos => (pos - contactPoint).normalized * Random.Range(minExplodeSpeed, maxExplodeSpeed), gameObject.transform.lossyScale.x);
            }
        }
    }
}
