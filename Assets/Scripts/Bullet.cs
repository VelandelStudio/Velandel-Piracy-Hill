using UnityEngine;

namespace VelandelPiracyHill
{
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

        private void FixedUpdate()
        {
            transform.rotation = Quaternion.LookRotation(rbody.velocity);
            if (transform.position.y < -50)
            {
                Destroy(gameObject);
            }
        }

        public void SetOwner(PhotonView ownerView)
        {
            bulletOwner = ownerView.GetComponent<PlayerShooter>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag != "Player")
            {
                DestroyBullet();
            }
        }

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