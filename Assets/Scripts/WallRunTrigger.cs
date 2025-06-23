using UnityEngine;

public class WallRunTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.SetWallRunBuffer(transform.right); // Store wall run data for a short buffer window
        }
    }
}