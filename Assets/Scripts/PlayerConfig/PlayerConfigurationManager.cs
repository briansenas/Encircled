using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;
    [SerializeField]
    private int MinPlayers = 1; 
    [SerializeField]
    private float waitForTimer = 5f; 
    private float waitFor = 0f; 

    private bool startWaiting = false; 
    private bool isLoaded = false; 

    public GameObject playerGameOverPrefab;
    [SerializeField]
    private TextMeshProUGUI TimerText;

    public static PlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a seccond instance of a singleton class.");
        }
        else
        {
            waitFor = waitForTimer; 
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
        playerConfigs[index].Die(); 
        if (playerConfigs.All(p => p.isDead == true))
        {
            var rootMap = GameObject.Find("GameMap"); 
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

    public bool isEveryoneReady(){
        return playerConfigs.Count >= MinPlayers && playerConfigs.All(p => p.isReady == true); 
    }

    public bool isEveryoneNotReady(){
      return playerConfigs.All(p => p.isReady == false); 
    }


    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true;
        if (isEveryoneReady())
        {
          startWaiting = true; 
        }
    }

    public void UnReadyPlayer(int index) { 
        playerConfigs[index].isReady = false; 
        startWaiting = false;  
    } 

    public void FixedUpdate(){
      if (!isLoaded){
         if (startWaiting){ 
           updateTimer(waitFor);
           waitFor -= Time.deltaTime;
           if (waitFor <= 0 && !isLoaded) {
             isLoaded = true; 
            SceneManager.LoadScene("GameScene");
           }
         } else{
           if (TimerText) TimerText.enabled = false; 
           waitFor = waitForTimer; 
         }
      }
    }

    public void updateTimer(float time_){
        if (TimerText) TimerText.enabled=true;
        time_ += 1;
        float minutes = Mathf.FloorToInt(time_ / 60); 
        float seconds = Mathf.FloorToInt(time_ % 60); 

        TimerText.text = string.Format("Steady: {00}", seconds);
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

    public void InitializePlayer(GameObject self_){
      isDead = false; 
      isReady = true; 
      this_ = self_;
      this_.GetComponent<SpriteRenderer>().enabled=true;
    }

    public void Die() {
      isDead = true; 
      if(this_)
        this_.GetComponent<SpriteRenderer>().enabled=false;
    }

    private GameObject this_; 
    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    public Material playerMaterial {get; set;}
    public bool isDead {get; set;}

}
