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
    /// The Awake lethod is used to disable the Ship gameObject and associate a NetworkTransformChild to it.
    /// When added, we set the transform to the shipblock and we disable the NetworktransformChil component because we do not need it
    ///	until the block is touched by a bullet. The we enable the ship back. 
    /// </summary>
    protected void Awake()
    {
        GameObject ship = GetComponentInParent<ShipController>().gameObject;
        ship.SetActive(false);
        transformChild = ship.AddComponent<NetworkTransformChild>();
        transformChild.target = transform;
        //transformChild.sendRate = 25f;
        transformChild.enabled = false;
        ship.SetActive(true);
    }

    /// <summary>
    /// Launched when the block is hit by a bullet.
    /// We enable the NetworkTransformChild on the parent.
    /// </summary>
    public void OnElementHit()
    {
        if (gameObject.GetComponent<NetworkTransform>() == null)
        {
            transformChild.enabled = true;
        }
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
