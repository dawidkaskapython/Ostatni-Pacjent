using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn.currentCheckpoint = transform;
            Debug.Log("Checkpoint activated at: " + transform.position);
        }
    }
}
