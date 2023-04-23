using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    public void PlayGame(){ 
        SceneManager.LoadScene("PlayerSetup");
    }

    public void GoToSettings(){
        SceneManager.LoadScene("SettingsMenu"); 
    }

    public void GoToMainMenu(){ 
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame(){
        Application.Quit(); 
    }
}
