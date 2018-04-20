using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The CanonController is an ActivableMechanism
/// When Player uses the mecanism, the canon can shoot CanonBall
/// </summary>
public class CanonController : ActivableMechanism
{
    public Transform shootPlace;
    public Transform canonBallSpawn;
    public GameObject canonBallPrefab;

    /// <summary>
    /// RpcOnActivation is launched by the ActivateInterractable Method (Template pattern)
    /// The RPC is only used to place the player on the canon.
    /// </summary>
    /// <param name="activatorID">Player</param>
    [ClientRpc]
    public override void RpcOnActivation(NetworkIdentity activatorID)
    {
        GetComponent<NetworkTransform>().enabled = true;
        activatorID.transform.SetParent(transform);
        activatorID.transform.position = shootPlace.position;
        activatorID.transform.LookAt(transform);
        //activatorID.GetComponent<PlayerController>().freezeMovement = true;
    }

    /// <summary>
    /// RpcOnActivation is launched by the LeaveInterractable Method (Template pattern)
    /// The RPC is only used to place the player at his position before he started using the canon.
    /// </summary>
    /// <param name="activatorID">Player</param>
    [ClientRpc]
    public override void RpcOnLeaving()
    {
        GetComponent<NetworkTransform>().enabled = false;
        userId.transform.position = initialPositionOfUser;
        userId.transform.rotation = initialRotationOfUser;
        userId.transform.SetParent(parentIdentity.transform);
        //userId.GetComponent<PlayerController>().freezeMovement = false;
    }

    /// <summary>
    /// RpcOnActivation is launched by the LeaveInterractable Method (Template pattern)
    /// The RPC is only used to place the player at his position before he started using the canon.
    /// </summary>
    /// <param name="activatorID">Player</param>
    [ClientRpc]
    public override void RpcOnExpulsing()
    {
        GetComponent<NetworkTransform>().enabled = false;
        userId.transform.SetParent(parentIdentity.transform);
        //userId.GetComponent<PlayerController>().freezeMovement = false;
    }

    /// <summary>
    /// Update method is here to know if the fire command is used.
    /// First of all, we ensure that someone isUsing the Canon and if the user is the local player.
    /// If that's the case, we check if the localPlayer has the Authority on the Canon. 
    /// If it is true, we launch the CmdFire.
	/// Also, when a player has authority on the Canon, he is able to make it rotate on the horizontal side (-10 to +10 degrees) 
	/// And on the Vertical side (+25 degrees max, we can not go under 0 degrees)
    /// </summary>
    protected void Update()
    {
        if (!userId || !userId.isLocalPlayer)
        {
            return;
        }

        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("ET QUAND IL RUGIT IL FAIT CE BRUIT CI");
                Debug.Log("BOOM BOOM !");
                CmdFire();
            }

            var yRotation = Input.GetAxis("Mouse X");
            Quaternion localRotation = transform.localRotation;
            var yEulerRotation = localRotation.eulerAngles.y + yRotation;
            if (yEulerRotation > 180 && yEulerRotation < 350)
            {
                yEulerRotation = 350;
            }
            else if (yEulerRotation < 180 && yEulerRotation > 10)
            {
                yEulerRotation = 10;
            }

            var zRotation = Input.GetAxis("Mouse Y");
            var zEulerRotation = localRotation.eulerAngles.z + zRotation;
            zEulerRotation = Mathf.Clamp(zEulerRotation, 0f, 20f);

            localRotation.eulerAngles = new Vector3(0, yEulerRotation, zEulerRotation);
            transform.localRotation = localRotation;
        }
    }

    /// <summary>
    /// CmdFire Method is called by client for the serveur
    /// it is telling him to fire a boulet
    /// </summary>
    [Command]
    void CmdFire()
    {
        var boulet = (GameObject)Instantiate(
            canonBallPrefab,
            canonBallSpawn.position,
            canonBallSpawn.rotation);

        boulet.GetComponent<Rigidbody>().velocity = boulet.transform.forward * 50;
        NetworkServer.Spawn(boulet);
        Destroy(boulet, 4.0f);
    }
}