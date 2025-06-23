
using UnityEngine;

public interface IEnemy
{
    void TakeDamage(float amount);
    void ApplyStun(float seconds);
    bool IsAlive();
    Transform GetTransform();
}
