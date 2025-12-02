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
    }

    void Update()
    {
        if (isPlayerInRange)
        {
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) <= killDistance)
            {
                KillPlayer();
            }
        }
        else
        {
            agent.SetDestination(transform.position); 
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    void KillPlayer()
    {
        Debug.Log("PlayerKilled");

player.gameObject.SetActive(false); 
    }
}
