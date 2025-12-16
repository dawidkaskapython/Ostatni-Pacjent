using UnityEngine;
using System.Collections; // Potrzebne do Coroutine

public class PlayerRespawn : MonoBehaviour
{
    private CharacterController charController; // Obs³uga kontrolera ruchu
    private Vector3 startPosition;
    public static Transform currentCheckpoint;

    void Start()
    {
        // Pobieramy kontroler (to on blokuje teleportacjê!)
        charController = GetComponent<CharacterController>();
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

        Debug.Log("Teleportacja gracza do: " + respawnPos);

        // Rozpoczynamy procedurê bezpiecznej teleportacji
        StartCoroutine(TeleportRoutine(respawnPos));
    }

    IEnumerator TeleportRoutine(Vector3 targetPos)
    {
        // 1. Wy³¹czamy kontroler ruchu (inaczej Unity zablokuje zmianê pozycji)
        if (charController != null) charController.enabled = false;

        // 2. Czekamy klatkê (dla pewnoœci fizyki)
        yield return new WaitForEndOfFrame();

        // 3. Przenosimy gracza
        transform.position = targetPos;

        // 4. Czekamy klatkê, ¿eby pozycja siê "zapisa³a"
        yield return new WaitForEndOfFrame();

        // 5. W³¹czamy kontroler z powrotem
        if (charController != null) charController.enabled = true;
    }
}