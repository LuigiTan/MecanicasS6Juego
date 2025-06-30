
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Vector3 spawnPoint;
    public TextMeshProUGUI healthBar;

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
            Respawn();
        }
    }

    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
