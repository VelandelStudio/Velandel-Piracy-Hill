using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : Photon.PunBehaviour
{
    public float m_Speed = 12f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.

    private float animVelocity = 0f;
    private float animVelocity5 = 0f;
    [SerializeField] private Animator shipVoxelAC;

    [SerializeField] private ParticleSystem[] PS_WaterFoam;
    private bool PSPlaying;

    private void Awake()
    {
        if (!photonView.isMine)
        {
            return;
        }

        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (!photonView.isMine)
        {
            return;
        }

        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable()
    {
        if (!photonView.isMine)
        {
            return;
        }

        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }

    /// <summary>
    /// Update the stats of the ship movemement.
    /// When the ship is moving, we start the PS associated to the ship (water drops).
    /// If he is not moving, we stop the PS. Every thing is passed only one time via RPC thanks to the PSPlaying bool.
    /// </summary>
    private void Update()
    {
        if (!photonView.isMine)
        {
            return;
        }

        // Store the value of both input axes.
        m_MovementInputValue = Mathf.Clamp(Input.GetAxis("Vertical"),0,1);
        m_TurnInputValue = Input.GetAxis("Horizontal");
        
        float movementValue = Mathf.Abs(m_Rigidbody.velocity.x + m_Rigidbody.velocity.z);
        if (!PSPlaying && movementValue > 1)
        {
            PSPlaying = true;
            photonView.RPC("StartPSBehaviour", PhotonTargets.All);
        }
        else if (PSPlaying && movementValue <= 1)
        {
            PSPlaying = false;
            photonView.RPC("StopPSBehaviour", PhotonTargets.All);
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.isMine)
        {
            return;
        }

        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Move();
        Turn();
        //m_Rigidbody.velocity = Vector3.zero;
        //m_Rigidbody.angularVelocity = Vector3.zero;
        Animate();
    }

    private void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on
        // the input, speed and the time between frames.
        Vector3 movement = new Vector3(0f, 0f, m_MovementInputValue * Time.deltaTime);

        // Apply this movement to the rigidbody's position.
        m_Rigidbody.AddRelativeForce(movement * m_Speed);

        //m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Turn()
    {
        // Determine the number of degrees to be turned based on the input,
        // speed and time between frames.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        // Make this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    private void Animate()
    {
        shipVoxelAC.SetFloat("Rotation", Mathf.SmoothDamp(shipVoxelAC.GetFloat("Rotation"), Input.GetAxisRaw("Horizontal"), ref animVelocity5, 1f));

        int modulo = Input.GetAxisRaw("Vertical") > 0 ? 1 : 0;
        shipVoxelAC.SetFloat("Movement", Mathf.SmoothDamp(shipVoxelAC.GetFloat("Movement"), modulo, ref animVelocity, 1f));
    }

    /// <summary>
    /// StartPSBehaviour
    /// RPC that starts the PS associated to the ship when he is moving (water drops)
    /// </summary>
    [PunRPC]
    private void StartPSBehaviour()
    {
        for (int i = 0; i < PS_WaterFoam.Length; i++)
        {
            PS_WaterFoam[i].Play(true);
        }
    }

    /// <summary>
    /// StopPSBehaviour
    /// RPC that stops the PS associated to the ship when he is not moving (water drops)
    /// </summary>
    [PunRPC]
    private void StopPSBehaviour()
    {        
        for (int i = 0; i < PS_WaterFoam.Length; i++)
        {
            PS_WaterFoam[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}