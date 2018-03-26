using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The CanonController is an ActivableMechanism
/// When Player uses the mecanism, the canon can shoot CanonBall
/// </summary>
public class CanonController : ActivableMechanism {

    public Transform shootPlace;
    public Transform canonBallSpawn;
    public GameObject canonBallPrefab;

    [SyncVar]
    private bool isUsed = false;
    private PlayerController playerUsingCanon;

    /// <summary>
    /// ActivateInterractable is to assigned a Player who press E button to the canon
    /// Placing him and tell the it is used.
    /// </summary>
    /// <param name="other">Player</param>
    public override void ActivateInterractable(Collider other)
    {
        if (other.tag == "Player" && !isUsed)
        {
            other.transform.position = shootPlace.position;
            other.transform.LookAt(transform);
            playerUsingCanon = other.GetComponent<PlayerController>();
            isUsed = true;
        }
    }

    /// <summary>
    /// Update method is here to know if the fire command is used.
    /// If it is the case, we check if a LocalPlayer is using the canon and then the canon fires.
    /// Next refacto will move this to the mother class.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerUsingCanon != null && !(playerUsingCanon.isLocalPlayer))
            {
                return;
            }

            CmdFire();
            Debug.Log("Tiré moussaillon");
        }
    }

    /// <summary>
    /// CmdFire Method is called by client for the serveur
    /// it is telling him to fire a boullet
    /// </summary>
    [Command]
    void CmdFire()
    {
        var boulet = (GameObject)Instantiate(
            canonBallPrefab,
            canonBallSpawn.position,
            canonBallSpawn.rotation);

        boulet.GetComponent<Rigidbody>().velocity = boulet.transform.forward * 20;
        NetworkServer.Spawn(boulet);
        Destroy(boulet, 4.0f);
    }
}
