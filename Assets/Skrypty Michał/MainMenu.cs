using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneToLoad;   // <- nazwa sceny do odpalenia

    public void StartGame()
    {
<<<<<<< HEAD
        SceneManager.LoadScene(sceneToLoad);
=======
        SceneManager.LoadScene("LATARKA_DEMO");
>>>>>>> 0f2b88e1d246902b3854ba15a31a290d4a84ca3e
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
