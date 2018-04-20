using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BarreMechanism : ActivableMechanism
{
    public Transform barrePlace;
    [SyncVar]
    public NetworkIdentity shipID; 

    [ClientRpc]
    public override void RpcOnActivation(NetworkIdentity activatorID)
    {
        activatorID.transform.SetParent(transform);
        activatorID.transform.position = barrePlace.position;
        activatorID.transform.LookAt(transform);
        //activatorID.GetComponent<PlayerController>().freezeMovement = true;

        //ShipController shipController = GetComponentInParent<ShipController>();
        //shipController.userId = userId;
        CmdAssignAuthorityToShip();
    }

    [Command]
    private void CmdAssignAuthorityToShip()
    {
        shipID.AssignClientAuthority(userId.connectionToClient);
    }

    [ClientRpc]
    public override void RpcOnLeaving()
    {
        userId.transform.position = initialPositionOfUser;
        userId.transform.rotation = initialRotationOfUser;
        //userId.GetComponent<PlayerController>().freezeMovement = false;
        userId.transform.SetParent(parentIdentity.transform);

        //ShipController shipController = GetComponentInParent<ShipController>();
        //shipController.userId = null;

    }

    [ClientRpc]
    public override void RpcOnExpulsing()
    {
        //userId.GetComponent<PlayerController>().freezeMovement = false;

        //ShipController shipController = GetComponentInParent<ShipController>();
        //shipController.userId = null;
    }
}
