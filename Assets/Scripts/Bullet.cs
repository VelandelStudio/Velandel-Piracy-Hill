using UnityEngine;

namespace VelandelPiracyHill
{
    /// <summary>
    /// Bullet class is the behavior of the bullet shooted with a cannon
    /// this is a Monobehavior because is instantiate by RPC
    /// </summary>
    public class Bullet : Photon.MonoBehaviour
    {
        [SerializeField] ParticleSystem firetrail;
        [SerializeField] ParticleSystem smoketrail;
        [SerializeField] ParticleSystem explosion;
        [SerializeField] Rigidbody rbody;

        PlayerShooter bulletOwner;

        float speed = 20f;

        /// <summary>
        /// Start is used to add the initial force of the cannonball.
        /// </summary>
        private void Start()
        {
            rbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Fixedupdate to calculate if we should detroy this bullet underwater.
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
        /// The explosion is calculated on the receiver and sended via RPC to all other client.
        /// The Explosion does not occure if the bullet owner is the receiver (i.e. the bullet hits my own ship)
        /// </summary>
        /// <param name="other">PlayerShip or environment</param>
        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.tag == "Player")
            {
                PlayerShip otherShip = other.transform.root.GetComponent<PlayerShip>();
                if (bulletOwner.photonView.viewID == otherShip.photonView.viewID)
                    return;

                if (otherShip.photonView.isMine)
                {
                    float contactX = other.contacts[0].point.x;
                    float contactY = other.contacts[0].point.y;
                    float contactZ = other.contacts[0].point.z;
                    otherShip.photonView.RPC("RPC_ExplodeContactPoint", PhotonTargets.All, contactX, contactY, contactZ);
                }
            }

            DestructibleElement element = other.transform.root.GetComponent<DestructibleElement>();
            if (element)
            {
                element.Explode(other.contacts[0].point);
            }

            if (bulletOwner.photonView.isMine)
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
            smoketrail.transform.SetParent(null);
            explosion.transform.SetParent(null);

            firetrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            smoketrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            explosion.Play();

            Destroy(smoketrail.gameObject, 10);
            Destroy(explosion.gameObject, 3);
            Destroy(gameObject);
        }
    }
}