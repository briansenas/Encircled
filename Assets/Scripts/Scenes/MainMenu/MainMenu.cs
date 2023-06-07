using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void Awake(){ 
        var configManager = GameObject.FindGameObjectWithTag("PlayerConfigurationManager");
        if(configManager){
            Destroy(configManager); 
        }
    }
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
