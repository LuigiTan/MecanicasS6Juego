using UnityEngine;

public class RangedWeapon : BaseWeapon
{
    [Header("Ranged Settings")]
    public Transform shootPoint;         // Lugar desde donde se dispara
    public float projectileSpeed = 15f;
    public float maxDistance = 50f;

    protected override void Attack()
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogWarning("Faltan referencias en RangedWeapon");
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint = ray.GetPoint(100f); // punto lejano

        Vector3 direction = (targetPoint - shootPoint.position).normalized;

        // Instancia el proyectil
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));

        // Se asegura que el proyectil tenga Rigidbody
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }

        Debug.DrawRay(shootPoint.position, direction * 3f, Color.green, 2f);
    }
}
