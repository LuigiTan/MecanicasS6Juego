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

        // Obtener dirección hacia donde mira la cámara
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint = ray.GetPoint(100f); // Para ajustar la distancia del ray 
        Vector3 direction = (targetPoint - shootPoint.position).normalized;

        // Instanciar proyectil con esa dirección
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 launchForce = direction * projectileForce + shootPoint.up * upwardForce;
            rb.AddForce(launchForce, ForceMode.Impulse);
        }
        Debug.DrawRay(shootPoint.position, direction * 3f, Color.yellow, 2f);
    }
}
