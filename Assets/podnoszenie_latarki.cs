using System.Diagnostics;
using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 4f;
    public LayerMask interactionLayer = ~0;

    [Header("Co ma siê staæ?")]
    public GameObject playerFlashlight; // Obiekt latarki u gracza
    public MonoBehaviour flashlightEffectScript; // Skrypt Flashlight Effect

    [Header("Efekt Migania")]
    public MonoBehaviour flickerScript; // PRZECI¥GNIJ TU SKRYPT OD MIGANIA

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (playerFlashlight != null)
            playerFlashlight.SetActive(false);

        if (flashlightEffectScript != null)
            flashlightEffectScript.enabled = false;
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

                if (hitPickup == this)
                {
                    Interact();
                }
            }
        }
    }

    void Interact()
    {
        // 1. Aktywujemy podstawowe elementy latarki
        if (playerFlashlight != null) playerFlashlight.SetActive(true);
        if (flashlightEffectScript != null) flashlightEffectScript.enabled = true;

        // 2. ODPALAMY MIGOTANIE
        if (flickerScript != null)
        {
            flickerScript.StartManualFlicker();
            Debug.Log("Latarka podniesiona i zaczê³a migaæ!");
        }

        // 3. Usuwamy latarkê z ziemi
        Destroy(gameObject);
    }
}