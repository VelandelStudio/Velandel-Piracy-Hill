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

    [SyncVar]
    public NetworkInstanceId parentNetId;

    public Transform shootPlace;
    public Transform canonBallSpawn;
    public GameObject canonBallPrefab;

    /// <summary>
    /// When the client Start, we locate the parentNetId set privously by the GameManager and 
    /// attribute the Canon prefab as child of its parent.
    ///	In this way, we have a perfectly synchronized scene Server/Client.
    /// </summary>
    public override void OnStartClient()
    {
        //GameObject parentObject = ClientScene.FindLocalObject(parentNetId);
        //transform.SetParent(parentObject.transform);
    }

    /// <summary>
    /// RpcOnActivation is launched by the ActivateInterractable Method (Template pattern)
	/// The RPC is only used to place the player on the canon.
    /// </summary>
    /// <param name="activatorID">Player</param>
	[ClientRpc]
    public override void RpcOnActivation(NetworkIdentity activatorID)
    {
        activatorID.transform.position = shootPlace.position;
        activatorID.transform.LookAt(transform);
    }

    /// <summary>
    /// RpcOnActivation is launched by the LeaveInterractable Method (Template pattern)
    /// The RPC is only used to place the player at his position before he started using the canon.
    /// </summary>
    /// <param name="activatorID">Player</param>
    [ClientRpc]
    public override void RpcOnLeaving()
    {
        userId.transform.position = initialPositionOfUser;
        userId.transform.rotation = initialRotationOfUser;
    }

    /// <summary>
    /// Update method is here to know if the fire command is used.
    /// First of all, we ensure that someone isUsing the Canon and if the user is the local player.
    /// If that's the case, we check if the localPlayer has the Authority on the Canon. 
    /// If it is true, we launch the CmdFire.
    /// </summary>
    protected void Update()
    {
        if (!userId || !userId.isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && hasAuthority)
        {
            Debug.Log("ET QUAND IL RUGIT IL FAIT CE BRUIT CI");
            Debug.Log("BOOM BOOM !");
            CmdFire();
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

        boulet.GetComponent<Rigidbody>().velocity = boulet.transform.forward * 20;
        NetworkServer.Spawn(boulet);
        Destroy(boulet, 4.0f);
    }
}