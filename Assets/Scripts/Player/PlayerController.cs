using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System;

/// <summary>
/// PlayerController inherits NetworkBehaviour
/// This class handle the movement and the shoot of the player in a Network Environment.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [Range(0, 1)]
    public int crew;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    protected ShipController shipController;

    [SyncVar]
    public bool freezeMovement = false;
    public bool IsPilot = false;

    /// <summary>
    /// OnStartLocalPlayer is called when the player is spawning
    /// Just turn the color to diferenciate from other players
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    /// <summary>
    /// When the script starts, we disable other cameras that are not the one associated to the localplayer.
    /// </summary>
	public void Start()
    {
        if (!isLocalPlayer)
        {
            GetComponentInChildren<Camera>().enabled = false;
        }
    }

    /// <summary>
    /// Update Method is called to apply movement and fire
    /// </summary>
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CmdFire();
        }

        /* if (IsPilot)
         {
             var x1 = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
             var z1 = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
             shipController.MoveBoat(x1, z1);
         }*/
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!freezeMovement)
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 10.0f;
            var y = Input.GetAxis("Mouse X") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 10.0f;

            transform.Rotate(0, y, 0);
            transform.Translate(x, 0, z);
        }
    }
    /// <summary>
    /// CmdFire Method is called by client for the serveur
    /// it is telling him to fire a bullet
    /// </summary>
    [Command]
    void CmdFire()
    {
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
    }
}