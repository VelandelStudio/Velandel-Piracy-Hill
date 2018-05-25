using System.Collections;
using System.Collections.Generic;
using PicaVoxel;
using UnityEngine;

namespace VelandelPiracyHill
{
    /// <summary>
    /// Heal Behavior of a player
    /// When it reaches 0, the player Die
    /// </summary>
    public class PlayerHealth : Photon.PunBehaviour
    {
        [SerializeField] private Exploder exploder;
        public List<Transform> explodePointsOnDeath;
        public HealthBar healthBar;
        private Volume[] volumes;
        PlayerShip _player;
        PlayerShip player
        {
            get
            {
                if (_player == null) _player = GetComponent<PlayerShip>();
                return _player;
            }
        }


        const int MAX_HP = 0;
        int hitPoints
        {
            get
            {
                object hp;

                if (photonView.owner.CustomProperties.TryGetValue("HP", out hp))
                {
                    return (int)hp;
                }
                else
                {
                    return MAX_HP;
                }
            }

            set
            {
                player.SetCustomProperty("HP", value);
            }
        }

        /// <summary>
        /// OnPhotonInstantiate method
        /// Trigger the instantiation of the playerHealth gameObj
        /// Setting and display its HP
        /// </summary>
        /// <param name="info"></param>
        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (photonView.isMine)
            {
                hitPoints = MAX_HP;
            }
            else
            {
                DisplayHealth();
            }
        }

        /// <summary>
        /// DisplayHealth method
        /// called to adjust the value of the current health 
        /// </summary>
        void DisplayHealth()
        {
            healthBar.SetHealthBarValue(GetNormalisedHealthPercent(hitPoints));
        }

        /// <summary>
        /// DoDamages method
        /// Called by the obj that touch the player and apply damages 
        /// </summary>
        /// <param name="bullet">Weapon Bullet</param>
        public void DoDamages(Bullet bullet)
        {
            hitPoints = Mathf.Clamp(hitPoints - bullet.damage, 0, MAX_HP);
            if (hitPoints == 0)
            {
                photonView.RPC("RPC_ExplodeShip", PhotonTargets.All);
            }
        }

        /// <summary>
        /// GetNormalisedHealthPercent method
        /// Pass the hp from number to purcent
        /// </summary>
        /// <param name="hp">CurrentHP</param>
        /// <returns>Hp as purcent</returns>
        float GetNormalisedHealthPercent(int hp)
        {
            return hp / (float)MAX_HP;
        }

        /// <summary>
        /// OnPhotonPlayerPropertiesChanged method
        /// This is to Sync a value over network
        /// </summary>
        /// <param name="playerAndUpdatedProps"></param>
        public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            var plyr = (PhotonPlayer)playerAndUpdatedProps[0];

            if (plyr.ID == photonView.ownerId)
            {
                var props = (ExitGames.Client.Photon.Hashtable)playerAndUpdatedProps[1];

                if (props.ContainsKey("HP"))
                {
                    DisplayHealth();
                }
            }
        }

        [PunRPC]
        void RPC_ExplodeShip()
        {
            for (int i = 0; i < explodePointsOnDeath.Count; i++)
            {
                ParticleSystem explosion = explodePointsOnDeath[i].GetComponentInChildren<ParticleSystem>();
                StartCoroutine(waitOtherBoom(explosion, explodePointsOnDeath[i].transform, i / 5.00f));
            }
        }

        IEnumerator waitOtherBoom(ParticleSystem particleSystem, Transform tr, float timer)
        {
            yield return new WaitForSeconds(timer);
            particleSystem.Play();

            for (int i = 0; i < volumes.Length; i++)
            {
                var batch = volumes[i].Explode(tr.position, 0.6f, 0, Exploder.ExplodeValueFilterOperation.GreaterThanOrEqualTo);
                if (batch.Voxels.Count > 0 && VoxelParticleSystem.Instance != null)
                {
                    // Adjust these values to change the speed of the exploding particles
                    var minExplodeSpeed = 20f;
                    var maxExplodeSpeed = 50f;
                    VoxelParticleSystem.Instance.SpawnBatch(batch, pos => (pos - tr.position).normalized * Random.Range(minExplodeSpeed, maxExplodeSpeed), gameObject.transform.lossyScale.x);
                }
            }
        }

        private void Start()
        {
            volumes = GetComponentsInChildren<Volume>();
            if (hitPoints == 0)
            {
                photonView.RPC("RPC_ExplodeShip", PhotonTargets.All);
            }
        }
    }
}