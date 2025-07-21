using UnityEngine;
using System;

public class HordeManager : MonoBehaviour
{
    public static HordeManager Instance { get; private set; }

    public int HordeCount { get; private set; } = 0;

    public event Action OnHordeTriggered;

    private float nextHordeTime;
    public float hordeInterval = 30f;

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
    }

    void Update()
    {
        if (Time.time >= nextHordeTime)
        {
            TriggerHorde();
            nextHordeTime = Time.time + hordeInterval;
        }
    }

    private void TriggerHorde()
    {
        HordeCount++;
        Debug.Log($"[HordeManager] Triggering Horde #{HordeCount}");
        OnHordeTriggered?.Invoke();
    }
}
