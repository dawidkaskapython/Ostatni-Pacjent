using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class FlashlightEffect : MonoBehaviour
{
    [Header("--- UI ---")]
    public Slider chargeSlider; 

    [Header("--- AUDIO ---")]
    public AudioClip chargeSound; 
    public AudioClip flashSound;  
    private AudioSource mySpeaker; 

    [Header("Ważne Referencje")]
    public Transform cameraTransform;
    public Transform flashlightColor; 

    [Header("Wartości Światła")]
    private Light yellowLight;
    public float baseSpotAngle = 60f;
    public float baseIntensity = 2f;
    public float chargeTargetSpot = 15f;
    public float chargeTargetIntensity = 8f;
    public float flashSpot = 175f;
    public float flashIntensity = 50f;

    [Header("Czasy")]
    public float chargeTime = 1.2f;
    public float flashHoldTime = 0.1f;
    public float cooldownTime = 1.5f;

    private bool isOnCooldown = false;

    void Start()
    {
        mySpeaker = GetComponent<AudioSource>();
        SetupAudioSource(); 

        if (chargeSlider != null) chargeSlider.value = 1f;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        if (flashlightColor != null)
        {
            yellowLight = flashlightColor.GetComponentInChildren<Light>();
            ResetLightInstant();
        }
    }

    private void SetupAudioSource()
    {
        mySpeaker.playOnAwake = false;
        mySpeaker.spatialBlend = 0f;          
        mySpeaker.ignoreListenerPause = true; 
        mySpeaker.volume = 1f;                
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isOnCooldown)
        {
            StopAllCoroutines(); 
            StartCoroutine(ChargeRoutine());
        }
    }

    // Natychmiastowy reset (używany po strzale/flashu)
    private void ResetLightInstant()
    {
        if (yellowLight != null)
        {
            yellowLight.spotAngle = baseSpotAngle;
            yellowLight.intensity = baseIntensity;
        }
        if (chargeSlider != null) chargeSlider.value = 1f;
    }

    IEnumerator ChargeRoutine()
    {
        float t = 0f;

        if (chargeSound != null)
        {
            mySpeaker.clip = chargeSound;
            mySpeaker.loop = true;
            mySpeaker.pitch = 1f;
            mySpeaker.Play();
        }

        while (Input.GetKey(KeyCode.F))
        {
            t += Time.unscaledDeltaTime; 
            float progress = Mathf.Clamp01(t / chargeTime);

            if (chargeSlider != null) chargeSlider.value = 1f - progress;

            if (yellowLight != null)
            {
                yellowLight.spotAngle = Mathf.Lerp(baseSpotAngle, chargeTargetSpot, progress);
                yellowLight.intensity = Mathf.Lerp(baseIntensity, chargeTargetIntensity, progress);
            }

            mySpeaker.pitch = Mathf.Lerp(1f, 1.8f, progress);

            if (t >= chargeTime)
            {
                mySpeaker.Stop();
                StartCoroutine(FlashSequence());
                yield break;
            }
            yield return null;
        }

        // Jeśli puścisz przycisk przed ładowaniem - POWOLNY POWRÓT
        mySpeaker.Stop();
        StartCoroutine(ReturnToBaseGradual());
    }

    IEnumerator FlashSequence()
    {
        isOnCooldown = true;

        if (flashSound != null) 
            mySpeaker.PlayOneShot(flashSound);

        if (yellowLight != null)
        {
            yellowLight.spotAngle = flashSpot;
            yellowLight.intensity = flashIntensity;
        }

        RaycastHit[] hits = Physics.SphereCastAll(cameraTransform.position, 2f, cameraTransform.forward, 15f);
        foreach (RaycastHit hit in hits)
        {
            VampireEnemy vEnem = hit.collider.GetComponent<VampireEnemy>();
            if (vEnem != null) vEnem.BurnAndDie();
        }

        yield return new WaitForSecondsRealtime(flashHoldTime);
        
        // PO FLASHU - NATYCHMIASTOWY POWRÓT
        ResetLightInstant();

        // Cooldown suwaka (ładowanie od 0 do 1)
        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.unscaledDeltaTime;
            if (chargeSlider != null) chargeSlider.value = timer / cooldownTime;
            yield return null;
        }

        if (chargeSlider != null) chargeSlider.value = 1f;
        isOnCooldown = false;
    }

    // Korutyna do powolnego wracania (używana tylko przy przerwaniu ładowania)
    IEnumerator ReturnToBaseGradual()
    {
        float t = 0f;
        float duration = 0.5f; // Czas powrotu w sekundach

        float startAngle = yellowLight.spotAngle;
        float startIntensity = yellowLight.intensity;
        float startSlider = chargeSlider != null ? chargeSlider.value : 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;

            if (yellowLight != null)
            {
                yellowLight.spotAngle = Mathf.Lerp(startAngle, baseSpotAngle, p);
                yellowLight.intensity = Mathf.Lerp(startIntensity, baseIntensity, p);
            }

            if (chargeSlider != null) 
                chargeSlider.value = Mathf.Lerp(startSlider, 1f, p);

            yield return null;
        }
        ResetLightInstant();
    }
}