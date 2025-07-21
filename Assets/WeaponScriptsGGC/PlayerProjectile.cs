using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;
    public float hitRadius = 0.2f;//No lo estamos usando por ahora.

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IEnemy enemy = other.GetComponent<IEnemy>();
            if (enemy != null && enemy.IsAlive())
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
