using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : ActivableMechanism
{
    [Range(0, 1)]
    public int crew;
    [SyncVar]
    public bool ShipControlled = false;
    public Transform barre;

    public override void ActivateInterractable(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();

        if (other.tag == "Player" && playerController && playerController.crew == crew)
        {
            other.transform.position = barre.position;
            other.transform.LookAt(transform.forward);

            other.transform.parent = transform;
            playerController.BoatControl();
        }
    }

    public void MoveBoat(float x, float z)
    {
        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
    }
}