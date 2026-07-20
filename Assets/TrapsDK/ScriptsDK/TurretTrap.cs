
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

    [Header("Rotation")]
    public Transform rotatingPart;   // Assign the turret head here
    public float rotationSpeed = 180f; // Degrees per second

    protected override void Update()
    {
        base.Update();

        RotateTowardsTarget();

        if (currentTarget != null && currentTarget.IsAlive())
        {
            if (Time.time - lastShot >= attackCooldown)
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

    private void RotateTowardsTarget()
    {
        if (rotatingPart == null)
            return;

        if (currentTarget == null || !currentTarget.IsAlive())
            return;

        Vector3 direction =
            currentTarget.GetTransform().position - rotatingPart.position;

        // Ignore vertical difference if your game is played on a flat plane.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rotatingPart.rotation = Quaternion.RotateTowards(
            rotatingPart.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}
