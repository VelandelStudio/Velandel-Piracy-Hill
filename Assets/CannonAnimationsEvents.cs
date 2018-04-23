using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonAnimationsEvents : MonoBehaviour {

	[SerializeField] ParticleSystem PS_CannonShoot;
	public void StartShooting()
	{
		Animator playerAnim = GetComponentInChildren<PirateAnimationsEvents>().GetComponent<Animator>();
		PS_CannonShoot.Play();
		playerAnim.SetTrigger("StartShooting");
	}
}
