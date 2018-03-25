using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CanonController : ActivableMechanism {

    public Transform shootPlace;
    public Transform canonBallSpawn;
    public GameObject canonBallPrefab;

    [SyncVar]
    private bool isUsed = false;
    private PlayerController playerUsingCanon;

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
    /// it is telling him to fire a bullet
    /// </summary>
    [Command]
    void CmdFire()
    {
        var boullet = (GameObject)Instantiate(
            canonBallPrefab,
            canonBallSpawn.position,
            canonBallSpawn.rotation);

        boullet.GetComponent<Rigidbody>().velocity = boullet.transform.forward * 20;
        NetworkServer.Spawn(boullet);
        Destroy(boullet, 4.0f);
    }
}
