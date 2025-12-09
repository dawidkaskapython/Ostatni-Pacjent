using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 10f;
    public float killDistance = 2f;

    private NavMeshAgent agent;
    private Vector3 playerStartPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Znajd� gracza po tagu
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                Debug.LogError("Nie znaleziono obiektu z tagiem PLAYER!");
                return;
            }
        }

        // Zapami�taj startow� pozycj� gracza (respawn)
        playerStartPos = player.position;

        // Wy��cz kolizj� mi�dzy wrogiem a graczem (wa�ne dla NavMeshAgent)
        Collider playerCollider = player.GetComponent<Collider>();
        Collider enemyCollider = GetComponent<Collider>();

        if (playerCollider != null && enemyCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, enemyCollider, true);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // �LEDZENIE
        if (distanceToPlayer <= detectionRadius)
            agent.SetDestination(player.position);
        else
            agent.SetDestination(transform.position);

        // ZABICIE
        if (distanceToPlayer <= killDistance)
            KillPlayer();
    }

    void KillPlayer()
    {
        // teleport gracza (bo u�ywasz Rigidbody + FirstPersonMovement)
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        player.position = playerStartPos;

        Debug.Log("PLAYER DEAD ? respawn.");
    }
}
