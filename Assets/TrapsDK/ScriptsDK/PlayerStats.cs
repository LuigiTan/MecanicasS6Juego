
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int money = 200;

    void Awake()
    {
        Instance = this;
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public int GetUpgradeCost(TrapBase trap)
    {
        return trap.cost + trap.level * 50;
    }
}
