using UnityEngine;
using System.Collections;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 4f;
    public LayerMask interactionLayer = ~0;

    [Header("G³ówne Aktywacje (Gracz)")]
    public GameObject playerFlashlight;      // Obiekt latarki u gracza
    public MonoBehaviour flashlightEffectScript; // Skrypt na kamerze gracza

    [Header("Œwiat³o Podœwietlaj¹ce (Scena)")]
    public GameObject highlightLight;        // PRZECI¥GNIJ TU ŒWIAT£O, KTÓRE MA ZGASN¥Æ NA STA£E

    [Header("Ustawienia Migania (Latarka Gracza)")]
    public GameObject flickerTarget;         // Co ma migaæ u gracza (np. ¿arówka)
    public int flickerCount = 3;
    public float flickerSpeed = 0.2f;
    public float darkPause = 1.0f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        // Przygotowanie stanu pocz¹tkowego
        if (playerFlashlight != null) playerFlashlight.SetActive(false);
        if (flashlightEffectScript != null) flashlightEffectScript.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
            {
                FlashlightPickup hitPickup = hit.transform.GetComponentInParent<FlashlightPickup>();
                if (hitPickup == this) Interact();
            }
        }
    }

    void Interact()
    {
        // 1. NATYCHMIASTOWE zgaszenie œwiat³a podœwietlaj¹cego przedmiot na ziemi
        if (highlightLight != null)
        {
            highlightLight.SetActive(false);
            Debug.Log("Œwiat³o pomocnicze zgaszone na sta³e.");
        }

        // 2. W³¹czenie latarki gracza
        if (playerFlashlight != null) playerFlashlight.SetActive(true);
        if (flashlightEffectScript != null) flashlightEffectScript.enabled = true;

        // 3. Odpalenie migania tylko dla latarki gracza
        if (flickerTarget != null)
        {
            mainCam.GetComponent<MonoBehaviour>().StartCoroutine(CustomFlickerRoutine());
        }

        // 4. Usuniêcie modelu z ziemi
        Destroy(gameObject);
    }

    private IEnumerator CustomFlickerRoutine()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            ToggleFlickerTarget(false);
            yield return new WaitForSeconds(flickerSpeed);
            ToggleFlickerTarget(true);
            yield return new WaitForSeconds(flickerSpeed);
        }

        ToggleFlickerTarget(false);
        yield return new WaitForSeconds(darkPause);

        ToggleFlickerTarget(true);
    }

    private void ToggleFlickerTarget(bool state)
    {
        if (flickerTarget == null) return;

        foreach (Light l in flickerTarget.GetComponentsInChildren<Light>()) l.enabled = state;
        foreach (Renderer r in flickerTarget.GetComponentsInChildren<Renderer>()) r.enabled = state;
    }
}