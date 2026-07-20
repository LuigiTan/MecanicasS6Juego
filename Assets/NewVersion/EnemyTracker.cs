using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    public static EnemyTracker Instance;

    private int aliveEnemies;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy()
    {
        aliveEnemies++;
    }

    public void UnregisterEnemy()
    {
        aliveEnemies--;

        if (aliveEnemies <= 0 && !WavePhaseManager.Instance.IsBuildPhase)
        {
            WavePhaseManager.Instance.StartBuildPhase();
        }
    }

    public int AliveEnemies => aliveEnemies;
}