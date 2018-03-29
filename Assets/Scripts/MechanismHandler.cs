using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MechanismHandler : NetworkBehaviour 
{	
	public ActivableMechanism UsingMechanism;	
	private NetworkIdentity mechanismID;
	
	[Command]
    public void CmdNotifyInsideMechanism(NetworkIdentity mechanism)
	{
		RpcInsideMechanism(mechanism);
	}
	[ClientRpc]
	private void RpcInsideMechanism(NetworkIdentity mechanism)
	{
		mechanismID = mechanism;
		UsingMechanism = mechanism.GetComponent<ActivableMechanism>();
	}
	
	[Command]
	public void CmdNotifyOutsideMechanism(NetworkIdentity mechanism)
	{		
		RpcOutsideMechanism();
	}
	[ClientRpc]
	private void RpcOutsideMechanism()
	{
		UsingMechanism = null;
		mechanismID = null;
	}
	
	
	private void Update()
	{
		if(!isLocalPlayer)
		{
			return;
		}
		
		if(mechanismID && Input.GetKeyDown(KeyCode.E))
		{
			if(UsingMechanism.IsActivable)
			{
				CmdUseMechanism();
			}
            Debug.Log(UsingMechanism.hasAuthority);
			if(UsingMechanism.hasAuthority)
			{
				CmdQuitMechanism();
			}
		}
	}
	
	[Command]
	private void CmdUseMechanism()
	{	
		mechanismID.AssignClientAuthority(connectionToClient);
		UsingMechanism.ActivateInterractable(GetComponent<NetworkIdentity>());
	}
	
	[Command]
	private void CmdQuitMechanism()
	{	
        UsingMechanism.LeaveInterractable();
        mechanismID.RemoveClientAuthority(connectionToClient);
	}
}
