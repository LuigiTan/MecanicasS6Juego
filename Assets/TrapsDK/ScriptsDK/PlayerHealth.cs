
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Vector3 spawnPoint;
    public TextMeshProUGUI healthBar;

    public GameOver gameOver;

    void Start()
    {
        spawnPoint = transform.position;
        currentHealth = maxHealth;
        healthBar.text = "Health: " + currentHealth;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        healthBar.text = "Health: " + currentHealth;
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        gameOver?.ShowGameOver("You Died");
    }
}
