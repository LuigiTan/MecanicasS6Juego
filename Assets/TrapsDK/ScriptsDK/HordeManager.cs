using UnityEngine;
using System;
using TMPro;

public class HordeManager : MonoBehaviour
{
    public static HordeManager Instance { get; private set; }

    public int HordeCount { get; private set; } = 0;

    public event Action OnHordeTriggered;

    public event Action<int> OnWaveStarted;

    private float nextHordeTime;
    public float hordeInterval = 30f;

    public TextMeshProUGUI waveCount;

    private bool combatActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        nextHordeTime = Time.time + hordeInterval;

        WavePhaseManager.Instance.OnCombatPhaseStarted += () =>
        {
            combatActive = true;
            nextHordeTime = Time.time + hordeInterval;
        };

        WavePhaseManager.Instance.OnBuildPhaseStarted += () =>
        {
            combatActive = false;
        };
    }

    void Update()
{
    if (!combatActive)
        return;

    if (Time.time >= nextHordeTime)
    {
        TriggerHorde();
        combatActive = false;
    }
}

    private void TriggerHorde()
    {
        HordeCount++;
        waveCount.text = "Wave: " + HordeCount;
        OnWaveStarted?.Invoke(HordeCount);
        OnHordeTriggered?.Invoke();
    }

    public void SetHordeCount(int count)
    {
        HordeCount = count;
        waveCount.text = "Wave: " + HordeCount;
    }

}
