using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.SceneManagement;

public class PlayerSetupMenuController : MonoBehaviour
{
  private int playerIndex;

  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private GameObject readyPanel;
  [SerializeField]
  private GameObject menuPanel;
  [SerializeField]
  private Button readyButton;
  [SerializeField]
  private Button menuButton;

  private float ignoreInputTime = .4f;
  private bool inputEnabled;
  private PlayerInput _input; 

  public void setPlayerIndex(int pi)
  {
    playerIndex = pi;
    titleText.SetText("Player " + (pi + 1).ToString());
    ignoreInputTime = Time.time + ignoreInputTime;
  }

  // Update is called once per frame
  void Update()
  {
    if(Time.time > ignoreInputTime)
    {
      inputEnabled = true;
    }
  }

  public void SelectColor(Material mat)
  {
    if(!inputEnabled) { return; }

    PlayerConfigurationManager.Instance.SetPlayerColor(playerIndex, mat);
    readyPanel.SetActive(true);
    readyButton.interactable = true;
    menuPanel.SetActive(false);
    readyButton.Select();

  }

  public void ReadyPlayer()
  {
    if (!inputEnabled) { return; }

    PlayerConfigurationManager.Instance.ReadyPlayer(playerIndex);
    readyButton.gameObject.SetActive(false);
  }

  public void setPlayerInput(PlayerInput _playerInput){
    _input = _playerInput;
    _input.currentActionMap.FindAction("Cancel").performed += Undo;
  }

  public void Undo(CallbackContext context) { 
    if (!inputEnabled) { return ; } 
    if (readyPanel.activeSelf == true && readyButton.gameObject.activeSelf == false) {
      PlayerConfigurationManager.Instance.UnReadyPlayer(playerIndex);  
      readyButton.gameObject.SetActive(true); 
    }
    else if (readyPanel.activeSelf == true) {
      menuPanel.SetActive(true); 
      menuButton.interactable = true; 
      readyPanel.SetActive(false); 
      readyButton.interactable = false; 
      this.GetComponentInChildren<EventSystem>().SetSelectedGameObject(menuButton.gameObject); 
    }
    else if (menuPanel.activeSelf == true 
        && PlayerConfigurationManager.Instance.isEveryoneNotReady()) 
      SceneManager.LoadScene("MainMenu");
  }

  public void OnDisable(){
    if (_input) 
      _input.currentActionMap.FindAction("Cancel").performed -= Undo;
  }
}
