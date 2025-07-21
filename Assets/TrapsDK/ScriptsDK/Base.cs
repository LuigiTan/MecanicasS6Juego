using System.Collections;
using TMPro;
using UnityEngine;

public class Base : MonoBehaviour
{
    public float health = 500f;
    public float damageCooldown = 1f;
    public TextMeshProUGUI baseHealth;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        baseHealth.text = "Base Health: " + health;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        baseHealth.text = "Base Health: " + health;
        Debug.Log("Goal damaged: -" + amount + " (Remaining: " + health + ")");
        if (health <= 0)
        {
            Debug.Log("Goal destroyed! Game Over.");
            StartCoroutine(DelayedRespawn(1.5f));
        }
    }
    private IEnumerator DelayedRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerHealth?.Respawn();
    }
}
