using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ShipBlock , implements IDestructibleElement interface
/// This script is attached to everyblock on the ship that can be exploded.
/// </summary>
public class ShipBlock : MonoBehaviour, IDestructibleElement
{
    NetworkTransformChild transformChild;

    /// <summary>
    /// Launched when the block is hit by a bullet.
    /// We enable the NetworkTransformChild on the parent.
    /// </summary>
    public void OnElementHit()
    {
        GetComponent<NetworkTransform>().enabled = true;
        Invoke("SetTrigger", 0.5f);
        Destroy(gameObject, 5);
    }

    private void SetTrigger()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>
    /// Launched when the block touches the water collider
    /// We destroy the NetworkTranformChild and the GameObject.
    /// </summary>
    public void OnElementTouchWater()
    {
        Destroy(transformChild, 5);
        Destroy(gameObject, 5);
    }
}