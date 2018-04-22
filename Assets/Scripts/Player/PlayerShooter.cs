using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class PlayerShooter : Photon.PunBehaviour
    {
        [SerializeField] Bullet bulletPrefab;

        float nextFireTimeLeft;
        float nextFireTimeRight;

        float fireDelay = 0.25f;

        void Awake()
        {
            enabled = photonView.isMine;
        }

        private void Update()
        {
            if (Time.time < nextFireTimeLeft && Time.deltaTime < nextFireTimeRight)
            {
                return;
            }

            if (Time.deltaTime < nextFireTimeLeft && Input.GetAxis("FireLeft") > 0)
            {
                nextFireTimeLeft = Time.time + fireDelay;
                photonView.RPC("RPC_Fire", PhotonTargets.All);
            }
            
            if (Time.deltaTime < nextFireTimeRight && Input.GetAxis("FireRight") > 0)
            {
                nextFireTimeRight = Time.time + fireDelay;
                photonView.RPC("RPC_Fire", PhotonTargets.All);
            }
        }

        [PunRPC]
        void RPC_FireBullet(PhotonMessageInfo info)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            //bullet.SetOwner(info.photonView);
            bullet.gameObject.SetActive(true);
        }
    }
}

