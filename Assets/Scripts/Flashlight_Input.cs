using UnityEngine;
using System.Collections;

public class FlashlightEffect : MonoBehaviour
{
    public Transform flashlightColor;
    private Light yellowLight;
    public float flashRange = 100f;


    // BASE VALUES
    private float baseSpotAngle = 56f;
    private float baseIntensity = 2f;

    // CHARGE TARGET VALUES
    public float chargeTargetSpot = 30f;
    public float chargeTargetIntensity = 5f;

    // FLASH VALUES
    public float flashSpot = 150f;
    public float flashIntensity = 10f;

    public float chargeTime = 1f;        // ile musisz trzymać F
    public float flashHoldTime = 0.1f;   // czas flasha
    public float cooldownTime = 2f;      // cooldown

    private bool isOnCooldown = false;

    void Start()
    {
        yellowLight = flashlightColor.Find("YellowLight").GetComponent<Light>();
        yellowLight.spotAngle = baseSpotAngle;
        yellowLight.intensity = baseIntensity;
    }

    void Update()
    {
        // jeśli wciskasz F i nie ma cooldownu
        if (Input.GetKeyDown(KeyCode.F) && !isOnCooldown)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    IEnumerator ChargeRoutine()
    {
        float t = 0f;

        // Dopóki trzymasz F – ładowanie
        while (Input.GetKey(KeyCode.F))
        {
            t += Time.deltaTime;

            // płynne przechodzenie do celów
            yellowLight.spotAngle = Mathf.Lerp(baseSpotAngle, chargeTargetSpot, t / chargeTime);
            yellowLight.intensity = Mathf.Lerp(baseIntensity, chargeTargetIntensity, t / chargeTime);

            // jeśli minęła wymagana sekunda → FLASH
            if (t >= chargeTime)
            {
                StartCoroutine(FlashSequence());
                yield break;
            }

            yield return null;
        }

        // Jeśli puściłeś wcześniej → powrót do bazowych wartości
        StartCoroutine(ReturnToBase());
    }

    IEnumerator FlashSequence()
    {
        isOnCooldown = true;

        // FLASH
        yellowLight.spotAngle = flashSpot;
        yellowLight.intensity = flashIntensity;

        // ───────────────────────────────────────────────
        // 1. Wypuszczenie promienia po flashu
        RaycastHit hit;

        // Używamy pozycji i kierunku obiektu z tym skryptem (najczęściej kamera)
        if (Physics.Raycast(transform.position, transform.forward, out hit, flashRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
        // ───────────────────────────────────────────────

        yield return new WaitForSeconds(flashHoldTime);

        // 2. Powrót
        yellowLight.spotAngle = baseSpotAngle;
        yellowLight.intensity = baseIntensity;

        // 3. Cooldown
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }


    IEnumerator ReturnToBase()
    {
        // powrót do bazowych gdy przerwiesz ładowanie
        float t = 0f;
        float duration = 0.2f;

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
