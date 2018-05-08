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
        public HealthBar healthBar;

        PlayerShip _player;
        PlayerShip player
        {
            get
            {
                if (_player == null) _player = GetComponent<PlayerShip>();
                return _player;
            }
        }


        const int MAX_HP = 100;
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

        void DisplayHealth()
        {
            healthBar.SetHealthBarValue(GetNormalisedHealthPercent(hitPoints));
        }

        public void DoDamages(Bullet bullet)
        {
            hitPoints = Mathf.Clamp(hitPoints - bullet.damage, 0, MAX_HP);
        }

        float GetNormalisedHealthPercent(int hp)
        {
            return hp / (float)MAX_HP;
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
    }
}