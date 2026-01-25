using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Potrzebne do obsługi Image (tła krwi)
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Player Control")]
    public Behaviour playerMovementScript; 
    public FirstPersonLook cameraLookScript;     
    public Rigidbody playerRigidbody;

    [Header("UI Elements")]
    public GameObject levelEndScreen;
    public string mainMenuName;
    public GameObject pauseScreen;
    public TMP_Text messageText;
    public GameObject flashlightUI; // Slider lub Canvas latarki do ukrycia

    [Header("Splash Screens (Czarne Plansze)")]
    public GameObject splashScreenObject;
    public TMP_Text splashText;
    public CanvasGroup splashCanvasGroup;
    public float fadeDuration = 1.5f;

    [Header("Death Screen Animation")]
    public GameObject deathScreenObject;
    public CanvasGroup deathPanelGroup;  // CanvasGroup całego panelu tła
    public CanvasGroup youDiedTextGroup; // CanvasGroup napisu "YOU DIED"
    public GameObject respawnButton;    // Przycisk respawnu
    public Image bloodBackground;       // Obrazek tła (ustaw mu kolor czerwony)

    private bool isPaused = false;
    private bool isSplashActive = false;
    private bool isEndingSplash = false;
    private bool isDead = false;

    private void Awake() => instance = this;

    void Start()
    {
        AudioListener.pause = false;

        if (splashScreenObject != null) ShowStartSplash();
        else SetGameplayState(true);

        // Upewniamy się, że ekran śmierci jest wyłączony na starcie
        if (deathScreenObject != null) deathScreenObject.SetActive(false);
    }

    void Update()
    {
        // Jeśli gracz nie żyje, blokujemy resztę sterowania UI
        if (isDead) return;

        if (isSplashActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartCoroutine(FadeAndCloseSplash());
        }

        if (!isSplashActive && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }

        if (isPaused || isSplashActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- LOGIKA EKRANU ŚMIERCI Z ANIMACJĄ ---

    public void ShowDeathScreen()
    {
        if (isDead) return;
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        isDead = true;
        SetGameplayState(false); // Blokada ruchu i myszki

        // 1. Ukrywamy latarkę
        if (flashlightUI != null) flashlightUI.SetActive(false);

        // 2. Przygotowujemy panel (wszystko na 0)
        deathScreenObject.SetActive(true);
        deathPanelGroup.alpha = 0f;
        youDiedTextGroup.alpha = 0f;
        respawnButton.SetActive(false);

        // Zatrzymujemy czas, ale animacje będą działać (unscaledDeltaTime)
        Time.timeScale = 0f;

        // 3. FADE IN KRWI (Tło)
        float elapsed = 0f;
        float bloodFadeDuration = 2.0f;
        while (elapsed < bloodFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            deathPanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / bloodFadeDuration);
            yield return null;
        }

        // 4. FADE IN "YOU DIED"
        elapsed = 0f;
        float textFadeDuration = 1.5f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            youDiedTextGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeDuration);
            yield return null;
        }

        // 5. POKAZUJEMY PRZYCISK
        yield return new WaitForSecondsRealtime(0.5f);
        respawnButton.SetActive(true);
        
        // Gwarancja widoczności kursora
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideDeathScreen()
    {
        isDead = false;
        if (deathScreenObject != null) deathScreenObject.SetActive(false);
        if (flashlightUI != null) flashlightUI.SetActive(true);
        
        Time.timeScale = 1f;
        // SetGameplayState(true) zostanie wywołane przez PlayerRespawn po teleportacji
    }

    // --- RESZTA LOGIKI ---

    public void ResumeGame() { if (isPaused) PauseUnpause(); }

    public void PauseUnpause()
    {
        if (isSplashActive || isDead) return;
        isPaused = !isPaused;
        pauseScreen.SetActive(isPaused);
        
        Time.timeScale = isPaused ? 0f : 1f;
        SetGameplayState(!isPaused);
    }

    public void SetGameplayState(bool active)
    {
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

    public void ShowStartSplash()
    {
        isSplashActive = true; isEndingSplash = false;
        if (splashCanvasGroup != null) splashCanvasGroup.alpha = 1f;
        splashScreenObject.SetActive(true);

        if (splashText)
        {
            splashText.text = "<b>„Azyl Świtu”, rok 1984.</b>\n\n" +
                              "Głowa pulsuje mi tępym bólem, a w ustach czuję metaliczny posmak taniej wódki i kurzu. " +
                              "Pamiętam tylko śmiech kolegów z huty i ostatni toast... a potem nastała ciemność.\n\n" +
                              "Budzę się w miejscu, o którym krążą legendy szeptane przy zgaszonym świetle. " +
                              "Powietrze śmierdzi chlorem, stęchlizną i czymś jeszcze... Nie jestem tu sam.\n\n" +
                              "<size=80%><color=#888888>[E] to Twój przycisk interakcji.\n" + 
                              "Naciśnij [SPACJĘ], aby otworzyć oczy...</color></size>";
        }
        SetGameplayState(false);
        Time.timeScale = 0f; 
    }

    public void ShowEndSplash()
    {
        isSplashActive = true; isEndingSplash = true;
        if (splashCanvasGroup != null) splashCanvasGroup.alpha = 1f;
        splashScreenObject.SetActive(true);

        if (splashText)
        {
            splashText.text = "<b>Ciemno... nagle zrobiło mi się zupełnie ciemno...</b>\n\n" +
                              "Co się dzieje? Słyszę ich, jest ich tam mnóstwo, krzyczą coś do siebie. Czuję, jak mnie łapią, szarpią za ręce, rzucam się, chcę uciec, ale trzymają mnie za mocno.\n\n" +
                              "Co to za smród? Alkohol? Chusteczka... przyciskają mi ją do twarzy. Nie mogę oddychać. Boże, zakładają mi kaftan, czuję te pasy na piersiach, nie mam siły się ruszyć. " +
                              "Muszę... muszę wytrzymać, nie mogę teraz zasnąć... ale wszystko mi odpływa. Nie daję rady.\n\n" +
                              "<size=80%><color=#888888>Naciśnij [SPACJĘ], aby wrócić na oddział...</color></size>";
        }
        SetGameplayState(false);
        Time.timeScale = 0f;
    }

    private IEnumerator FadeAndCloseSplash()
    {
        isSplashActive = false;
        if (isEndingSplash) {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }
        if (splashCanvasGroup != null) {
            float currentTime = 0;
            while (currentTime < fadeDuration) {
                currentTime += Time.unscaledDeltaTime;
                splashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
                yield return null;
            }
        }
        splashScreenObject.SetActive(false);
        SetGameplayState(true);
        Time.timeScale = 1f;
    }

    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuName); }
    public void QuitGame() => Application.Quit();
}