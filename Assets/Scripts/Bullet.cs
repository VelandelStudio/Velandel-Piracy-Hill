using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bullet Class
/// Simply call the OnCollisionEnter callback and apply damage
/// If the collision doesn't have Health script, it is just destroy
/// </summary>
public class Bullet : MonoBehaviour {

    private void OnCollisionEnter(Collision collision)
    {
        var hit = collision.gameObject;
        var health = hit.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(10);
        }

        Destroy(gameObject);
    }
}
