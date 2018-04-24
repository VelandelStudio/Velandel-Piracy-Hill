using System.Collections.Generic;
using UnityEngine;

namespace VelandelPiracyHill
{
    /// <summary>
    /// Class PlayerShooter permits for the player to shoot
    /// Move crew on a side of the boat to take Canon
    /// And shoot by the side where the crew is
    /// </summary>
    public class PlayerShooter : Photon.PunBehaviour
    {
        [SerializeField] Bullet bulletPrefab;
        [SerializeField] List<Animator> leftCanons;
        [SerializeField] List<Animator> rightCanons;

        int squadPos = 0;

        /// <summary>
        /// Awake just disable scripts which or not the current Client
        /// </summary>
        void Awake()
        {
            enabled = photonView.isMine;
        }

        /// <summary>
        /// Update wait input from the player
        /// it launches RPC to move crew at the left or the right of the boat and set the SideShooter
        /// it also launch RPC to Shoot Bullet from cannons side
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                photonView.RPC("RPC_MoveCrew", PhotonTargets.All, 1);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                photonView.RPC("RPC_MoveCrew", PhotonTargets.All, 2);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                photonView.RPC("RPC_FireBullet", PhotonTargets.All);
            }
        }

        /// <summary>
        /// RPC called by update when fireButton is pressed
        /// Calling fire methode with side of the boat depending on where is the crew
        /// </summary>
        /// <param name="info">The shooter player current Client</param>
        [PunRPC]
        void RPC_FireBullet(PhotonMessageInfo info)
        {
            if (squadPos == 0)
            {
                Debug.Log("Squad Not Set to Canon side");
                return;
            }

            if (squadPos == 1)
            {
                CannonSideShoot(leftCanons, info);
            }

            if (squadPos == 2)
            {
                CannonSideShoot(rightCanons, info);
            }

            Debug.Log("NOW TWIRL, AND TWIRL HONEY TWUUUUUUURL");
        }

        /// <summary>
        /// Just sync the position of the squad on the boat.
        /// </summary>
        /// <param name="pos">Squad side</param>
        [PunRPC]
        void RPC_MoveCrew(int pos)
        {
            squadPos = pos;
        }

        /// <summary>
        /// This method get by the side, the good cannonSide
        /// Depending on which canon is loaded, it play shoot anim and
        /// instantiate a bullet over the network
        /// </summary>
        /// <param name="cannonSide">The list of the sideCannons</param>
        /// <param name="info">The shooter player current Client</param>
        void CannonSideShoot(List<Animator> cannonSide, PhotonMessageInfo info)
        {
            foreach (Animator anim in cannonSide)
            {
                if (anim.GetBool("CannonLoaded"))
                {
                    anim.SetBool("CannonLoaded", false);
                    Debug.Log("Left Fire");

                    Transform canon = anim.GetComponent<Transform>();
                    Transform bulletSpawn = TransformExtensions.FindAnyChild<Transform>(canon, "Bullet Spawn");

                    anim.SetTrigger("StartShooting");

                    var bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
                    bullet.SetOwner(info.photonView);
                    bullet.gameObject.SetActive(true);
                }
            }
        }
    }
}