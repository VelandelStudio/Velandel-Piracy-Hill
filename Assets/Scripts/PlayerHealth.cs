using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace VelandelPiracyHill
{
    /// <summary>
    /// Heal Behavior of a player
    /// When it reaches 0, the player Die
    /// </summary>
    public class PlayerHealth : Photon.PunBehaviour
    {
        public Color fullHealth = Color.green;
        public Color zeroHealth = Color.red;
        public Slider healthBar;
        public Image fillBar;

        const int MAX_HP = 100;

        PlayerShip player;

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

        private void Awake()
        {
            player = GetComponent<PlayerShip>();
        }

        void DisplayHealth()
        {
            healthBar.value = hitPoints;
            fillBar.color = Color.Lerp(zeroHealth, fullHealth, hitPoints / MAX_HP);

            /*
            if (photonView.isMine)
            {
                GameUI.SetHealth(GetNormalisedHealthPercent(hitPoints);
            }
            */
        }

        public void DoDamages(Bullet bullet)
        {
            hitPoints = Mathf.Clamp(hitPoints - bullet.damage, 0, MAX_HP);

            /*
            if (hitPoints == 0)
            {
                StartCoroutine(ExplodeTank());
            }
            */
        }
    }
}