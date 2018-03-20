using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System;

/// <summary>
/// PlayerController inherits NetworkBehaviour
/// This class handle the movement and the shoot of the player in a Network Environment.
/// </summary>
public class PlayerController : NetworkBehaviour {

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    /// <summary>
    /// OnStartLocalPlayer is called when the player is spawning
    /// Just turn the color to diferenciate from other players
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    /// <summary>
    /// Update Method is called to apply movement and fire
    /// </summary>
    void Update ()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
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
