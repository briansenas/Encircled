using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseMenu;
    public Button resumeButton; 

    public void Start(){
      if(resumeButton) 
        resumeButton.Select();  
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PlayerConfigurationManager.Instance.isPaused = false;
        Destroy(pauseMenu); 
        Destroy(this); 
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        PlayerConfigurationManager.Instance.isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
