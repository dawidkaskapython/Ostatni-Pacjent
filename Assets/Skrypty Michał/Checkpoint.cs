using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 lastCheckpointPosition; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastCheckpointPosition = transform.position;
            Debug.Log("Checkpoint done");
        }
    }
}
