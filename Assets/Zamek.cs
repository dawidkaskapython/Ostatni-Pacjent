using System.Collections;
using UnityEngine;
using TMPro; // Pamiêtaj o TextMeshPro

public class CombinationLock : MonoBehaviour
{
    [Header("Ustawienia Zamka")]
    public GameObject[] lockCylinders; // Tablica 3 cylindrów (przypisz w kolejnoœci od lewej do prawej)
    public int[] correctCombination;   // Prawid³owy kod, np. 1, 2, 3

    [Header("Ustawienia Kamery")]
    public Transform inspectionPoint;  // Pusty obiekt: Gdzie kamera ma siê ustawiæ
    public float transitionSpeed = 2f; // Szybkoœæ zbli¿ania

    [Header("Ustawienia Gracza")]
    public MonoBehaviour playerMovementScript; // Skrypt ruchu gracza
    public MonoBehaviour cameraLookScript;     // Skrypt obracania kamer¹

    [Header("UI")]
    public TMP_Text statusText;       // Tekst "Otwarto" lub "Z³y kod"
    public GameObject crosshair;      // Celownik (warto go ukryæ przy zbli¿eniu)

    // Prywatne zmienne stanu
    private int[] currentValues;      // Przechowuje aktualne cyfry na cylindrach
    private bool isInteracting = false;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Camera mainCam;
    private bool isRotating = false;  // Zabezpieczenie przed spamowaniem klikniêæ

    void Start()
    {
        // Inicjalizacja tablicy wartoœci (domyœlnie same zera)
        currentValues = new int[lockCylinders.Length];
        mainCam = Camera.main;
    }

    void Update()
    {
        // 1. Jeœli nie interagujemy, sprawdzamy czy gracz celuje w "Zamek" i klika E
        if (!isInteracting)
        {
            // ZMIANA: Wejœcie klawiszem E
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                // Zak³adamy, ¿e ten skrypt jest na obiekcie, który ma Collider (obudowa zamka)
                if (Physics.Raycast(ray, out hit, 3f))
                {
                    if (hit.transform == this.transform || IsPartOfLock(hit.transform))
                    {
                        EnterLockMode();
                    }
                }
            }
        }
        // 2. Jeœli interagujemy (jesteœmy w zbli¿eniu)
        else
        {
            // ZMIANA: Wyjœcie klawiszem E (zamiast Escape)
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitLockMode();
            }

            // Klikanie w cylindry (nadal lewym przyciskiem myszy, bo to wygodne)
            if (Input.GetMouseButtonDown(0) && !isRotating)
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    CheckCylinderHit(hit.transform);
                }
            }
        }
    }

    // Sprawdza czy klikniêty obiekt to jeden z cylindrów
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

    // Obraca cylinder i aktualizuje matematykê
    IEnumerator RotateCylinder(int index)
    {
        isRotating = true;

        // 1. Aktualizacja wartoœci (matematyka)
        currentValues[index]++;
        if (currentValues[index] > 9) currentValues[index] = 0;

        // --- LOGOWANIE WARTOŒCI ---
        Debug.Log($"Cylinder {index} (obrócony) wskazuje teraz: {currentValues[index]}");
        // -------------------------------

        // 2. Obrót wizualny (animacja)
        Quaternion startRot = lockCylinders[index].transform.localRotation;

        // ZMIANA: Obrót na osi Y (druga oœ) o -36 stopni
        Quaternion endRot = startRot * Quaternion.Euler(0, -36, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // Szybkoœæ obrotu cylindra
            lockCylinders[index].transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        // Upewniamy siê, ¿e rotacja jest idealna na koniec
        lockCylinders[index].transform.localRotation = endRot;

        isRotating = false;

        // 3. SprawdŸ czy kod jest poprawny
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
            if (statusText) statusText.text = "SZYFR POPRAWNY!";
            statusText.color = Color.green;
            Debug.Log("Otwarto!");
            // Tutaj dodaj logikê otwierania drzwi/sejfów
        }
        else
        {
            if (statusText) statusText.text = ""; // Czyœcimy tekst jak jest Ÿle
        }
    }

    // --- LOGIKA KAMERY I BLOKADY GRACZA ---

    void EnterLockMode()
    {
        isInteracting = true;

        // Zapisz pozycjê gracza
        originalCameraPosition = mainCam.transform.position;
        originalCameraRotation = mainCam.transform.rotation;
        originalCameraParent = mainCam.transform.parent;

        // Zablokuj ruch gracza
        if (playerMovementScript) playerMovementScript.enabled = false;
        if (cameraLookScript) cameraLookScript.enabled = false;
        if (crosshair) crosshair.SetActive(false);

        // Poka¿ kursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Rozpocznij animacjê kamery
        StopAllCoroutines();
        StartCoroutine(MoveCameraToInspection());
    }

    void ExitLockMode()
    {
        isInteracting = false;

        // Ukryj kursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshair) crosshair.SetActive(true);

        // Rozpocznij powrót kamery
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

        // Przywróæ sterowanie po zakoñczeniu animacji
        if (playerMovementScript) playerMovementScript.enabled = true;
        if (cameraLookScript) cameraLookScript.enabled = true;

        // Upewnij siê, ¿e kamera wróci³a idealnie na miejsce
        mainCam.transform.position = originalCameraPosition;
        mainCam.transform.rotation = originalCameraRotation;
    }

    // Pomocnicza funkcja sprawdzaj¹ca czy kliknêliœmy w czêœæ zamka (np. cylindry)
    bool IsPartOfLock(Transform t)
    {
        foreach (var cyl in lockCylinders)
        {
            if (t.gameObject == cyl) return true;
        }
        return false;
    }
}