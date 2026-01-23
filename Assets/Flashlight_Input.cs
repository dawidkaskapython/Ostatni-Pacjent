using UnityEngine;
using System.Collections;

public class FlashlightEffect : MonoBehaviour
{
    [Header("--- USTAWIENIA BOJOWE ---")]
    public float flashRange = 1f;
    public float beamRadius = 1.0f;

    [Header("--- AUDIO ---")]
    [Tooltip("Dźwięk ładowania (powinien być zapętlony w ustawieniach klipu)")]
    public AudioClip chargeSound;
    [Tooltip("Dźwięk samego błysku/strzału")]
    public AudioClip flashSound;
    private AudioSource audioSource;

    [Header("Ważne")]
    public Transform cameraTransform;
    public Transform flashlightColor;

    // --- CAMERA SHAKE ---
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 20f;
    private Vector3 originalCameraPos;

    // VALUES
    private Light yellowLight;
    public float baseSpotAngle = 56f;
    public float baseIntensity = 2f;
    public float chargeTargetSpot = 30f;
    public float chargeTargetIntensity = 5f;
    public float flashSpot = 150f;
    public float flashIntensity = 10f;
    public float chargeTime = 1f;
    public float flashHoldTime = 0.1f;
    public float cooldownTime = 2f;

    private float defaultLightRange;
    private bool isOnCooldown = false;

    // DEBUG (Linie pomocnicze)
    private Vector3 debugStart;
    private Vector3 debugEnd;
    private bool debugHit;
    private float debugHitSize;
    private float debugTimer = 0f;

    void Start()
    {
        // Konfiguracja AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Jeśli nie masz komponentu na obiekcie, skrypt sam go doda
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (cameraTransform == null)
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        if (flashlightColor != null)
        {
            yellowLight = flashlightColor.Find("YellowLight").GetComponent<Light>();
            yellowLight.spotAngle = baseSpotAngle;
            yellowLight.intensity = baseIntensity;
            defaultLightRange = yellowLight.range;
        }

        if (cameraTransform != null)
            originalCameraPos = cameraTransform.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isOnCooldown)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    IEnumerator ChargeRoutine()
    {
        float t = 0f;

        // Odtwarzaj dźwięk ładowania
        if (chargeSound != null)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        while (Input.GetKey(KeyCode.F))
        {
            t += Time.deltaTime;

            // Podnosimy wysokość dźwięku (pitch) wraz z ładowaniem dla lepszego efektu
            audioSource.pitch = Mathf.Lerp(1f, 1.5f, t / chargeTime);

            if (yellowLight != null)
            {
                yellowLight.spotAngle = Mathf.Lerp(baseSpotAngle, chargeTargetSpot, t / chargeTime);
                yellowLight.intensity = Mathf.Lerp(baseIntensity, chargeTargetIntensity, t / chargeTime);
            }

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraPos + (Random.insideUnitSphere * shakeAmount) * Mathf.Sin(Time.time * shakeSpeed);
            }

            if (t >= chargeTime)
            {
                audioSource.Stop(); // Zatrzymaj ładowanie
                audioSource.pitch = 1f;
                StartCoroutine(FlashSequence());
                ResetCamera();
                yield break;
            }
            yield return null;
        }

        // Jeśli puścisz F przed czasem
        audioSource.Stop();
        audioSource.pitch = 1f;
        ResetCamera();
        StartCoroutine(ReturnToBase());
    }

    IEnumerator FlashSequence()
    {
        isOnCooldown = true;

        // Odtwórz dźwięk strzału (jednorazowo)
        if (flashSound != null)
        {
            audioSource.PlayOneShot(flashSound);
        }

        if (yellowLight != null)
        {
            yellowLight.spotAngle = flashSpot;
            yellowLight.intensity = flashIntensity;
        }

        // --- LOGIKA ATAKU (bez zmian) ---
        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;
        Vector3 startPoint = origin + (direction * 0.5f);
        debugStart = startPoint;
        debugHit = false;
        debugTimer = 3.0f;

        RaycastHit[] hits = Physics.SphereCastAll(startPoint, beamRadius, direction, flashRange, ~0, QueryTriggerInteraction.Collide);
        RaycastHit bestHit = new RaycastHit();
        bool foundValidTarget = false;
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Enemy")) continue;
            if (hit.distance > flashRange) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                bestHit = hit;
                foundValidTarget = true;
            }
        }

        if (foundValidTarget)
        {
            debugEnd = startPoint + (direction * bestHit.distance);
            debugHit = true;
            debugHitSize = beamRadius;
            var enemyScript = bestHit.collider.GetComponentInParent<EnemyScript>();
            if (enemyScript != null) enemyScript.DieByFlashlight();
            else Destroy(bestHit.collider.gameObject);
        }
        else
        {
            debugEnd = startPoint + (direction * flashRange);
        }

        yield return new WaitForSeconds(flashHoldTime);

        if (yellowLight != null)
        {
            yellowLight.spotAngle = baseSpotAngle;
            yellowLight.intensity = baseIntensity;
        }

        yield return new WaitForSeconds(cooldownTime);
        if (yellowLight != null) yellowLight.range = defaultLightRange;
        isOnCooldown = false;
    }

    // Reszta metod (ResetCamera, ReturnToBase, OnDrawGizmos) pozostaje bez zmian
    void ResetCamera() { if (cameraTransform != null) cameraTransform.localPosition = originalCameraPos; }

    IEnumerator ReturnToBase()
    {
        float t = 0f;
        float duration = 0.2f;
        if (yellowLight != null)
        {
            float startSpot = yellowLight.spotAngle;
            float startIntensity = yellowLight.intensity;
            while (t < duration)
            {
                t += Time.deltaTime;
                yellowLight.spotAngle = Mathf.Lerp(startSpot, baseSpotAngle, t / duration);
                yellowLight.intensity = Mathf.Lerp(startIntensity, baseIntensity, t / duration);
                yield return null;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Vector3 start = cameraTransform.position + (cameraTransform.forward * 0.5f);
            Gizmos.DrawRay(start, cameraTransform.forward * flashRange);
            Gizmos.DrawWireSphere(start + (cameraTransform.forward * flashRange), beamRadius);
        }
        if (debugTimer > 0)
        {
            debugTimer -= Time.deltaTime;
            Gizmos.color = debugHit ? Color.green : Color.red;
            Gizmos.DrawLine(debugStart, debugEnd);
            Gizmos.DrawWireSphere(debugEnd, debugHit ? debugHitSize : beamRadius);
        }
    }
}