using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;
    public Rigidbody playerRigidbody;

    [Header("UI Elements")]
    public GameObject levelEndScreen;
    public string mainMenuName;
    public GameObject pauseScreen;
    public TMP_Text messageText;

    [Header("Splash Screens (Czarne Plansze)")]
    public GameObject splashScreenObject;
    public TMP_Text splashText;
    public CanvasGroup splashCanvasGroup;
    public float fadeDuration = 1.5f;

    private bool isPaused = false;
    private bool isSplashActive = false;
    private bool isEndingSplash = false;

    private void Awake() => instance = this;

    void Start()
    {
        if (splashScreenObject != null)
        {
            ShowStartSplash();
        }
        else
        {
            SetGameplayState(true); // Normalny start gry
        }
    }

    void Update()
    {
        // 1. Obs³uga planszy (Spacja/Enter)
        if (isSplashActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartCoroutine(FadeAndCloseSplash());
        }

        // 2. Obs³uga Pauzy (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }

        // 3. PANCERNA BLOKADA MYSZY
        // Jeœli gra jest zatrzymana, wymuszamy widocznoœæ kursora w ka¿dej klatce.
        // To odcina sterowanie kamer¹ w wiêkszoœci gotowych kontrolerów Unity.
        if (isPaused || isSplashActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        if (isPaused) PauseUnpause();
    }

    public void ShowStartSplash()
    {
        isSplashActive = true;
        isEndingSplash = false;

        if (splashCanvasGroup != null) splashCanvasGroup.alpha = 1f;
        splashScreenObject.SetActive(true);

        if (splashText)
        {
            splashText.text = "<b>„Azyl Œwitu”, rok 1984.</b>\n\n" +
                              "G³owa pulsuje mi têpym bólem, a w ustach czujê metaliczny posmak taniej wódki i kurzu. " +
                              "Pamiêtam tylko œmiech kolegów z huty i ostatni toast... a potem nasta³a ciemnoœæ.\n\n" +
                              "Budzê siê w miejscu, o którym kr¹¿¹ legendy szeptane przy zgaszonym œwietle. " +
                              "Powietrze œmierdzi chlorem, stêchlizn¹ i czymœ jeszcze... Nie jestem tu sam.\n\n" +
                              "<size=80%><color=#888888>[E] to Twój przycisk interakcji.\n" + 
                              "Naciœnij [SPACJÊ], aby otworzyæ oczy...</color></size>";
        }

        SetGameplayState(false);
    }

    public void ShowEndSplash()
    {
        isSplashActive = true;
        isEndingSplash = true;

        if (splashCanvasGroup != null) splashCanvasGroup.alpha = 1f;
        splashScreenObject.SetActive(true);

        if (splashText)
        {
            splashText.text = "<b>Ciemno... nagle zrobi³o mi siê zupe³nie ciemno...</b>\n\n" +
                              "Co siê dzieje? S³yszê ich, jest ich tam mnóstwo, krzycz¹ coœ do siebie. Czujê, jak mnie ³api¹, szarpi¹ za rêce, rzucam siê, chcê uciec, ale trzymaj¹ mnie za mocno.\n\n" +
                              "Co to za smród? Alkohol? Chusteczka... przyciskaj¹ mi j¹ do twarzy. Nie mogê oddychaæ. Bo¿e, zak³adaj¹ mi kaftan, czujê te pasy na piersiach, nie mam si³y siê ruszyæ. " +
                              "Muszê... muszê wytrzymaæ, nie mogê teraz zasn¹æ... ale wszystko mi odp³ywa. Nie dajê rady.\n\n" +
                              "<size=80%><color=#888888>Naciœnij [SPACJÊ], aby wróciæ na oddzia³...</color></size>";
        }

        SetGameplayState(false);
    }

    private IEnumerator FadeAndCloseSplash()
    {
        isSplashActive = false;

        if (isEndingSplash)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }

        if (splashCanvasGroup != null)
        {
            float currentTime = 0;
            while (currentTime < fadeDuration)
            {
                currentTime += Time.unscaledDeltaTime;
                splashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
                yield return null;
            }
        }

        splashScreenObject.SetActive(false);
        SetGameplayState(true);
    }

    public void PauseUnpause()
    {
        if (isSplashActive) return;

        isPaused = !isPaused;
        pauseScreen.SetActive(isPaused);
        SetGameplayState(!isPaused);
    }

    // G£ÓWNA METODA ZARZ¥DZAJ¥CA STANEM GRY
    private void SetGameplayState(bool active)
    {
        Time.timeScale = active ? 1f : 0f;

        if (playerMovementScript != null) playerMovementScript.enabled = active;
        if (cameraLookScript != null) cameraLookScript.enabled = active;

        if (active)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuName);
    }

    public void QuitGame() => Application.Quit();
}