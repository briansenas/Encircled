using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;

public class GameOverScript : MonoBehaviour
{
  [SerializeField]
  public TextMeshProUGUI playersText; 

  private float ignoreInputTime = 1.5f;
  private bool inputEnabled; 


  public void Setup(int playerIndex){ 
    playersText.text = "Player " + ( playerIndex + 1 ) + " WON!"; 
  }

  // Update is called once per frame
  void Update()
  {
    if(Time.time > ignoreInputTime)
    {
      inputEnabled = true;
    }
  }

  public void MainMenu()
  {
    if(!inputEnabled) { return; }
    SceneManager.LoadScene("MainMenu"); 

  }

  public void Restart()
  {
    if (!inputEnabled) { return; }
    SceneManager.LoadScene("GameScene");
  }
}
