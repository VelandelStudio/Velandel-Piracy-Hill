using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// CannonBall public class
/// This script is attached to a CannonBall Prefab
/// When we hit an element, we apply an explosions on hit rigidbodies
/// </summary>
public class CannonBall : MonoBehaviour
{
    [SerializeField] protected float areaOfEffect;
    [SerializeField] protected float explosionForce;

    /// <summary>
    /// Starts this instance.
    /// If the Area of effect or the Explosion force are not set in the editor, we attribute to them a default value
    /// </summary>
    protected virtual void Start()
    {
        areaOfEffect = areaOfEffect == 0 ? 3 : areaOfEffect;
        explosionForce = explosionForce == 0 ? 500 : explosionForce;
    }

    /// <summary>
    /// Called when [collision enter].
    /// When a collision is detected, we surround an overlapSphere and add force to the rigidbody inside this sphere.
    /// Every RB in the sphere will have a isKinematic = false;
	/// If the component hit is a IDestructibleElement, we call its OnElementHit Method.
    /// After this, we Destroy the CannonBall
    /// </summary>
    /// <param name="collision">The collision.</param>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(collision.contacts[0].point, areaOfEffect);
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
            if (rb)
            {
                if (rb.GetComponent<IDestructibleElement>() != null)
                {
                    rb.GetComponent<IDestructibleElement>().OnElementHit();
                }

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddExplosionForce(100, transform.position, 10, 0f, ForceMode.VelocityChange);
            }
        }
        NetworkServer.Destroy(gameObject);
    }
}