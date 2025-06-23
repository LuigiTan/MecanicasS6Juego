using UnityEngine;

public class Mine : MonoBehaviour
{
    public float explosionForce = 5f;  // Fuerza de la explosión hacia atrás
    public float explosionUpwardForce = 1f; // Pequeña fuerza hacia arriba
    public float explosionRadius = 5f; // Radio de la explosión

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Calcular dirección desde la mina hacia el jugador
                Vector3 explosionDirection = (other.transform.position - transform.position).normalized;

                // Eliminar componente Y para evitar que se lance demasiado alto
                explosionDirection.y = 0;

                // Aplicar fuerza en la dirección contraria con un leve empuje hacia arriba
                Vector3 finalForce = explosionDirection * explosionForce + Vector3.up * explosionUpwardForce;

                // Aplicar la fuerza con VelocityChange para mayor control
                playerRb.AddForce(finalForce, ForceMode.VelocityChange);
                //playerRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.15f, ForceMode.VelocityChange);

            }

            // Destruir la mina después de explotar
            Destroy(gameObject);
        }
    }
}
