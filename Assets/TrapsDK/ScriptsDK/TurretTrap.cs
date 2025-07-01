
using UnityEngine;
using System.Collections.Generic;

public class TurretTrap : TrapBase
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float projectileDamage = 10f;

    private Queue<IEnemy> queue = new Queue<IEnemy>();
    private IEnemy currentTarget;
    private float lastShot;

    protected override void Update()
    {
        base.Update();

        if (currentTarget != null && currentTarget.IsAlive())
        {
            if (Time.time - lastShot >= attackSpeed)
            {
                PerformAttack();
                lastShot = Time.time;
            }
        }
        else
        {
            GetNextTarget();
        }
    }

    protected override void PerformAttack()
    {
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        go.GetComponent<Projectile>().Initialize(currentTarget, projectileDamage, projectileSpeed);
    }

    protected override void OnEnemyEnter(Collider other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null && !queue.Contains(enemy))
            queue.Enqueue(enemy);

        if (currentTarget == null)
            GetNextTarget();
    }

    protected override void OnEnemyExit(Collider other) { }

    private void GetNextTarget()
    {
        while (queue.Count > 0)
        {
            IEnemy e = queue.Dequeue();
            if (e != null && e.IsAlive())
            {
                currentTarget = e;
                return;
            }
        }

        currentTarget = null;
    }

    protected override void UpgradeStats()
    {
        base.UpgradeStats();
        projectileDamage *= damageMultiplier;
        projectileSpeed *= 1.1f;
    }
}
