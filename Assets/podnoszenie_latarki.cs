using UnityEngine;
using System.Collections;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 4f;
    public LayerMask interactionLayer = ~0;

    [Header("G³ówne Aktywacje")]
    public GameObject playerFlashlight;      // Ca³a latarka (¿eby j¹ w³¹czyæ)
    public MonoBehaviour flashlightEffectScript; // Twój skrypt efektów na kamerze

    [Header("Ustawienia Migania (Custom)")]
    public GameObject flickerTarget;         // WSKA¯ CO MA MIGAC (np. samo œwiat³o)
    public int flickerCount = 3;             // Ile razy mrugnie
    public float flickerSpeed = 0.2f;        // Prêdkoœæ mrugania (wiêksza liczba = wolniej)
    public float darkPause = 1.0f;           // Ile sekund ciemnoœci przed zapaleniem

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        // Wy³¹czamy latarkê gracza na starcie
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
                // Logika "tylko jeœli patrzê na tê latarkê"
                FlashlightPickup hitPickup = hit.transform.GetComponentInParent<FlashlightPickup>();
                if (hitPickup == this) Interact();
            }
        }
    }

    void Interact()
    {
        // 1. W³¹czamy g³ówny obiekt i skrypt
        if (playerFlashlight != null) playerFlashlight.SetActive(true);
        if (flashlightEffectScript != null) flashlightEffectScript.enabled = true;

        // 2. Odpalamy miganie na wybranym celu
        if (flickerTarget != null)
        {
            // Odpalamy korutynê na kamerze, bo ta latarka na ziemi zostanie zniszczona
            mainCam.GetComponent<MonoBehaviour>().StartCoroutine(CustomFlickerRoutine());
        }

        Destroy(gameObject); // Usuwamy latarkê z ziemi
    }

    private IEnumerator CustomFlickerRoutine()
    {
        // Pêtla migania
        for (int i = 0; i < flickerCount; i++)
        {
            ToggleTarget(false);
            yield return new WaitForSeconds(flickerSpeed);
            ToggleTarget(true);
            yield return new WaitForSeconds(flickerSpeed);
        }

        // Przerwa w ciemnoœci
        ToggleTarget(false);
        yield return new WaitForSeconds(darkPause);

        // Zapalenie na sta³e
        ToggleTarget(true);
    }

    private void ToggleTarget(bool state)
    {
        if (flickerTarget == null) return;

        // Wy³¹cza/w³¹cza œwiat³a wewn¹trz celu
        Light[] lights = flickerTarget.GetComponentsInChildren<Light>();
        foreach (Light l in lights) l.enabled = state;

        // Wy³¹cza/w³¹cza model/renderery wewn¹trz celu
        Renderer[] renderers = flickerTarget.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = state;
    }
}