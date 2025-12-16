using UnityEngine;
using System.Collections;

public class FlickerJumpscare : MonoBehaviour
{
    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // Przeciągnij tu skrypt ruchu gracza
    public MonoBehaviour cameraLookScript;     // (Opcjonalne) Przeciągnij tu skrypt kamery/myszki

    [Header("Monster Spawn")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;

    [Header("Light Settings")]
    public Light pointLight;
    public float flickerDuration = 4f;
    public Vector2 flickerIntervalRange = new Vector2(0.05f, 0.3f);

    [Header("Audio")]
    public AudioClip jumpscareSound;
    public float soundVolume = 1f;

    private bool triggered = false;
    private GameObject monsterInstance;
    private AudioSource audioSource;

    private void Start()
    {
        // Tworzymy AudioSource w runtime
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // 1. ZABLOKUJ RUCH (i kamerę) NA STARCIE
        TogglePlayerMovement(false);

        StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        // Spawn potwora
        monsterInstance = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

        // Odpal dźwięk
        if (jumpscareSound != null)
        {
            audioSource.transform.position = spawnPoint.position;
            audioSource.PlayOneShot(jumpscareSound, soundVolume);
        }

        pointLight.enabled = true;

        float timer = 0f;

        // Pętla migania
        while (timer < flickerDuration)
        {
            pointLight.enabled = !pointLight.enabled;

            if (!pointLight.enabled)
                monsterInstance.SetActive(false);
            else
                monsterInstance.SetActive(true);

            float interval = Random.Range(flickerIntervalRange.x, flickerIntervalRange.y);
            yield return new WaitForSeconds(interval);
            timer += interval;
        }

        // KONIEC SCENKI

        // Ustawienia światła i sprzątanie potwora
        pointLight.enabled = true;

        if (monsterInstance != null)
            Destroy(monsterInstance);

        // 2. ODBLOKUJ RUCH PO ZAKOŃCZENIU
        TogglePlayerMovement(true);
    }

    // Pomocnicza funkcja do włączania/wyłączania sterowania
    private void TogglePlayerMovement(bool enable)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = enable;

        if (cameraLookScript != null)
            cameraLookScript.enabled = enable;

        // Opcjonalnie: Jeśli gracz ma Rigidbody i chcesz go zatrzymać w miejscu natychmiast:
        Rigidbody rb = playerMovementScript.GetComponent<Rigidbody>();
        if (rb != null && !enable)
        {
            rb.linearVelocity = Vector3.zero; // W starszych Unity użyj: rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}