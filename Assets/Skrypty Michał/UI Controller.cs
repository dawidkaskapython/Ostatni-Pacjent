using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject levelEndScreen;
    public string mainMenuName;
    public GameObject pauseScreen;
    public TMP_Text messageText;

    void Start()
    {
        messageText.text = "";

        // 1. Na start blokujemy myszkê na œrodku i j¹ ukrywamy (¿eby graæ)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Odblokuj myszkê przed wyjœciem do menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseUnpause()
    {
        if (pauseScreen.activeSelf == false)
        {
            // --- PAUZA (W³¹czamy menu) ---
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;

            // 2. Odblokuj myszkê, ¿eby gracz móg³ klikaæ w przyciski
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // --- POWRÓT DO GRY (Wy³¹czamy menu) ---
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;

            // 3. Zablokuj myszkê z powrotem na œrodku ekranu
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowLevelEndScreen(bool levelCompleted)
    {
        levelEndScreen.SetActive(true);

        // 4. Na ekranie koñcowym te¿ chcemy myszkê
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
    }

    public void HideMessage()
    {
        messageText.text = "";
    }
}