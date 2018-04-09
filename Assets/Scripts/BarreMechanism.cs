using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BarreMechanism : ActivableMechanism
{
    public Transform barrePlace;

    [ClientRpc]
    public override void RpcOnActivation(NetworkIdentity activatorID)
    {
        GetComponent<NetworkTransform>().enabled = true;
        activatorID.transform.position = barrePlace.position;
        activatorID.transform.LookAt(transform);
        activatorID.GetComponent<PlayerController>().freezeMovement = true;

        ShipController shipController = GetComponentInParent<ShipController>();
        shipController.userId = userId;
    }

    [ClientRpc]
    public override void RpcOnLeaving()
    {
        GetComponent<NetworkTransform>().enabled = false;
        userId.transform.position = initialPositionOfUser;
        userId.transform.rotation = initialRotationOfUser;
        userId.GetComponent<PlayerController>().freezeMovement = false;

        ShipController shipController = GetComponentInParent<ShipController>();
        shipController.userId = null;

    }

    [ClientRpc]
    public override void RpcOnExpulsing()
    {
        GetComponent<NetworkTransform>().enabled = false;
        userId.GetComponent<PlayerController>().freezeMovement = false;

        ShipController shipController = GetComponentInParent<ShipController>();
        shipController.userId = null;
    }
}
