using System.Collections;
using TMPro;
using UnityEngine;

public class Base : MonoBehaviour
{
    public float health = 500f;
    public float damageCooldown = 1f;
    public TextMeshProUGUI baseHealth;
    private PlayerHealth playerHealth;
    
    public TextMeshProUGUI BaseDamagedText;

    public GameOver gameOverManager;

    public GameObject damageCube;

    public float dmgTimer = 0.5f;

    private void Start()
    {
        playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        baseHealth.text = "Base Health: " + health;
        damageCube.SetActive(false);
        BaseDamagedText.gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        baseHealth.text = "Base Health: " + health;
        Debug.Log("Goal damaged: -" + amount + " (Remaining: " + health + ")");
        BaseDamagedText.gameObject.SetActive(true);
        damageCube.SetActive(true);

        StartCoroutine(DamageTimer(dmgTimer));

        if (health <= 0)
        {
            Debug.Log("Goal destroyed! Game Over.");
            StartCoroutine(DelayedRespawn(1.5f));
        }
    }

    private IEnumerator DelayedRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameOverManager?.ShowGameOver("The base Died");
    }

    private IEnumerator DamageTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        damageCube.SetActive(false);
        BaseDamagedText.gameObject.SetActive(false);
    }
}
