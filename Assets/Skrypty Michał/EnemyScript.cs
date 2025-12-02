using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public Transform player; 
    public float detectionRadius = 10f; 
    public float killDistance = 1f; 

    private NavMeshAgent agent;
    private bool isPlayerInRange = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (player == null)
        {
            Debug.LogError("Player not assigned and not found in the scene. Make sure the player has the 'Player' tag.");
        }

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

        if (distanceToPlayer <= detectionRadius)
        {
            isPlayerInRange = true;
            agent.SetDestination(player.position); 
        }
        else
        {
            isPlayerInRange = false;
            agent.SetDestination(transform.position); 
        }

        if (isPlayerInRange && distanceToPlayer <= killDistance)
        {
            KillPlayer();
        }
    }

    void KillPlayer()
    {
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        if (playerScript != null)
        {
            playerScript.Respawn(); 
            Debug.Log("Player killed by enemy and respawned.");
        }
    }
}
