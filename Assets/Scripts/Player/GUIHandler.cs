using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GUIHandler : NetworkBehaviour
{

    public override void OnStartLocalPlayer()
    {
        GameObject.Find("Canvas").GetComponent<GUIManager>().localPlayerObj = this;
    }

    [Command]
    public void CmdAttributeTeam(GameObject ship)
    {
        gameObject.transform.SetParent(ship.transform);
        gameObject.transform.position = ship.transform.position + Vector3.up * 2;
        ship.gameObject.SetActive(false);
        NetworkTransformChild Nchild = ship.gameObject.AddComponent<NetworkTransformChild>();
        Nchild.target = gameObject.transform;
        ship.gameObject.SetActive(true);

        RpcAttributeTeam(ship);
    }

    [ClientRpc]
    private void RpcAttributeTeam(GameObject ship)
    {
        gameObject.transform.SetParent(ship.transform);
        gameObject.transform.position = ship.transform.position + Vector3.up * 2;
    }
}