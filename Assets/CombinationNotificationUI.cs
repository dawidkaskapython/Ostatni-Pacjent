using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class CombinationNotificationUI : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private Vector3 originalPos;

    [Header("Ustawienia Efektu")]
    public float fadeInDuration = 1.0f;
    public float displayDuration = 3.0f;
    public float fadeOutDuration = 1.0f;
    
    [Header("Ustawienia Trzęsienia")]
    public float shakeIntensity = 2.0f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup != null) canvasGroup.alpha = 0f; 
        originalPos = transform.localPosition;
    }

    // Wywołujemy to bez żadnych parametrów
    public void TriggerSuccess()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        float elapsed = 0;
        // 1. FADE IN
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            ApplyShake();
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 2. STAY & SHAKE
        elapsed = 0;
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            ApplyShake();
            yield return null;
        }

        // 3. FADE OUT
        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            ApplyShake();
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        transform.localPosition = originalPos;
    }

    private void ApplyShake()
    {
        float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
        float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
        transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
    }
}