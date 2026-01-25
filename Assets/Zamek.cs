using System.Collections;
using UnityEngine;
using TMPro;

public class CombinationLock : MonoBehaviour
{
    [Header("Ustawienia Zamka")]
    public GameObject[] lockCylinders; 
    public int[] correctCombination;

    [Header("Ustawienia Kamery i Interakcji")]
    public Transform inspectionPoint;
    public float transitionSpeed = 2.5f;  
    public float interactionDistance = 4f;
    public LayerMask interactionLayer = ~0;

    [Header("Ustawienia Gracza")]
    public Transform playerRoot;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;

    [Header("UI Standardowe")]
    public TMP_Text statusText;
    public GameObject crosshair;

    [Header("NOWE: Tekst Sukcesu")]
    [Tooltip("Przeciągnij tutaj obiekt tekstowy z Canvy, który ma się pojawić po otwarciu.")]
    public GameObject successUIObject; 
    public float displayDuration = 5f;
    public float shakeAmount = 3f;

    [Header("Połączenia")]
    public DoorController doorToOpen;

    private int[] currentValues;
    private bool isInteracting = false;
    private Vector3 savedPlayerPos;
    private Quaternion savedPlayerRot;
    private Quaternion savedCameraRot;
    private Camera mainCam;
    private bool isRotating = false;
    private bool isLockSolved = false;
    private Coroutine moveRoutine; 

    void Start()
    {
        currentValues = new int[lockCylinders.Length];
        mainCam = Camera.main;

        // Upewniamy się, że tekst sukcesu jest schowany na starcie
        if (successUIObject != null) successUIObject.SetActive(false);
    }

    void Update()
    {
        if (isLockSolved) return;

        if (!isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
                {
                    if (hit.transform.GetComponentInParent<CombinationLock>() == this) EnterLockMode();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E)) ExitLockMode();

            if (Input.GetMouseButtonDown(0) && !isRotating)
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10f, interactionLayer)) CheckCylinderHit(hit.transform);
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
        currentValues[index] = (currentValues[index] + 1) % 10;

        Quaternion startRot = lockCylinders[index].transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, -36, 0);

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
            if (currentValues[i] != correctCombination[i]) { isCorrect = false; break; }
        }

        if (isCorrect)
        {
            isLockSolved = true; // Blokujemy zamek przed dalszą interakcją

            // --- TU ODPALAMY TWÓJ TEKST ---
            if (successUIObject != null) 
                StartCoroutine(ShakeAndFadeUI());

            if (statusText) { statusText.text = "OTWARTE"; statusText.color = Color.green; }
            if (doorToOpen != null) doorToOpen.OpenDoor();
            
            StartCoroutine(AutoExitAfterSuccess());
        }
    }

    // --- KORUTYNA EFEKTU TEKSTU ---
    IEnumerator ShakeAndFadeUI()
    {
        successUIObject.SetActive(true);
        
        // Dodajemy CanvasGroup jeśli go nie ma, by móc płynnie wygasić (Alpha)
        CanvasGroup group = successUIObject.GetComponent<CanvasGroup>();
        if (group == null) group = successUIObject.AddComponent<CanvasGroup>();
        
        RectTransform rect = successUIObject.GetComponent<RectTransform>();
        Vector2 originalPos = rect.anchoredPosition;
        group.alpha = 1f;

        float elapsed = 0f;

        // 1. Wyświetlanie i trzęsienie
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-shakeAmount, shakeAmount);
            float offsetY = Random.Range(-shakeAmount, shakeAmount);
            rect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        rect.anchoredPosition = originalPos; // Reset pozycji

        // 2. Płynne znikanie
        float fadeElapsed = 0f;
        while (fadeElapsed < 1f)
        {
            fadeElapsed += Time.deltaTime;
            group.alpha = 1f - fadeElapsed;
            yield return null;
        }

        successUIObject.SetActive(false);
    }

    IEnumerator AutoExitAfterSuccess()
    {
        yield return new WaitForSeconds(1.2f);
        ExitLockMode();
    }

    // --- LOGIKA KAMERY I RUCHU (BEZ ZMIAN) ---
    void EnterLockMode()
    {
        isInteracting = true;
        CharacterController cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        if (UIController.instance != null) UIController.instance.SetGameplayState(false);
        if (crosshair) crosshair.SetActive(false);

        savedPlayerPos = playerRoot.position;
        savedPlayerRot = playerRoot.rotation;
        savedCameraRot = mainCam.transform.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Vector3 targetPlayerPos = inspectionPoint.position - (mainCam.transform.position - playerRoot.position);
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MovePlayerRoutine(targetPlayerPos, inspectionPoint.rotation, true));
    }

    void ExitLockMode()
    {
        isInteracting = false;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MovePlayerRoutine(savedPlayerPos, savedCameraRot, false));
    }

    IEnumerator MovePlayerRoutine(Vector3 targetPos, Quaternion targetCamRot, bool toInspection)
    {
        float t = 0;
        Vector3 startPos = playerRoot.position;
        Quaternion startBodyRot = playerRoot.rotation;
        Quaternion startCamRot = mainCam.transform.rotation;
        Quaternion targetBodyRot = toInspection ? Quaternion.LookRotation(Vector3.ProjectOnPlane(targetCamRot * Vector3.forward, Vector3.up)) : savedPlayerRot;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothT = t * t * (3f - 2f * t);
            playerRoot.position = Vector3.Lerp(startPos, targetPos, smoothT);
            playerRoot.rotation = Quaternion.Slerp(startBodyRot, targetBodyRot, smoothT);
            mainCam.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, smoothT);
            yield return null;
        }

        if (!toInspection)
        {
            CharacterController cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
            if (UIController.instance != null) UIController.instance.SetGameplayState(true);
            if (crosshair) crosshair.SetActive(true);
        }
    }
}