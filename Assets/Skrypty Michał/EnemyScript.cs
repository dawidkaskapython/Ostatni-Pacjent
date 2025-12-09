using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 10f;
    public float killDistance = 2f;

    private NavMeshAgent agent;
    private Vector3 enemyStartPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyStartPos = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                Debug.LogError("Nie znaleziono gracza z tagiem 'Player'.");
        }

        Collider pc = player.GetComponent<Collider>();
        Collider ec = GetComponent<Collider>();
        if (pc != null && ec != null)
            Physics.IgnoreCollision(pc, ec, true);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
            agent.SetDestination(player.position);
        else
            agent.SetDestination(transform.position);

        if (distanceToPlayer <= killDistance)
            KillPlayer();
    }

    void KillPlayer()
    {
        PlayerEvents.OnPlayerDeath?.Invoke();
    }

    void ResetEnemyPosition()
    {
        if (agent != null)
            agent.ResetPath();

        transform.position = enemyStartPos;
    }
}
