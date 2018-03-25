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

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    protected ShipController shipController;

    public bool IsPilot = false;
    /// <summary>
    /// OnStartLocalPlayer is called when the player is spawning
    /// Just turn the color to diferenciate from other players
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    void Start()
    {
        /*Transform tr = GameObject.FindWithTag("Ship1").transform;
        transform.parent = tr;*/
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

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!IsPilot)
            {
                CmdSpawnShip();
            }
            else
            {
                CmdRemoveAuthority();
            }
        }

        if (IsPilot)
        {
            Debug.Log("Hello");
            var x1 = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z1 = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
            shipController.MoveBoat(x1, z1);
        }
        else
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

            transform.Rotate(0, x, 0);
            transform.Translate(0, 0, z);
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

    [Command]
    void CmdSpawnShip()
    {
        /*var ship = (GameObject)Instantiate(
            ShipPrefab,
            ShipPrefab.transform.position,
            ShipPrefab.transform.rotation);
        NetworkServer.Spawn(ship);*/
        if (!transform.GetComponentInParent<ShipController>().ShipControlled)
        {
            transform.parent.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            RpcSpawn();
        }
    }

    [Command]
    void CmdRemoveAuthority()
    {
        /*var ship = (GameObject)Instantiate(
            ShipPrefab,
            ShipPrefab.transform.position,
            ShipPrefab.transform.rotation);
        NetworkServer.Spawn(ship);*/

        transform.parent.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToClient);
        RpcDeSpawn();
    }

    [ClientRpc]
    void RpcSpawn()
    {
        shipController = transform.GetComponentInParent<ShipController>();
        shipController.ShipControlled = true;
        IsPilot = true;
    }

    [ClientRpc]
    void RpcDeSpawn()
    {
        shipController = transform.GetComponentInParent<ShipController>();
        shipController.ShipControlled = false;
        IsPilot = false;
    }
}