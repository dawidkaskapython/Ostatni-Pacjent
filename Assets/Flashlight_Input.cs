using UnityEngine;
using System.Collections;

public class FlashlightEffect : MonoBehaviour
{
    [Header("--- USTAWIENIA BOJOWE ---")]
    [Tooltip("Maksymalny zasięg ataku w metrach")]
    public float flashRange = 1f;

    [Tooltip("Szerokość promienia (im mniejszy, tym precyzyjniejszy zasięg)")]
    public float beamRadius = 1.0f;

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

    // Zapamiętujemy domyślny zasięg światła, żeby go przywrócić po strzale
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
        // Automatyczne przypisanie kamery
        if (cameraTransform == null)
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
            else Debug.LogError("BRAK KAMERY! Przypisz MainCamera w Inspectorze.");
        }

        if (flashlightColor != null)
        {
            yellowLight = flashlightColor.Find("YellowLight").GetComponent<Light>();
            yellowLight.spotAngle = baseSpotAngle;
            yellowLight.intensity = baseIntensity;

            // Zapamiętaj ustawienie z Inspectora
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
        while (Input.GetKey(KeyCode.F))
        {
            t += Time.deltaTime;
            // Animacja ładowania
            if (yellowLight != null)
            {
                yellowLight.spotAngle = Mathf.Lerp(baseSpotAngle, chargeTargetSpot, t / chargeTime);
                yellowLight.intensity = Mathf.Lerp(baseIntensity, chargeTargetIntensity, t / chargeTime);
            }
            // Trzęsienie kamerą
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraPos + (Random.insideUnitSphere * shakeAmount) * Mathf.Sin(Time.time * shakeSpeed);
            }

            if (t >= chargeTime)
            {
                StartCoroutine(FlashSequence());
                ResetCamera();
                yield break;
            }
            yield return null;
        }
        ResetCamera();
        StartCoroutine(ReturnToBase());
    }

    void ResetCamera()
    {
        if (cameraTransform != null) cameraTransform.localPosition = originalCameraPos;
    }

    IEnumerator FlashSequence()
    {
        isOnCooldown = true;

        if (yellowLight != null)
        {
            yellowLight.spotAngle = flashSpot;
            yellowLight.intensity = flashIntensity;
        }

        // ───────────────────────────────────────────────
        // LOGIKA ATAKU Z "BEZPIECZNIKIEM" DYSTANSU

        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        // Startujemy minimalnie przed kamerą
        Vector3 startPoint = origin + (direction * 0.5f);

        debugStart = startPoint;
        debugHit = false;
        debugTimer = 3.0f; // Debug wyświetla się przez 3 sekundy

        // Pobieramy WSZYSTKO na drodze
        RaycastHit[] hits = Physics.SphereCastAll(startPoint, beamRadius, direction, flashRange, ~0, QueryTriggerInteraction.Collide);

        RaycastHit bestHit = new RaycastHit();
        bool foundValidTarget = false;
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // 1. FILTR: Musi być WRÓG
            if (!hit.collider.CompareTag("Enemy")) continue;

            // 2. FILTR: Musi być BLISKO (Naprawa błędu "zabijania z kilometra")
            // Ignorujemy trafienia, które matematycznie są dalej niż limit
            if (hit.distance > flashRange)
            {
                // Debug.Log($"Ignoruję wroga {hit.collider.name} bo jest za daleko ({hit.distance}m > {flashRange}m)");
                continue;
            }

            // Szukamy najbliższego z poprawnych celów
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                bestHit = hit;
                foundValidTarget = true;
            }
        }

        // Wykonanie wyroku
        if (foundValidTarget)
        {
            debugEnd = startPoint + (direction * bestHit.distance);
            debugHit = true;
            debugHitSize = beamRadius;

            Debug.Log($"<color=red>!!! TRAFIONO WROGA !!!</color> Dystans: {bestHit.distance:F1}m / Limit: {flashRange}m");

            var enemyScript = bestHit.collider.GetComponentInParent<EnemyScript>();
            if (enemyScript != null) enemyScript.DieByFlashlight();
            else Destroy(bestHit.collider.gameObject);
        }
        else
        {
            // Pudło
            debugEnd = startPoint + (direction * flashRange);
            Debug.Log($"<color=gray>[PUDŁO] Nic w zasięgu {flashRange}m.</color>");
        }

        // ───────────────────────────────────────────────

        yield return new WaitForSeconds(flashHoldTime);

        // Powrót do normy
        if (yellowLight != null)
        {
            yellowLight.spotAngle = baseSpotAngle;
            yellowLight.intensity = baseIntensity;
        }

        yield return new WaitForSeconds(cooldownTime);

        // Przywracamy stary zasięg światła
        if (yellowLight != null) yellowLight.range = defaultLightRange;

        isOnCooldown = false;
    }

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

    // --- WIZUALIZACJA ---
    void OnDrawGizmos()
    {
        // 1. PODGLĄD NA ŻYWO (SZARY) - pokazuje aktualne ustawienia przed strzałem
        if (cameraTransform != null)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Vector3 start = cameraTransform.position + (cameraTransform.forward * 0.5f);
            Gizmos.DrawRay(start, cameraTransform.forward * flashRange);
            Gizmos.DrawWireSphere(start + (cameraTransform.forward * flashRange), beamRadius);
        }

        // 2. WYNIK STRZAŁU (KOLOROWY) - zostaje na ekranie przez 3 sekundy
        if (debugTimer > 0)
        {
            debugTimer -= Time.deltaTime;
            Gizmos.color = debugHit ? Color.green : Color.red;
            Gizmos.DrawLine(debugStart, debugEnd);
            Gizmos.DrawWireSphere(debugEnd, debugHit ? debugHitSize : beamRadius);
        }
    }
}