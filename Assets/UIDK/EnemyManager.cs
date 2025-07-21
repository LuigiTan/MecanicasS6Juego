using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public List<GameObject> enemyPrefabs;

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetEnemyPrefab(string name)
    {
        return enemyPrefabs.Find(p => p.name == name);
    }
}
