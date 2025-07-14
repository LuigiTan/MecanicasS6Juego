
using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public abstract class TrapBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float health = 100f;
    public float attackCooldown = 1f;
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
    private GameObject rangeIndicatorInstance;

    protected SphereCollider sphereCollider;
    protected bool playerNearby = false;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI upgradeLevelText;

    private PlayerConstruction constructionManager;

    protected virtual void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = activationRadius;

        if (upgradeText != null) upgradeText.text = "";
        if (upgradeLevelText != null) upgradeLevelText.text = "";

        if (rangeIndicatorPrefab)
        {
            GameObject rangeObj = Instantiate(rangeIndicatorPrefab, transform);
            rangeIndicatorInstance = Instantiate(rangeIndicatorPrefab, transform);
            rangeIndicatorInstance.transform.localPosition = Vector3.zero; // center on trap
            UpdateRangeIndicatorScale(); // Apply scale based on current activationRadius

            // Scale correctly to match the activationRadius
            float diameter = activationRadius * 2f;

            // Normalize to prefab’s original size
            Renderer renderer = rangeObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                float originalSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.z);
                float scaleFactor = diameter / originalSize;
                rangeObj.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
            }
            else
            {
                // Fallback if no renderer, scale uniformly
                rangeObj.transform.localScale = Vector3.one * diameter;
            }
        }

        constructionManager = FindObjectOfType<PlayerConstruction>();
    }

    protected virtual void Update()
    {
        if (playerNearby)
        {
            // Live update UI as long as player is near
            if (upgradeText != null)
            {
                int cost = PlayerStats.Instance.GetUpgradeCost(this);
                upgradeText.text = level >= maxLevel
                ? "MAX LEVEL"
                : "Upgrade Cost: $" + cost;
            }

            if (upgradeLevelText != null)
            {
                upgradeLevelText.text =
                    "Trap Level: " + level +
                    "\nHealth: " + health +
                    "\nAttack Cooldown: " + attackCooldown +
                    "\nActivation Radius: " + activationRadius;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryUpgrade();
            }
        }
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            constructionManager?.OnTrapDestroyed(this);
            Destroy(gameObject);
        }
    }

    protected virtual void TryUpgrade()
    {
        if (level >= maxLevel)
        {
            Debug.Log("Trap already at max level.");
            return;
        }

        int upgradeCost = PlayerStats.Instance.GetUpgradeCost(this);
        if (PlayerStats.Instance.SpendMoney(upgradeCost))
        {
            level++;
            UpgradeStats();
        }
        else
        {
            Debug.Log("Not enough money to upgrade.");
        }
    }

    protected virtual void UpgradeStats()
    {
        attackCooldown *= attackSpeedMultiplier;
        activationRadius *= radiusMultiplier;
        sphereCollider.radius = activationRadius;
        UpdateRangeIndicatorScale();
    }

    private void UpdateRangeIndicatorScale()
    {
        if (rangeIndicatorInstance == null) return;
        float diameter = activationRadius * 2f;

        // Scale uniformly assuming prefab represents diameter = 1 unit
        rangeIndicatorInstance.transform.localScale = Vector3.one * diameter;
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            OnEnemyExit(other);
        else if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (upgradeText != null) upgradeText.text = "";
            if (upgradeLevelText != null) upgradeLevelText.text = "";
        }
    }
}
