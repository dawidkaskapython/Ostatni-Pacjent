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

        // Zapami�tujemy gdzie sta� wr�g na pocz�tku gry
        startPosition = transform.position;

        agent.stoppingDistance = 1.0f;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("BRAK GRACZA Z TAGIEM 'Player'!");
        }
    }

    // --- Pod��czamy si� do eventu �mierci ---
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

        // 1. Logika po�cigu (tylko gdy gracz jest blisko)
        if (distanceToPlayer <= detectionRadius)
        {
            agent.SetDestination(player.position);

            // Sprawdzenie czy z�apa� gracza
            if (distanceToPlayer <= killDistance)
            {
                KillPlayer();
            }
        }
        else
        {
            // Opcjonalnie: Je�li gracz po prostu uciek� (�yje, ale jest daleko),
            // to wr�g mo�e powoli wraca� spacerem.
            // Je�li chcesz, �eby wr�g sta� w miejscu jak zgubi gracza, wykasuj lini� poni�ej.
            agent.SetDestination(startPosition);
        }
    }

    void KillPlayer()
    {
        Debug.Log("DOTKNI�CIE - �MIER� GRACZA");

        // Wywo�ujemy event (to zresetuje te� gracza)
        PlayerEvents.OnPlayerDeath?.Invoke();

        // UWAGA: Tutaj nie musimy wywo�ywa� teleportacji r�cznie,
        // zrobi to funkcja TeleportToStart, kt�ra "s�ucha" tego eventu.
    }

    // Ta funkcja wykonuje si� automatycznie, gdy gracz zginie
    void TeleportToStart()
    {
        if (isDead || agent == null) return;

        // 1. Resetujemy �cie�k� (�eby przesta� biec)
        agent.ResetPath();

        // 2. NATYCHMIASTOWA TELEPORTACJA (WARP)
        // Warp jest konieczny dla NavMeshAgenta, zwyk�e transform.position by go zepsu�o
        agent.Warp(startPosition);

        // Opcjonalnie: Obracamy go do pocz�tkowej rotacji (je�li chcesz)
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
        Debug.Log("Wr�g zabity latark�.");
        Destroy(gameObject, 0.5f);
    }
}