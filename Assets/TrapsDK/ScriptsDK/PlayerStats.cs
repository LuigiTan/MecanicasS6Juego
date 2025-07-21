
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public TextMeshProUGUI moneyText;

    public int money = 200;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        moneyText.text = "Money: $" + money;
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            moneyText.text = "Money: $" + money;
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        moneyText.text = "Money: $" + money;
    }

    public int GetUpgradeCost(TrapBase trap)
    {
        moneyText.text = "Money: $" + money;
        return trap.cost + trap.level * 50;
    }
}
