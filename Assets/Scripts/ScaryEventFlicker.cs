using UnityEngine;
using System.Collections;

public class FlickerJumpscare : MonoBehaviour
{
    [Header("Monster Spawn")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;

    [Header("Light Settings")]
    public Light pointLight;
    public float flickerDuration = 4f;
    public Vector2 flickerIntervalRange = new Vector2(0.05f, 0.3f);

    [Header("Audio")]
    public AudioClip jumpscareSound;   // dźwięk jumpscare’a
    public float soundVolume = 1f;

    private bool triggered = false;
    private GameObject monsterInstance;
    private AudioSource audioSource;

    private void Start()
    {
        // Tworzymy AudioSource w runtime – najprościej i najpewniej
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        // Spawn potwora
        monsterInstance = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

        // Odpal dźwięk w tym samym momencie
        if (jumpscareSound != null)
        {
            audioSource.transform.position = spawnPoint.position; // dźwięk dochodzi z miejsca spawnu
            audioSource.PlayOneShot(jumpscareSound, soundVolume);
        }

        pointLight.enabled = true;

        float timer = 0f;

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

        // Końcowe ustawienia: światło ON, potwór znika
        pointLight.enabled = true;

        if (monsterInstance != null)
            Destroy(monsterInstance);
    }
}