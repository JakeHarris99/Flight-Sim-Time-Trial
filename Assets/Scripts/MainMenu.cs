using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Loads game scene
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    //Closes application
    public void ExitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
