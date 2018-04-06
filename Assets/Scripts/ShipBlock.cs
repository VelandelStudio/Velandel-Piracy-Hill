using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ShipBlock , implements IDestructibleElement interface
/// This script is attached to everyblock on the ship that can be exploded.
/// </summary>
public class ShipBlock : NetworkBehaviour, IDestructibleElement
{

  /*  private void Start()
    {
        int x  = Mathf.RoundToInt(transform.localPosition.x);
        int y = Mathf.RoundToInt(transform.localPosition.y);
        int z = Mathf.RoundToInt(transform.localPosition.z);
        transform.localPosition = new Vector3(x,y,z);

    }*/

    /// <summary>
    /// Launched when the block is hit by a bullet.
    /// We enable the NetworkTransformChild on the parent.
    /// </summary>
    public void OnElementHit()
    {
        //GetComponent<NetworkTransform>().enabled = true;
        Invoke("SetTrigger", 1f);
        Invoke("OnElementTouchWater", 10f);
    }

    private void SetTrigger()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>
    /// This method is launched when the shipBlock is trigger and when it collides a player.
    /// We apply a force to a the Pkayer when the collision is detected. Also, we apply a force on the shipBlock backward.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            Rigidbody playerRB = other.GetComponent<Rigidbody>();
            Rigidbody shipBlockRB = GetComponent<Rigidbody>();
            playerRB.AddForce(transform.forward * 20f, ForceMode.Impulse);
            shipBlockRB.AddForce(-transform.forward * 20f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Launched when the block touches the water collider
    /// We destroy the NetworkTranformChild and the GameObject.
    /// </summary>
    public void OnElementTouchWater()
    {
        NetworkServer.Destroy(gameObject);
    }
}