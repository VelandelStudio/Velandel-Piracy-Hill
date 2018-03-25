using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ManagerGame : NetworkBehaviour
{
    public GameObject Canon;

    public override void OnStartServer()
    {
        Debug.Log("hello world");
        var canon = Instantiate(
                Canon,
                Canon.transform.position,
                Canon.transform.rotation);
        NetworkServer.Spawn(canon);
    }
}