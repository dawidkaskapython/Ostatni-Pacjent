using UnityEngine;
using System.Collections;

public class FlashlightEffect : MonoBehaviour
{
    public Transform flashlightColor;
    private Light yellowLight;
    public float flashRange = 100f;

    // --- CAMERA SHAKE ---
    public Transform cameraTransform;
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 20f;
    private Vector3 originalCameraPos;
    // ---------------------

    // BASE VALUES
    private float baseSpotAngle = 56f;
    private float baseIntensity = 2f;

    // CHARGE TARGET VALUES
    public float chargeTargetSpot = 30f;
    public float chargeTargetIntensity = 5f;

    // FLASH VALUES
    public float flashSpot = 150f;
    public float flashIntensity = 10f;

    public float chargeTime = 1f;
    public float flashHoldTime = 0.1f;
    public float cooldownTime = 2f;

    private bool isOnCooldown = false;

    void Start()
    {
        yellowLight = flashlightColor.Find("YellowLight").GetComponent<Light>();
        yellowLight.spotAngle = baseSpotAngle;
        yellowLight.intensity = baseIntensity;

        // zapamiętaj oryginalną pozycję kamery
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

            yellowLight.spotAngle = Mathf.Lerp(baseSpotAngle, chargeTargetSpot, t / chargeTime);
            yellowLight.intensity = Mathf.Lerp(baseIntensity, chargeTargetIntensity, t / chargeTime);

            // --- CAMERA SHAKE ---
            if (cameraTransform != null)
            {
                cameraTransform.localPosition =
                    originalCameraPos +
                    (Random.insideUnitSphere * shakeAmount) *
                    Mathf.Sin(Time.time * shakeSpeed);
            }
            // ---------------------

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
        if (cameraTransform != null)
            cameraTransform.localPosition = originalCameraPos;
    }

    IEnumerator FlashSequence()
    {
        isOnCooldown = true;

        yellowLight.spotAngle = flashSpot;
        yellowLight.intensity = flashIntensity;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, flashRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
            }
        }

        yield return new WaitForSeconds(flashHoldTime);

        yellowLight.spotAngle = baseSpotAngle;
        yellowLight.intensity = baseIntensity;

        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    IEnumerator ReturnToBase()
    {
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
