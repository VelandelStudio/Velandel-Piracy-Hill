using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/** ActivableMechanism, public abstract class
 * @extends : Mechanism
 * This class represents mechanism that shoudl be activated by the player. These mechanisms can not activate themselves.
 **/
public abstract class ActivableMechanism : Mechanism
{
    /** DisplayTextOfInterractable, public override void 
 * We tell to the player which button to press to activate the mechanism
 **/
    public override void DisplayTextOfInterractable()
    {
        Debug.Log("Press E to activate.");
    }

    /** CancelTextOfInterractable, public override void 
	 * Does nothing currently but will handle the Behaviour of the GUI.
	 **/
    public override void CancelTextOfInterractable(Collider other) { }

    /// <summary>
    /// ActivateInterractable, public override void 
	/// If the mechanism is Activable, then we attribute all elements inside the attribute from the player which starts to control it.
    /// Then, we launch the RPCOnActivation that will be handled in the child script.
    ///</summary>
    ///<param name="activatorID"> the Player </param>
    public override void ActivateInterractable(NetworkIdentity activatorID)
    {
        if (IsActivable)
        {
            userId = activatorID;
            IsActivable = false;
            GetComponent<NetworkTransform>().enabled = true;
            parentIdentity = userId.transform.parent.GetComponent<NetworkIdentity>();
            userId.transform.SetParent(transform);

            initialPositionOfUser = userId.transform.position;
            initialRotationOfUser = userId.transform.rotation;
            RpcOnActivation(userId);
        }
    }

    /// <summary>
    /// When the player leaves the Interractable element, we set it back to its original parent (ship).
    /// We also notifies the Mechanism to set it activable then we call the RPC.
    ///</summary>
    public override void LeaveInterractable()
    {
        GetComponent<NetworkTransform>().enabled = true;
        IsActivable = true;
        userId.transform.SetParent(parentIdentity.transform);
        RpcOnLeaving();
    }

    /// <summary>
    /// RPC associated to the ActivateInterractable method. Must be override in children.
    ///</summary>
    [ClientRpc]
    public override abstract void RpcOnActivation(NetworkIdentity activatorID);

    /// <summary>
    /// RPC associated to the LeaveInterractable method. Must be override in children.
    ///</summary>
    [ClientRpc]
    public override abstract void RpcOnLeaving();
}