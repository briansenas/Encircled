
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerConfiguration playerConfig;
    private PlayerMovement mover;

    [SerializeField]
    private SpriteRenderer playerMesh;

    private PlayerControls controls;

    private void Awake()
    {
        mover = GetComponent<PlayerMovement>();
        controls = new PlayerControls();
    }

    public void InitializePlayer(PlayerConfiguration config)
    {
        playerConfig = config;
        playerMesh.color = config.playerMaterial.color;
        config.Input.onActionTriggered += Input_onActionTriggered;
    }   

    private void Input_onActionTriggered(CallbackContext obj)
    {
        if (obj.action.name == controls.Land.Move.name)
        {
            onMove(obj);
        }

        if (obj.action.name == controls.Land.Jump.name)
        {
            onJump(obj);
        }

        if (obj.action.name == controls.Land.Dash.name)
        {
            onDash(obj);
        }
    }

	public void onMove(InputAction.CallbackContext context){
        if(mover != null)
            mover.onMove(context); 
	}

	public void onJump(InputAction.CallbackContext context){
        if(mover != null)
            mover.onJump(context); 
	} 

	public void onDash(InputAction.CallbackContext context){ 
        if(mover != null)
            mover.onDash(context); 
	}

}