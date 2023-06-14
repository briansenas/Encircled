using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using static UnityEngine.InputSystem.InputAction;

public class SpawnPlayerSetupMenu : MonoBehaviour
{
  public GameObject playerSetupMenuPrefab;

  private GameObject rootMenu;
  public PlayerInput input;

  private GameObject menu; 
  private PlayerSetupMenuController _playerSetupMenuController; 

  private void Awake()
  {
    rootMenu = GameObject.Find("SetupPanels");
    if(rootMenu != null)
    {
      menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform, false);
      menu.name="Player " + input.playerIndex.ToString();
      input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
      _playerSetupMenuController = menu.GetComponent<PlayerSetupMenuController>(); 
      _playerSetupMenuController.setPlayerIndex(input.playerIndex);
      _playerSetupMenuController.setPlayerInput(input);
    }

  }
}
