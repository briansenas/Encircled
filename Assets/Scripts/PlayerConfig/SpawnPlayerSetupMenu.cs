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

    private void Awake()
    {
        rootMenu = GameObject.Find("SetupPanels");
        if(rootMenu != null)
        {
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform, false);
            input.currentActionMap.FindAction("Cancel").performed += 
                  menu.GetComponentInChildren<PlayerSetupMenuController>().Undo; 
            input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().setPlayerIndex(input.playerIndex);
        }
        
    }
}
