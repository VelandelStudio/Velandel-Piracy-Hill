using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VelandelPiracyHill
{
    /// <summary>
    /// </summary>
    public class Bullet : MonoBehaviour
    {

        [SerializeField] ParticleSystem explosion;
        [SerializeField] ParticleSystem trail;
        [SerializeField] Rigidbody rbody;

        PlayerShooter bulletOwner;
        float speed = 60f;

        private void FixedUpdate()
        {
            rbody.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
        }

        public void SetOwner(PhotonView ownerView)
        {
            bulletOwner = ownerView.GetComponent<PlayerShooter>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Collision");

            DestroyBullet();
        }

        private void DestroyBullet()
        {
            throw new NotImplementedException();
        }
    }
}

