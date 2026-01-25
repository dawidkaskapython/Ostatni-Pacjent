using UnityEngine;
using System.Collections;

public class VampireEnemy : MonoBehaviour
{
    [Header("Efekty Śmierci")]
    public GameObject dustExplosionPrefab; // Tylko efekt końcowy (pył)
    
    [Header("Ustawienia")]
    public float burningTime = 2.0f; // Jak długo trwa proces "smażenia"

    private bool isDying = false;
    private Renderer myRenderer;
    private Color originalColor;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            originalColor = myRenderer.material.color; 
        }
    }

    public void BurnAndDie()
    {
        if (isDying) return;
        StartCoroutine(BurningRoutine());
    }

    IEnumerator BurningRoutine()
    {
        isDying = true;

        float timer = 0f;
        Vector3 startPos = transform.position;

        // Pętla "Smażenia" (bez ognia, tylko trzęsienie i kolor)
        while (timer < burningTime)
        {
            timer += Time.deltaTime;
            float progress = timer / burningTime;

            // 1. ZMIANA KOLORU (Czerwony -> Czarny)
            if (myRenderer != null)
            {
                if (progress < 0.5f)
                    myRenderer.material.color = Color.Lerp(originalColor, Color.red, progress * 2);
                else
                    myRenderer.material.color = Color.Lerp(Color.red, Color.black, (progress - 0.5f) * 2);
            }

            // 2. TRZĘSIENIE
            transform.position = startPos + (Random.insideUnitSphere * 0.1f);

            // 3. SKALOWANIE (Kurczenie)
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, progress);

            yield return null;
        }

        // FINAŁ - Wybuch w pył
        if (dustExplosionPrefab != null)
        {
            Instantiate(dustExplosionPrefab, transform.position, Quaternion.identity);
        }

        // Usuwamy wroga
        Destroy(gameObject);
    }
}