using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 startPosition;
    public static Transform currentCheckpoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void OnEnable()
    {
        PlayerEvents.OnPlayerDeath += Respawn;
    }

    void OnDisable()
    {
        PlayerEvents.OnPlayerDeath -= Respawn;
    }

    public void Respawn()
    {
        Vector3 respawnPos = currentCheckpoint != null
            ? currentCheckpoint.position
            : startPosition;

        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        transform.position = respawnPos;
    }
}
