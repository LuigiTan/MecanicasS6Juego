using UnityEngine;

public class TrapUpgradeTrigger : MonoBehaviour
{
    private TrapBase trap;

    private void Awake()
    {
        trap = GetComponentInParent<TrapBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            trap.SetPlayerNearby(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            trap.SetPlayerNearby(false);
    }
}