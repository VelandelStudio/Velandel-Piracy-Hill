using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Heal Behavior of a player
/// When it reaches 0, the player Die
/// </summary>
public class Health : NetworkBehaviour{

    public const int maxHealth = 100;

    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;
    public RectTransform healthBar;

    /// <summary>
    /// Methode TakeDamage remove heal from player if it touched by Bullet
    /// At zero, player die
    /// </summary>
    /// <param name="amount">Damage Number</param>
    public void TakeDamage(int amount)
    {
        if (!isServer)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Dead !");
        }
    }

    /// <summary>
    /// OnChangeHealth methode is sync with the healthbar of the player.
    /// Dependind on player's health, the server sync with all client the current health of the player
    /// For all other clients.
    /// </summary>
    /// <param name="currentHealth">Current life of a player</param>
    void OnChangeHealth(int currentHealth)
    {
        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
    }
}
