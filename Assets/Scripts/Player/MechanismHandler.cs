using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// MechanismHandler class, attached to the player that will handle the behaviour when
/// a player is inside a mechanism and when he is using it or not.
/// </summary>
public class MechanismHandler : NetworkBehaviour
{
    public ActivableMechanism UsingMechanism;
    private NetworkIdentity mechanismID;

    /// <summary>
    /// Commands the notify inside mechanism.
    /// we only launch a RPC that will attribute elements to the player inside the mechanism trigger.
    /// </summary>
    /// <param name="mechanism">The mechanism.</param>
    [Command]
    public void CmdNotifyInsideMechanism(NetworkIdentity mechanism)
    {
        RpcInsideMechanism(mechanism);
    }
    /// <summary>
    /// RPC inside mechanism, is an RPC that will set which mechanism we are inside and the instance of it's activable mechanism.
    /// </summary>
    /// <param name="mechanism">The mechanism.</param>
    [ClientRpc]
    private void RpcInsideMechanism(NetworkIdentity mechanism)
    {
        mechanismID = mechanism;
        UsingMechanism = mechanism.GetComponent<ActivableMechanism>();
    }

    /// <summary>
    /// Commands the notify outside mechanism.
    /// we only launch a RPC that will remove elements to the player outside the mechanism trigger.
    /// </summary>
    /// <param name="mechanism">The mechanism.</param>
    [Command]
    public void CmdNotifyOutsideMechanism(NetworkIdentity mechanism)
    {
        if (mechanism.hasAuthority)
        {
            CmdQuitMechanism();
        }
        RpcOutsideMechanism();
    }
    /// <summary>
    /// RPC outside mechanism, is an RPC that will reset the values of the player going out of the mechanism.
    /// </summary>
    /// <param name="mechanism">The mechanism.</param>
    [ClientRpc]
    private void RpcOutsideMechanism()
    {
        UsingMechanism = null;
        mechanismID = null;
    }


    /// <summary>
    /// On Update, we check if the player is the local one and if we can use a mechanism. this instance.
    /// If the player cans and if he presses E, then we activate the mechanism.
    /// If the player is already using a mechanism and press E we leave the mechanism.
    /// </summary>
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (mechanismID && Input.GetKeyDown(KeyCode.E))
        {
            if (UsingMechanism.IsActivable)
            {
                CmdUseMechanism();
            }

            if (UsingMechanism.hasAuthority)
            {
                CmdQuitMechanism();
            }
        }
    }

    /// <summary>
    /// Commands the use of a mechanism.
    /// When a player starts using a mechanism (by pressing E), we assign to him the authority of the mechanism.
    /// then, we Activate the Interractable.
    /// </summary>
    [Command]
    private void CmdUseMechanism()
    {
        mechanismID.AssignClientAuthority(connectionToClient);
        UsingMechanism.ActivateInterractable(GetComponent<NetworkIdentity>());
    }

    /// <summary>
    /// Commands the quit of a mechanism.
    /// When a player stops using a mechanism (by pressing E), we Leave the Interractable,
    /// then, we remove to him the authority of the mechanism.
    /// </summary>
    [Command]
    private void CmdQuitMechanism()
    {
        UsingMechanism.LeaveInterractable();
        mechanismID.RemoveClientAuthority(connectionToClient);
    }

    /// <summary>
    /// Commands the quit of a mechanism.
    /// When a player stops using a mechanism (by pressing E), we Leave the Interractable,
    /// then, we remove to him the authority of the mechanism.
    /// </summary>
    [Command]
    private void CmdExpulseMechanism()
    {
        UsingMechanism.ExpulsedFromInterractable();
        mechanismID.RemoveClientAuthority(connectionToClient);
    }
}