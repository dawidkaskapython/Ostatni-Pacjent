using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    private CharacterController charController;
    private Vector3 startPosition;
    public static Transform currentCheckpoint;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        startPosition = transform.position;
    }

    void OnEnable()
    {
        // Zmieniamy: najpierw pokazujemy UI zamiast od razu teleportowa
        PlayerEvents.OnPlayerDeath += ShowDeathUI;
    }

    void OnDisable()
    {
        PlayerEvents.OnPlayerDeath -= ShowDeathUI;
    }

    // Nowa metoda wywoywana przez event
    private void ShowDeathUI()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowDeathScreen();
        }
    }

    // T metod podepniesz pod przycisk Respawn na Canvasie
    public void StartRespawnProcess()
    {
        if (UIController.instance != null)
        {
            UIController.instance.HideDeathScreen();
        }
        Respawn();
    }

    public void Respawn()
    {
        Vector3 respawnPos = currentCheckpoint != null
            ? currentCheckpoint.position
            : startPosition;

        StartCoroutine(TeleportRoutine(respawnPos));
    }

    IEnumerator TeleportRoutine(Vector3 targetPos)
    {
        if (charController != null) charController.enabled = false;

        yield return new WaitForEndOfFrame();
        transform.position = targetPos;
        yield return new WaitForEndOfFrame();

        if (charController != null) charController.enabled = true;

        // Po teleportacji wracamy do gry
        if (UIController.instance != null)
        {
            UIController.instance.SetGameplayState(true);
        }
    }
}