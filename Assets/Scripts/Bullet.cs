using UnityEngine;

namespace VelandelPiracyHill
{
    /// <summary>
    /// Bullet class is the behavior of the bullet shooted with a cannon
    /// this is a Monobehavior because is instantiate by RPC
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField] ParticleSystem trail;
        [SerializeField] ParticleSystem explosion;
        [SerializeField] Rigidbody rbody;

        PlayerShooter bulletOwner;

        float speed = 20f;

        private void Start()
        {
            rbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Fixedupdate to calculate the homogeneous movement of the bullet
        /// </summary>
        private void FixedUpdate()
        {
            transform.rotation = Quaternion.LookRotation(rbody.velocity);
            if (transform.position.y < -50)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Attribute the bullet the shooter current client
        /// </summary>
        /// <param name="ownerView"></param>
        public void SetOwner(PhotonView ownerView)
        {
            bulletOwner = ownerView.GetComponent<PlayerShooter>();
        }
        
        /// <summary>
        /// Do what happened when the bullet collide another object
        /// </summary>
        /// <param name="other">BoatPlayer or environment</param>
	private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag != "Player")
            {
                DestroyBullet();
            }
        }

        /// <summary>
        /// Handle the correct destruction of the bullet
        /// detach particules system and play explosions
        /// then destroy the gameObject
        /// </summary>
        private void DestroyBullet()
        {
            trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            explosion.transform.SetParent(null);
            explosion.Play();
            Destroy(explosion.gameObject, 3);
            Destroy(gameObject);
        }
    }
}