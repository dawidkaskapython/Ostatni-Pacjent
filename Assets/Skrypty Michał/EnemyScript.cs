using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 15f;
    public float killDistance = 2.5f;

    private NavMeshAgent agent;
    private bool isDead = false;
    private Vector3 startPosition; // Pozycja do teleportacji

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Zapamiêtujemy gdzie sta³ wróg na pocz¹tku gry
        startPosition = transform.position;

        agent.stoppingDistance = 1.0f;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("BRAK GRACZA Z TAGIEM 'Player'!");
        }
    }

    // --- Pod³¹czamy siê do eventu œmierci ---
    void OnEnable()
    {
        PlayerEvents.OnPlayerDeath += TeleportToStart;
    }

    void OnDisable()
    {
        PlayerEvents.OnPlayerDeath -= TeleportToStart;
    }
    // ----------------------------------------

    void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Logika poœcigu (tylko gdy gracz jest blisko)
        if (distanceToPlayer <= detectionRadius)
        {
            agent.SetDestination(player.position);

            // Sprawdzenie czy z³apa³ gracza
            if (distanceToPlayer <= killDistance)
            {
                KillPlayer();
            }
        }
        else
        {
            // Opcjonalnie: Jeœli gracz po prostu uciek³ (¿yje, ale jest daleko),
            // to wróg mo¿e powoli wracaæ spacerem.
            // Jeœli chcesz, ¿eby wróg sta³ w miejscu jak zgubi gracza, wykasuj liniê poni¿ej.
            agent.SetDestination(startPosition);
        }
    }

    void KillPlayer()
    {
        Debug.Log("DOTKNIÊCIE - ŒMIERÆ GRACZA");

        // Wywo³ujemy event (to zresetuje te¿ gracza)
        PlayerEvents.OnPlayerDeath?.Invoke();

        // UWAGA: Tutaj nie musimy wywo³ywaæ teleportacji rêcznie,
        // zrobi to funkcja TeleportToStart, która "s³ucha" tego eventu.
    }

    // Ta funkcja wykonuje siê automatycznie, gdy gracz zginie
    void TeleportToStart()
    {
        if (isDead || agent == null) return;

        // 1. Resetujemy œcie¿kê (¿eby przesta³ biec)
        agent.ResetPath();

        // 2. NATYCHMIASTOWA TELEPORTACJA (WARP)
        // Warp jest konieczny dla NavMeshAgenta, zwyk³e transform.position by go zepsu³o
        agent.Warp(startPosition);

        // Opcjonalnie: Obracamy go do pocz¹tkowej rotacji (jeœli chcesz)
        // transform.rotation = Quaternion.identity; 
    }

    public void DieByFlashlight()
    {
        isDead = true;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        Debug.Log("Wróg zabity latark¹.");
        Destroy(gameObject, 0.5f);
    }
}