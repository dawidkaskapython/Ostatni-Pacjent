using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public string firstLevelName;


    public void StartGame()
    {
        SceneManager.LoadScene("LATARKA_DEMO");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
