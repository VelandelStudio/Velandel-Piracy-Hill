using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ManagerGame : NetworkBehaviour
{
    public GameObject Canon;
    public GameObject Ship;
    public Transform spawCrew0;
    public Transform spawCrew1;

    public override void OnStartServer()
    {
        Debug.Log("hello world");

        var canon = Instantiate(
                Canon,
                Canon.transform.position,
                Canon.transform.rotation);
        NetworkServer.Spawn(canon);

        var ship0 = Instantiate(
                Ship,
                spawCrew0.position,
                spawCrew0.rotation);
        ship0.GetComponent<ShipController>().crew = 0;
        ship0.name = "ship0";
        NetworkServer.Spawn(ship0);

        var ship1 = Instantiate(
                Ship,
                spawCrew1.position,
                spawCrew1.rotation);
        ship1.GetComponent<ShipController>().crew = 1;
        ship1.name = "ship1";
        NetworkServer.Spawn(ship1);
    }
}