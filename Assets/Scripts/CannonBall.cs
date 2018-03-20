using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

    [SerializeField] protected float areaOfEffect;
    [SerializeField] protected float explosionForce;

    protected virtual void Start()
    {
        areaOfEffect = areaOfEffect == 0 ? 3 : areaOfEffect;
        explosionForce = explosionForce == 0 ? 500 : explosionForce;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(collision.contacts[0].point, areaOfEffect);
        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
            if(rb)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(500, transform.position,5, 0f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
