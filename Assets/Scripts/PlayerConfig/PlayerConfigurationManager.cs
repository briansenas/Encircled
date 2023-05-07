using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;
    [SerializeField]
    private int MinPlayers = 1; 

    public GameObject playerGameOverPrefab;

    public static PlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a seccond instance of a singleton class.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
        
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        pi.transform.SetParent(transform);

        if(!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
    }

    public void HandleDeath(int index)
    {
        playerConfigs[index].isDead = true;
        if (playerConfigs.All(p => p.isDead == true))
        {
            var rootMap = GameObject.Find("Map1"); 
            rootMap.SetActive(false); 
            var rootMenu = GameObject.Find("GameOverLayout");
            if(rootMenu != null)
            {
                var menu = Instantiate(playerGameOverPrefab, rootMenu.transform);
                playerConfigs[0].Input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
                menu.GetComponentInChildren<GameOverScript>().Setup(index);
            }
        }
    }


    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public PlayerConfiguration GetPlayerConfig(int index)
    {
        return playerConfigs[index];
    }

    public void SetPlayerColor(int index, Material color)
    {
        playerConfigs[index].playerMaterial = color;
    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true;
        if (playerConfigs.Count >= MinPlayers && playerConfigs.All(p => p.isReady == true))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        isReady = false; 
        Input = pi;
        isDead = true; 
    }

    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    public Material playerMaterial {get; set;}
    public bool isDead {get; set;}
}