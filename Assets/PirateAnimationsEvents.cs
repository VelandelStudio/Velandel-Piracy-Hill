using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class PirateAnimationsEvents : MonoBehaviour {

	[SerializeField] private GameObject cannonBallAnim;
	[SerializeField] private GameObject cannonBallSlot;
	private Transform cannonBallSlotParent;	
	
	public void StartReloadingCannon()
	{
		Animator cannonAnimator = transform.parent.GetComponent<Animator>();
		cannonAnimator.SetTrigger("StartReloading");
	}
	
	public void EndReloadingCannon()
	{
		Animator cannonAnimator = transform.parent.GetComponent<Animator>();
		cannonAnimator.SetTrigger("EndReloading");
	}
	
	public void PickUpNewCannonBall()
	{
		Instantiate(cannonBallAnim, cannonBallSlot.transform);
	}
	
	public void DropCannonBall()
	{
		cannonBallSlotParent = cannonBallSlot.transform.parent;
		cannonBallSlot.transform.SetParent(null);
	}
	
	public void PickUpCannonBall()
	{
		cannonBallSlot.transform.SetParent(cannonBallSlotParent);
	}
	
	public void DestroyHeldObject()
	{
		Destroy(cannonBallSlot.transform.GetChild(0).gameObject);
	}

    public void NotifyCannonLoaded()
    {
        Animator cannonAnimator = transform.parent.GetComponent<Animator>();
        cannonAnimator.SetBool("CannonLoaded", true);
    }
}
