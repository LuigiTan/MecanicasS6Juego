using System;
using UnityEngine;

public class WavePhaseManager : MonoBehaviour
{
    public static WavePhaseManager Instance;

    public bool IsBuildPhase { get; private set; } = true;

    public event Action OnBuildPhaseStarted;
    public event Action OnCombatPhaseStarted;

    private void Awake()
    {
        Instance = this;
    }

    public void StartBuildPhase()
    {
        IsBuildPhase = true;
        Debug.Log("BUILD PHASE");

        OnBuildPhaseStarted?.Invoke();
    }

    public void StartCombatPhase()
    {
        IsBuildPhase = false;
        Debug.Log("COMBAT PHASE");

        OnCombatPhaseStarted?.Invoke();
    }
}