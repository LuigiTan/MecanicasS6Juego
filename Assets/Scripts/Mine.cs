using UnityEngine;

public class Mine : MonoBehaviour
{
    public float explosionForce = 5f;  // Fuerza de la explosi�n hacia atr�s
    public float explosionUpwardForce = 1f; // Peque�a fuerza hacia arriba
    public float explosionRadius = 5f; // Radio de la explosi�n

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Calcular direcci�n desde la mina hacia el jugador
                Vector3 explosionDirection = (other.transform.position - transform.position).normalized;

                // Eliminar componente Y para evitar que se lance demasiado alto
                explosionDirection.y = 0;

                // Aplicar fuerza en la direcci�n contraria con un leve empuje hacia arriba
                Vector3 finalForce = explosionDirection * explosionForce + Vector3.up * explosionUpwardForce;

                // Aplicar la fuerza con VelocityChange para mayor control
                playerRb.AddForce(finalForce, ForceMode.VelocityChange);
                //playerRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.15f, ForceMode.VelocityChange);

            }

            // Destruir la mina despu�s de explotar
            Destroy(gameObject);
        }
    }
}
