using UnityEngine;
using System.Collections;
using emotitron.Network.NST.Sample;
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
            Collider[] hitColliders = Physics.OverlapSphere(contactPoint, 2f, layerMask, QueryTriggerInteraction.Ignore);

            StartCoroutine(CoroutineExploder(hitColliders, contactPoint));

            exploder.transform.position = contactPoint;
            exploder.transform.position += new Vector3(0f, 0.25f, 0f);
            exploder.Explode(new Vector3(0f, 7f, 0f),transform.lossyScale.x * 4);
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
                    //rb.AddExplosionForce(10f, contactPoint, 1f, 0.2f);
                }
            }
            yield return null;
        }
    }
}


