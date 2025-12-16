using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Player Control - Przypisz skrypty tutaj")]
    // Skrypt odpowiadaj¹cy za chodzenie (np. PlayerMovement)
    public MonoBehaviour playerMovementScript;
    // Skrypt odpowiadaj¹cy za rozgl¹danie siê (jeœli jest osobny, np. MouseLook)
    public MonoBehaviour cameraLookScript;
    // Opcjonalnie: Rigidbody gracza, ¿eby zatrzymaæ poœlizg
    public Rigidbody playerRigidbody;

    [Header("UI Elements")]
    public GameObject levelEndScreen;
    public string mainMenuName;
    public GameObject pauseScreen;
    public TMP_Text messageText;

    // Flaga sprawdzaj¹ca czy gra jest zapauzowana
    private bool isPaused = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. Na start blokujemy myszkê i ukrywamy j¹
        LockCursor();

        // Upewniamy siê, ¿e menu pauzy jest wy³¹czone na starcie
        if (pauseScreen) pauseScreen.SetActive(false);
    }

    void Update()
    {
        // Obs³uga klawisza ESCAPE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void PauseUnpause()
    {
        // Jeœli menu koñca poziomu jest aktywne, nie pozwól na pauzowanie
        if (levelEndScreen.activeSelf) return;

        if (!isPaused)
        {
            // === W£¥CZ PAUZÊ ===
            isPaused = true;
            pauseScreen.SetActive(true);

            // Zatrzymaj czas w grze
            Time.timeScale = 0f;

            // Odblokuj kursor myszy (¿eby klikaæ w menu)
            UnlockCursor();

            // WY£¥CZ sterowanie gracza
            EnablePlayerControl(false);
        }
        else
        {
            // === WY£¥CZ PAUZÊ (WRÓÆ DO GRY) ===
            isPaused = false;
            pauseScreen.SetActive(false);

            // Wznów czas
            Time.timeScale = 1f;

            // Zablokuj kursor myszy (¿eby celowaæ)
            LockCursor();

            // W£¥CZ sterowanie gracza
            EnablePlayerControl(true);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Wa¿ne: przywróæ czas przed zmian¹ sceny
        UnlockCursor();
        SceneManager.LoadScene(mainMenuName);
    }

    public void QuitGame()
    {
        Debug.Log("Wychodzenie z gry...");
        Application.Quit();
    }

    public void ShowLevelEndScreen(bool levelCompleted)
    {
        levelEndScreen.SetActive(true);
        UnlockCursor();
        EnablePlayerControl(false);
    }

    public void ShowMessage(string message)
    {
        if (messageText) messageText.text = message;
    }

    public void HideMessage()
    {
        if (messageText) messageText.text = "";
    }

    // --- METODY POMOCNICZE ---

    private void EnablePlayerControl(bool enable)
    {
        // 1. Wy³¹cz/W³¹cz chodzenie
        if (playerMovementScript != null)
            playerMovementScript.enabled = enable;

        // 2. Wy³¹cz/W³¹cz rozgl¹danie siê (jeœli masz osobny skrypt kamery)
        if (cameraLookScript != null)
            cameraLookScript.enabled = enable;

        // 3. (Opcjonalnie) Wyzeruj prêdkoœæ, ¿eby gracz nie "œlizga³ siê" po pauzie
        if (!enable && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero; // W Unity 6 u¿yj linearVelocity, w starszych velocity
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}