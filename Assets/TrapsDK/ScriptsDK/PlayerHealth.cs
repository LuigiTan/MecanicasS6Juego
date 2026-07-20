
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Vector3 spawnPoint;
    public TextMeshProUGUI healthBar;

    public GameOver gameOver;

    public GameObject damageCube;
    public float dmgTimer = 0.5f;

    void Start()
    {
        spawnPoint = transform.position;
        currentHealth = maxHealth;
        healthBar.text = "Health: " + currentHealth;
        damageCube.SetActive(false);
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        healthBar.text = "Health: " + currentHealth;
        damageCube.SetActive(true);

        StartCoroutine(DamageTimer(dmgTimer));

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        gameOver?.ShowGameOver("You Died");
    }
    private IEnumerator DamageTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        damageCube.SetActive(false);
    }
}
