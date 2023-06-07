using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    //SETTINGS MENU
    public AudioMixer mainMixer;
    public float defaultVolume;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        if(volumeSlider){
            volumeSlider.value = PlayerPrefs.GetFloat("volume");
        }    
        
        if(fullscreenToggle){
            fullscreenToggle.isOn = Screen.fullScreen;
        }     
    }

    void Update()
    {
        mainMixer.SetFloat("volume", defaultVolume);
        PlayerPrefs.SetFloat("volume", defaultVolume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen; 
    }

    public void SetVolume(float volume)
    {
        defaultVolume = volume;
    }
}
