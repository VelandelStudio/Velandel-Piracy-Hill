using UnityEngine;
using System.Collections;
using PicaVoxel;

namespace VelandelPiracyHill
{
    /// <summary>
    /// PlayerShip class is the behavior of the PlayerShip and disabled components Camera/RB etc that are not in our PhotonView
    /// This is a PunBehviour that contains some RPC.
    /// </summary>
    public class PlayerShip : Photon.PunBehaviour
    {
        [SerializeField] private Exploder exploder;

        [HideInInspector] public Camera[] PlayerCams;
        [HideInInspector] public Rigidbody[] Rbs;

        /// <summary>
        /// Awake disables ShipControllers/Cameras/RBs thet are not owned by the PhotonView (i.e other players).
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            PlayerCams = GetComponentsInChildren<Camera>();
            Rbs = GetComponentsInChildren<Rigidbody>();

            if (!photonView.isMine)
            {
                for (int i = 0; i < PlayerCams.Length; i++)
                {
                    PlayerCams[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < Rbs.Length; i++)
                {
                    Rbs[i].isKinematic = true;
                }
            }
        }

        /// <summary>
        /// RPC_ExplodeContactPoint RPC launched when an ennemy cannonball reached us.
        /// Handles Voxels and non-Voxels prefabs explosion.
        /// </summary>
        [PunRPC]
        public void RPC_ExplodeContactPoint(float x, float y, float z)
        {
            Vector3 contactPoint = new Vector3(x, y, z);
            int layerMask = ~(1 << LayerMask.NameToLayer("Indestructible"));
            Collider[] hitColliders = Physics.OverlapSphere(contactPoint, 0.4f, layerMask, QueryTriggerInteraction.Ignore);
            
            StartCoroutine(CoroutineExploder(hitColliders, contactPoint));
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Volume vol = hitColliders[i].GetComponent<Volume>();
                if(vol)
                {
                    var batch = vol.Explode(contactPoint, 0.4f, 0, Exploder.ExplodeValueFilterOperation.GreaterThanOrEqualTo);
                    if (batch.Voxels.Count > 0 && VoxelParticleSystem.Instance != null)
                    {
                        // Adjust these values to change the speed of the exploding particles
                        var minExplodeSpeed = 20f;
                        var maxExplodeSpeed = 50f;
                        VoxelParticleSystem.Instance.SpawnBatch(batch, pos => (pos - contactPoint).normalized * Random.Range(minExplodeSpeed, maxExplodeSpeed), gameObject.transform.lossyScale.x);
                    }
                }
            }
        }

        private IEnumerator CoroutineExploder(Collider[] hitColliders, Vector3 contactPoint)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].GetComponent<Chunk>())
                {
                    continue;
                }
                else
                {
                    hitColliders[i].transform.SetParent(null);
                    Destroy(hitColliders[i]);
                    Destroy(hitColliders[i].gameObject, 10f);
                    if (hitColliders[i].GetComponent<Rigidbody>() == null)
                    {
                        hitColliders[i].gameObject.AddComponent<Rigidbody>();
                    }

                    Rigidbody rb = hitColliders[i].GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    rb.AddExplosionForce(10f, contactPoint, 1f, 0.2f);
                }
            }
            yield return null;
        }

        public void SetCustomProperty(string propName, object value)
        {
            ExitGames.Client.Photon.Hashtable prop = new ExitGames.Client.Photon.Hashtable();
            prop.Add(propName, value);
            photonView.owner.SetCustomProperties(prop);
        }
    }
}


