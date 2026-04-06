using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Settings menu functions
public class SettingsMenu : MonoBehaviour
{
    private CanvasGroup settingsMenu;
    [SerializeField]
    private AudioMixer mixer;
    // Volume slider
    [SerializeField]
    private Slider volSlider;
    // Fullscreen toggle
    [SerializeField]
    private Toggle fsToggle;
    [SerializeField]
    private TMP_Dropdown resDropdown;
    // Scripts
    InputMenu iMenu;
    
    void Start()
    {
        // Loads the previous values
        volSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        SetVol();
        fsToggle.isOn = PlayerPrefs.GetInt("Full Screen", 0) != 0;
        resDropdown.value = PlayerPrefs.GetInt("Resolution Option", 1);
        ToggleFullscreen();

        settingsMenu = GetComponent<CanvasGroup>();
        StartCoroutine(GetComponents());
    }

    IEnumerator GetComponents()
    {
        yield return null;

        iMenu = FindAnyObjectByType<InputMenu>();
    }

    // Updates the volume
    public void SetVol()
    {
        mixer.SetFloat("Master Volume", volSlider.value);
    }

    // Toggles fullscreen
    public void ToggleFullscreen()
    {
        // When fullscreen is enabled
        if (fsToggle.isOn)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
            resDropdown.interactable = false;
        }
        // Sets game resolution
        else
        {
            SetResolution();
            resDropdown.interactable = true;
        }
    }

    // Sets game resolution
    public void SetResolution()
    {
        int width;
        int height;

        switch (resDropdown.value)
        {
            case 0:
                width = 854;
                height = 480;
                break;
            default:
            case 1:
                width = 1280;
                height = 720;
                break;
            case 2:
                width = 1600;
                height = 900;
                break;
            case 3:
                width = 1920;
                height = 1080;
                break;
        }

        Screen.SetResolution(width, height, fsToggle.isOn);
    }

    // Saves setting values
    private void OnDisable()
    {
        // Saves prefs
        PlayerPrefs.SetFloat("Volume", volSlider.value);
        PlayerPrefs.SetInt("Full Screen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("Resolution Option", resDropdown.value);
        PlayerPrefs.Save();
    }

    // Open settings menu
    public void OpenSettingsMenu()
    {
        iMenu.SmallMenuOpenClose(settingsMenu, !settingsMenu.interactable);
    }
}
