using PicaVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleElement : MonoBehaviour {
    [SerializeField] private Exploder exploder;

    public void Explode(Vector3 contactPoint)
    {
        exploder.transform.position = contactPoint;
        exploder.transform.position -= new Vector3(0f, 0.25f, 0f);
        exploder.Explode(new Vector3(0f, 10f, 0f), gameObject.transform.lossyScale.x);
    }
}
