using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // <-- tu przeci¹gasz PlayerMovement !!!

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
            // PAUSE
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnablePlayer(false);
        }
        else
        {
            // UNPAUSE
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            EnablePlayer(true);
        }
    }

    public void ShowLevelEndScreen(bool levelCompleted)
    {
        levelEndScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnablePlayer(false);
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
    }

    public void HideMessage()
    {
        messageText.text = "";
    }

    // --- NOWE: W£¥CZ/WY£¥CZ PLAYER MOVEMENT ---
    private void EnablePlayer(bool enable)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = enable;
    }
}
