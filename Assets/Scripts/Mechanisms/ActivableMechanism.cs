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

    /** ActivateInterractable, public override abstract void 
	 * The behaviour of the ActivableMechanism will be set in the child script.
	 **/
    public override void ActivateInterractable(NetworkIdentity activatorID)
    {
        if (IsActivable)
        {
            userId = activatorID;
            IsActivable = false;
            initialPositionOfUser = activatorID.transform.position;
            initialRotationOfUser = activatorID.transform.rotation;
            RpcOnActivation(activatorID);
        }
    }

    public override void LeaveInterractable()
    {
        IsActivable = true;
        RpcOnLeaving();
    }

    /** ActivateInterractable, public override abstract void 
	 * The behaviour of the ActivableMechanism will be set in the child script.
	 **/
    [ClientRpc]
    public override abstract void RpcOnActivation(NetworkIdentity activatorID);

    [ClientRpc]
    public override abstract void RpcOnLeaving();
}