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


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        if (!photonView.isMine)
        {
            enabled = false;
        }
    }


    private void OnEnable()
    {
        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }

    private void Update()
    {
        // Store the value of both input axes.
        m_MovementInputValue = Input.GetAxis("Vertical");
        m_TurnInputValue = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Move();
        Turn();
        //m_Rigidbody.velocity = Vector3.zero;
        //m_Rigidbody.angularVelocity = Vector3.zero;
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
}
