using UnityEngine;

public class AoEProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float explosionRadius = 3f;
    public float lifeTime = 5f;
    public LayerMask enemyMask;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyMask);

        foreach (Collider col in hits)
        {
            IEnemy enemy = col.GetComponent<IEnemy>();
            if (enemy != null && enemy.IsAlive())
            {
                enemy.TakeDamage(damage);
            }
        }

        Debug.Log("Explosión AoE: enemigos afectados = " + hits.Length);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
