using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ManagerGame : NetworkBehaviour
{

    public GameObject Ship;
    public override void OnStartServer()
    {
        Debug.Log("hello world");
        var ship = (GameObject)Instantiate(
                Ship,
                Ship.transform.position,
                Ship.transform.rotation);
        NetworkServer.Spawn(ship);
    }
}