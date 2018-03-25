using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour
{

    GameObject controllerPlayer;
    [SyncVar]
    public bool ShipControlled = false;

    public void MoveBoat(float x, float z)
    {
        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
    }
}