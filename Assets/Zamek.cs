using System.Collections;
using UnityEngine;
using TMPro;

public class CombinationLock : MonoBehaviour
{
    [Header("Ustawienia Zamka")]
    public GameObject[] lockCylinders; // Tablica 3 cylindrów (przypisz obiekty w Unity)
    public int[] correctCombination;   // Prawid³owy kod, np. 1, 2, 3

    [Header("Ustawienia Kamery i Interakcji")]
    public Transform inspectionPoint;  // Pusty obiekt: Gdzie kamera ma siê ustawiæ
    public float transitionSpeed = 2f; // Szybkoœæ zbli¿ania kamery
    public float interactionDistance = 4f; // Zasiêg dzia³ania klawisza E
    public LayerMask interactionLayer = ~0; // Warstwy

    [Header("Ustawienia Gracza")]
    public MonoBehaviour playerMovementScript; // Skrypt ruchu gracza
    public MonoBehaviour cameraLookScript;     // Skrypt rozgl¹dania siê

    [Header("UI")]
    public TMP_Text statusText;       // Tekst stanu (np. "Otwarto")
    public GameObject crosshair;      // Celownik

    [Header("Po³¹czenia")]
    public DoorController doorToOpen; // <-- TU PRZYPISZ DRZWI ZE SKRYPTEM DoorController

    // Prywatne zmienne stanu
    private int[] currentValues;      // Przechowuje aktualne cyfry na cylindrach
    private bool isInteracting = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Camera mainCam;
    private bool isRotating = false;
    private bool isLockSolved = false; // Blokada po otwarciu

    void Start()
    {
        currentValues = new int[lockCylinders.Length];
        mainCam = Camera.main;

        if (inspectionPoint == null) Debug.LogError("B£¥D: Nie przypisano 'InspectionPoint'!");
        if (mainCam == null) Debug.LogError("B£¥D: Brak MainCamera!");
    }

    void Update()
    {
        if (isLockSolved) return; // Jeœli zamek otwarty, nic nie rób

        // 1. Tryb chodzenia
        if (!isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
                {
                    CombinationLock hitLock = hit.transform.GetComponentInParent<CombinationLock>();
                    if (hitLock == this)
                    {
                        EnterLockMode();
                    }
                }
            }
        }
        // 2. Tryb interakcji (zbli¿enie)
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitLockMode();
            }

            if (Input.GetMouseButtonDown(0) && !isRotating)
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10f, interactionLayer))
                {
                    CheckCylinderHit(hit.transform);
                }
            }
        }
    }

    void CheckCylinderHit(Transform hitObject)
    {
        for (int i = 0; i < lockCylinders.Length; i++)
        {
            if (hitObject.gameObject == lockCylinders[i])
            {
                StartCoroutine(RotateCylinder(i));
                return;
            }
        }
    }

    IEnumerator RotateCylinder(int index)
    {
        isRotating = true;

        // Matematyka wartoœci
        currentValues[index]++;
        if (currentValues[index] > 9) currentValues[index] = 0;

        // Animacja obrotu
        Quaternion startRot = lockCylinders[index].transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, -36, 0); // Zmieñ oœ, jeœli trzeba

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            lockCylinders[index].transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        lockCylinders[index].transform.localRotation = endRot;

        isRotating = false;

        CheckCombination();
    }

    void CheckCombination()
    {
        bool isCorrect = true;
        for (int i = 0; i < correctCombination.Length; i++)
        {
            if (currentValues[i] != correctCombination[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            if (statusText)
            {
                statusText.text = "OTWARTE";
                statusText.color = Color.green;
            }

            Debug.Log("SZYFR POPRAWNY - OTWIERANIE DRZWI!");

            // --- NOWA CZÊŒÆ: OTWIERANIE DRZWI ---
            if (doorToOpen != null)
            {
                doorToOpen.OpenDoor();
                isLockSolved = true; // Zapobiega dalszemu klikaniu

                // Opcjonalnie: Automatyczne wyjœcie z trybu zamka po 1 sekundzie
                StartCoroutine(AutoExitAfterSuccess());
            }
            else
            {
                Debug.LogWarning("Przypisz obiekt drzwi w polu 'Door To Open' w Inspectorze!");
            }
        }
        else
        {
            if (statusText) statusText.text = "";
        }
    }

    // Dodatek: Czeka chwilê i wychodzi z trybu zamka po otwarciu
    IEnumerator AutoExitAfterSuccess()
    {
        yield return new WaitForSeconds(1.0f);
        ExitLockMode();
    }

    // --- LOGIKA KAMERY ---

    void EnterLockMode()
    {
        isInteracting = true;
        originalCameraPosition = mainCam.transform.position;
        originalCameraRotation = mainCam.transform.rotation;

        if (playerMovementScript) playerMovementScript.enabled = false;
        if (cameraLookScript) cameraLookScript.enabled = false;
        if (crosshair) crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StopAllCoroutines();
        StartCoroutine(MoveCameraToInspection());
    }

    void ExitLockMode()
    {
        isInteracting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshair) crosshair.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ReturnCameraToPlayer());
    }

    IEnumerator MoveCameraToInspection()
    {
        float t = 0;
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCam.transform.position = Vector3.Lerp(startPos, inspectionPoint.position, t);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, inspectionPoint.rotation, t);
            yield return null;
        }
        mainCam.transform.position = inspectionPoint.position;
        mainCam.transform.rotation = inspectionPoint.rotation;
    }

    IEnumerator ReturnCameraToPlayer()
    {
        float t = 0;
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCam.transform.position = Vector3.Lerp(startPos, originalCameraPosition, t);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, originalCameraRotation, t);
            yield return null;
        }
        mainCam.transform.position = originalCameraPosition;
        mainCam.transform.rotation = originalCameraRotation;

        if (playerMovementScript) playerMovementScript.enabled = true;
        if (cameraLookScript) cameraLookScript.enabled = true;
    }
}