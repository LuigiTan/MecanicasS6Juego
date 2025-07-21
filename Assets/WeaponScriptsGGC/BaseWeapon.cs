using UnityEngine;

public enum WeaponType
{
    Melee,
    Ranged,
    AoE
}

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Base Stats")]
    public float damage = 10f;
    public float attackSpeed = 1f; // Ataques por segundo
    public WeaponType weaponType;

    [Header("Projectiles (solo para Ranged/AoE)")]
    public GameObject projectilePrefab;

    protected float lastAttackTime = -999f; // Para manejar cooldown

    // Método general que valida si puede atacar
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + (1f / attackSpeed);
    }

    // Se llama desde PlayerCombat cuando se intenta atacar
    public void TryAttack()
    {
        if (CanAttack())
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    // Implementar en clases hijas
    protected abstract void Attack();
}
