using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The CanonController is an ActivableMechanism
/// When Player uses the mecanism, the canon can shoot CanonBall
/// </summary>
public class CannonController : Photon.MonoBehaviour
{
    public ParticleSystem ShootOrderPS;
    private Animator cannonAC;

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private float nextTimerChange;
    private float velocity = 0.0F;

    public float NextShootTimer;

    private void Start()
    {
        cannonAC = GetComponent<Animator>();
        nextTimerChange = Random.Range(1f, 3f);
        Invoke("MoveToTargetRotation", nextTimerChange);
        InvokeRepeating("RandNextShootTimer", 0f, 10f);
    }

    private void Update()
    {
        SmoothApplyRotation();
    }

    private void MoveToTargetRotation()
    {
        verticalRotation = Random.Range(0f, 1f);
        horizontalRotation = Random.Range(-1f, 1f);
        nextTimerChange = Random.Range(1f, 3f);
        Invoke("MoveToTargetRotation", nextTimerChange);
    }

    private void SmoothApplyRotation()
    {
        cannonAC.SetFloat("VerticalRotation", Mathf.SmoothDamp(cannonAC.GetFloat("VerticalRotation"), verticalRotation, ref velocity, 0.2f));
        cannonAC.SetFloat("HorizontalRotation", Mathf.SmoothDamp(cannonAC.GetFloat("HorizontalRotation"), horizontalRotation, ref velocity, 0.2f));
    }

    private void RandNextShootTimer()
    {
        NextShootTimer = Random.Range(0.5f, 1.5f);
    }
}