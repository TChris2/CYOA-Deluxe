using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Settings functions
public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;
    // Volume slider
    [SerializeField]
    private Slider volSlider;
    // Fullscreen toggle
    [SerializeField]
    private Toggle fsToggle;
    
    void Start()
    {
        // Loads the previous values
        volSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        SetVol();
        fsToggle.isOn = PlayerPrefs.GetFloat("Full Screen", 0) != 0;
        ToggleFullscreen(true);
    }

    // Updates the volume
    public void SetVol()
    {
        mixer.SetFloat("Master Volume", Mathf.Log10(volSlider.value) * 20);
    }

    // Toggles fullscreen
    public void ToggleFullscreen(bool isStart)
    {
        int width;
        int height;

        // When fullscreen is enabled
        if (fsToggle.isOn)
        {
            // Avoids updating the screen size when the function is loaded in the Start() function
            if (!isStart)
            {
                PlayerPrefs.SetInt("Window Size X", Screen.width);
                PlayerPrefs.SetInt("Window Size Y", Screen.height);
            }

            width = Display.main.systemWidth;
            height = Display.main.systemHeight;
        }
        // When fullscreen is disabled
        else
        {
            // Sets previous screen size
            width = PlayerPrefs.GetInt("Window Size X", Screen.width);
            height = PlayerPrefs.GetInt("Window Size Y", Screen.height);
        }

        Screen.SetResolution(width, height, fsToggle.isOn);
    }

    // Saves setting values
    private void OnDisable()
    {
        // Saves prefs
        PlayerPrefs.SetFloat("Volume", volSlider.value);
        PlayerPrefs.SetFloat("Full Screen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
