using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseMenu;
    public Button resumeButton; 
    public float waitFor = .5f; 

    public void Start(){
      if(resumeButton) 
        resumeButton.Select();  
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PlayerConfigurationManager.Instance.isPaused = false;
        Camera.main.GetComponent<CameraFollowPath>().waitFor = waitFor;
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
