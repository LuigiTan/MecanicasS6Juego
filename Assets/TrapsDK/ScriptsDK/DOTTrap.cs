
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DOTTrap : TrapBase
{
    public float damagePerTick = 5f;

    private HashSet<IEnemy> enemies = new HashSet<IEnemy>();
    private float lastTick;

    protected override void Update()
    {
        base.Update();
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
                enemy.TakeDamage(damagePerTick);
        }
    }

    protected override void OnEnemyEnter(Collider other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null) enemies.Add(enemy);
    }

    protected override void OnEnemyExit(Collider other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null) StartCoroutine(RemoveAfterDelay(enemy));
    }

    private IEnumerator RemoveAfterDelay(IEnemy enemy)
    {
        yield return new WaitForSeconds(2f);
        enemies.Remove(enemy);
    }

    protected override void UpgradeStats()
    {
        base.UpgradeStats();
        damagePerTick *= damageMultiplier;
    }
}
