using System.Collections;
using UnityEngine;
using TMPro;

public class CombinationLock : MonoBehaviour
{
    [Header("Ustawienia Zamka")]
    public GameObject[] lockCylinders; // Tablica 3 cylindrów
    public int[] correctCombination;   // Prawid³owy kod, np. 1, 2, 3

    [Header("Ustawienia Kamery i Interakcji")]
    public Transform inspectionPoint;   // Pusty obiekt: Gdzie kamera ma siê ustawiæ
    public float transitionSpeed = 2f;  // Szybkoœæ zbli¿ania kamery
    public float interactionDistance = 4f;
    public LayerMask interactionLayer = ~0;

    [Header("Ustawienia Gracza")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;

    [Header("UI")]
    public TMP_Text statusText;
    public GameObject crosshair;

    [Header("Po³¹czenia")]
    public DoorController doorToOpen;

    // Prywatne zmienne stanu
    private int[] currentValues;
    private bool isInteracting = false;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Camera mainCam;
    private bool isRotating = false;
    private bool isLockSolved = false;

    private Coroutine cameraRoutine; // Referencja do ruchu kamery

    void Start()
    {
        currentValues = new int[lockCylinders.Length];
        mainCam = Camera.main;

        if (inspectionPoint == null) Debug.LogError("B£¥D: Nie przypisano 'InspectionPoint'!");
        if (mainCam == null) Debug.LogError("B£¥D: Brak MainCamera!");

        // Zapamiêtujemy pozycjê startow¹ kamery wzglêdem gracza
        originalLocalPosition = mainCam.transform.localPosition;
        originalLocalRotation = mainCam.transform.localRotation;
    }

    void Update()
    {
        if (isLockSolved) return;

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
        // 2. Tryb interakcji
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

        currentValues[index]++;
        if (currentValues[index] > 9) currentValues[index] = 0;

        Quaternion startRot = lockCylinders[index].transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, -36, 0);

        float t = 0;
        float speed = 5f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
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

            if (doorToOpen != null)
            {
                doorToOpen.OpenDoor();
                isLockSolved = true;
                StartCoroutine(AutoExitAfterSuccess());
            }
        }
    }

    IEnumerator AutoExitAfterSuccess()
    {
        yield return new WaitForSeconds(1.0f);
        ExitLockMode();
    }

    // --- LOGIKA KAMERY ---

    void EnterLockMode()
    {
        isInteracting = true;

        // Zapisujemy aktualn¹ pozycjê lokaln¹ na wypadek, gdyby gracz siê zmieni³
        originalLocalPosition = mainCam.transform.localPosition;
        originalLocalRotation = mainCam.transform.localRotation;

        if (playerMovementScript) playerMovementScript.enabled = false;
        if (cameraLookScript) cameraLookScript.enabled = false;
        if (crosshair) crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        cameraRoutine = StartCoroutine(MoveCameraRoutine(inspectionPoint.position, inspectionPoint.rotation, true));
    }

    void ExitLockMode()
    {
        isInteracting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshair) crosshair.SetActive(true);

        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        cameraRoutine = StartCoroutine(MoveCameraRoutine(Vector3.zero, Quaternion.identity, false));
    }

    IEnumerator MoveCameraRoutine(Vector3 targetPos, Quaternion targetRot, bool toInspection)
    {
        float t = 0;
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        // Kluczowe zabezpieczenie: transitionSpeed musi byæ > 0
        float safeSpeed = Mathf.Max(transitionSpeed, 0.1f);

        while (t < 1f)
        {
            t += Time.deltaTime * safeSpeed;

            if (toInspection)
            {
                // Ruch do punktu w œwiecie
                mainCam.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }
            else
            {
                // Powrót do lokalnej pozycji wewn¹trz g³owy gracza
                mainCam.transform.localPosition = Vector3.Lerp(mainCam.transform.localPosition, originalLocalPosition, t);
                mainCam.transform.localRotation = Quaternion.Slerp(mainCam.transform.localRotation, originalLocalRotation, t);
            }
            yield return null;
        }

        if (toInspection)
        {
            mainCam.transform.position = targetPos;
            mainCam.transform.rotation = targetRot;
        }
        else
        {
            mainCam.transform.localPosition = originalLocalPosition;
            mainCam.transform.localRotation = originalLocalRotation;

            // W³¹czamy skrypty dopiero PO zakoñczeniu ruchu powrotnego
            if (playerMovementScript) playerMovementScript.enabled = true;
            if (cameraLookScript) cameraLookScript.enabled = true;
        }
    }
}