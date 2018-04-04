using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaCollider : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDestructibleElement>() != null)
        {
            IDestructibleElement element = other.GetComponent<IDestructibleElement>();
            element.OnElementTouchWater();
        }
    }
}