
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

    [Header("Upgrade Visuals")]
    public float scalePerLevel = 0.15f;

    public Color level1Color = Color.white;
    public Color level2Color = Color.yellow;
    public Color level3Color = Color.red;

    private Vector3 originalScale;

    protected virtual void Start()
    {
        originalScale = transform.localScale;
        ApplyUpgradeVisuals();

        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = activationRadius;

        if (upgradeText != null) upgradeText.text = "";
        if (upgradeLevelText != null) upgradeLevelText.text = "";

        if (rangeIndicatorPrefab)
        {
            rangeIndicatorInstance = Instantiate(rangeIndicatorPrefab, transform);
            rangeIndicatorInstance.transform.localPosition = Vector3.zero;
            rangeIndicatorInstance.transform.localRotation = Quaternion.identity;
            UpdateRangeIndicatorScale();

            // Scale correctly to match the activationRadius
            float diameter = activationRadius * 2f;

            // Normalize to prefab�s original size
            Renderer renderer = rangeIndicatorInstance.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                float originalSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.z);
                float scaleFactor = diameter / originalSize;
                rangeIndicatorInstance.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
            }
            else
            {
                // Fallback if no renderer, scale uniformly
                rangeIndicatorInstance.transform.localScale = Vector3.one * diameter;
            }
        }

        if (WavePhaseManager.Instance != null)
        {
            WavePhaseManager.Instance.OnBuildPhaseStarted += ShowRangeIndicator;
            WavePhaseManager.Instance.OnCombatPhaseStarted += HideRangeIndicator;

            // Match the current game phase immediately.
            if (WavePhaseManager.Instance.IsBuildPhase)
                ShowRangeIndicator();
            else
                HideRangeIndicator();
        }
        else
        {
            // Fallback if no manager exists.
            ShowRangeIndicator();
        }

        ShowRangeIndicator();
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

        ApplyUpgradeVisuals();
    }

    private void UpdateRangeIndicatorScale()
    {
        if (rangeIndicatorInstance == null)
            return;

        float diameter = activationRadius * 2f;

        rangeIndicatorInstance.transform.localScale = new Vector3(diameter, 1f, diameter);
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

    private void ShowRangeIndicator()
    {
        if (rangeIndicatorInstance == null)
            return;

        UpdateRangeIndicatorScale();
        rangeIndicatorInstance.SetActive(true);
    }

    private void HideRangeIndicator()
    {
        if (rangeIndicatorInstance != null)
        {
            rangeIndicatorInstance.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        if (WavePhaseManager.Instance != null)
        {
            WavePhaseManager.Instance.OnBuildPhaseStarted -= ShowRangeIndicator;
            WavePhaseManager.Instance.OnCombatPhaseStarted -= HideRangeIndicator;
        }
    }

    private void ApplyUpgradeVisuals()
    {
        // Scale
        float scaleMultiplier = 1f + ((level - 1) * scalePerLevel);
        transform.localScale = originalScale * scaleMultiplier;

        // Color
        Color targetColor = level1Color;

        switch (level)
        {
            case 2:
                targetColor = level2Color;
                break;

            case 3:
                targetColor = level3Color;
                break;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            // Skip the range indicator
            if (rangeIndicatorInstance != null &&
                r.transform.IsChildOf(rangeIndicatorInstance.transform))
                continue;

            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = targetColor;
            }
        }
    }
}
