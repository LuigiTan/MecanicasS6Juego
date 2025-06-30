
using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public abstract class TrapBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float health = 100f;
    public float attackSpeed = 1f;
    public float activationRadius = 5f;
    public int cost = 100;
    public int level = 1;
    public int maxLevel = 3;

    [Header("Upgrade Multipliers")]
    public float damageMultiplier = 1.5f;
    public float attackSpeedMultiplier = 0.85f;
    public float radiusMultiplier = 1.1f;

    [Header("Runtime")]
    public GameObject rangeIndicatorPrefab;

    protected SphereCollider sphereCollider;
    protected bool playerNearby = false;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI upgradeLevelText;

    protected virtual void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = activationRadius;

        upgradeText.text = "";
        upgradeLevelText.text = "";

        if (rangeIndicatorPrefab)
        {
            GameObject rangeObj = Instantiate(rangeIndicatorPrefab, transform);
            rangeObj.transform.localScale = Vector3.one * activationRadius * 2f;
        }
    }

    protected virtual void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryUpgrade();
        }
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void TryUpgrade()
    {
        if (level >= maxLevel) return;

        int upgradeCost = PlayerStats.Instance.GetUpgradeCost(this);
        if (PlayerStats.Instance.SpendMoney(upgradeCost))
        {
            level++;
            UpgradeStats();
            upgradeText.text = "Upgrade Cost: $" + cost;
            upgradeLevelText.text = "Trap Level: $" + level + "\nHealth: " + health + "\nAttack Speed: " + attackSpeed + "\nActivation Radius: " + activationRadius;
        }
    }

    protected virtual void UpgradeStats()
    {
        attackSpeed *= attackSpeedMultiplier;
        activationRadius *= radiusMultiplier;
        sphereCollider.radius = activationRadius;
    }

    protected abstract void OnEnemyEnter(Collider other);
    protected abstract void OnEnemyExit(Collider other);
    protected abstract void PerformAttack();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            OnEnemyEnter(other);
        else if (other.CompareTag("Player"))
        {
            playerNearby = true;
            upgradeText.text = "Upgrade Cost: $" + cost;
            upgradeLevelText.text = "Trap Level: $" + level + "\nHealth: " + health + "\nAttack Speed: " + attackSpeed + "\nActivation Radius: " + activationRadius;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            OnEnemyExit(other);
        else if (other.CompareTag("Player"))
        {
            playerNearby = false;
            upgradeText.text = "";
            upgradeLevelText.text = "";
        }
    }
}
