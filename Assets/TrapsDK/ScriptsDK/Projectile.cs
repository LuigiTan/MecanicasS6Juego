
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private IEnemy target;
    private float damage;
    private float speed;

    public void Initialize(IEnemy t, float dmg, float spd)
    {
        target = t;
        damage = dmg;
        speed = spd;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (target == null || !target.IsAlive())
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.GetTransform().position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.GetTransform().position) < 0.2f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
