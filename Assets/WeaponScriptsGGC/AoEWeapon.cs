using UnityEngine;

public class AoEWeapon : BaseWeapon
{
    [Header("AoE Settings")]
    public Transform shootPoint;
    public float projectileForce = 10f;
    public float upwardForce = 2f;

    protected override void Attack()
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogWarning("Faltan referencias en AoEWeapon");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force = shootPoint.forward * projectileForce + shootPoint.up * upwardForce;
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}
