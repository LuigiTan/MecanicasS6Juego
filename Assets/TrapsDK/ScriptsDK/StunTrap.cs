using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StunTrap : TrapBase
{
    [Header("Stun Trap Settings")]
    public float stunDuration = 1.5f;
    public float syncInterval = 0.5f;

    private HashSet<IEnemy> enemies = new HashSet<IEnemy>();
    private float lastTick;
    private float lastSync;

    protected override void Update()
    {
        base.Update();

        // Periodically rescan nearby enemies to ensure none are missed
        if (Time.time - lastSync >= syncInterval)
        {
            SyncEnemiesInRange();
            lastSync = Time.time;
        }

        if (enemies.Count > 0 && Time.time - lastTick >= attackCooldown)
        {
            PerformAttack();
            lastTick = Time.time;
        }
    }

    protected override void PerformAttack()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive())
            {
                enemy.ApplyStun(stunDuration);
            }
        }
    }

    protected override void OnEnemyEnter(Collider other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null)
        {
            enemies.Add(enemy);
        }
    }

    protected override void OnEnemyExit(Collider other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null)
        {
            StartCoroutine(RemoveAfterDelay(enemy));
        }
    }

    private IEnumerator RemoveAfterDelay(IEnemy enemy)
    {
        yield return new WaitForSeconds(2f);
        enemies.Remove(enemy);
    }

    private void SyncEnemiesInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, activationRadius);
        foreach (var hit in hits)
        {
            IEnemy enemy = hit.GetComponent<IEnemy>();
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }
    }

    protected override void UpgradeStats()
    {
        base.UpgradeStats();
        stunDuration += 0.5f;
    }
}
