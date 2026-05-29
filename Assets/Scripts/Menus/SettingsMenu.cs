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
    // Quit game button
    [SerializeField]
    private Button quitBtn;
    // Volume slider
    [SerializeField]
    private Slider volSlider;
    // Subtitles toggle
    [SerializeField]
    private Toggle subtitlesToggle;
    // Fullscreen toggle
    [SerializeField]
    private Toggle fsToggle;
    // Resolution Dropdown
    [SerializeField]
    private TMP_Dropdown resDropdown;
    // Scripts
    InputMenu iMenu;
    SubtitlesManager subtitlesManager;
    VoidCover vCover;
    Camera cam;
    CanvasScaler[] canvasScalers;
    private Vector2 lastScreenSize;
    
    void Start()
    {
        cam = Camera.main;
        canvasScalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        subtitlesManager = FindAnyObjectByType<SubtitlesManager>();
        vCover = FindAnyObjectByType<VoidCover>();

        quitBtn.onClick.AddListener(() => Application.Quit());
        // Loads the previous values
        volSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        SetVol();
        fsToggle.isOn = PlayerPrefs.GetInt("Full Screen", 0) != 0;
        resDropdown.value = PlayerPrefs.GetInt("Resolution Option", 1);
        ToggleFullscreen();
        subtitlesToggle.isOn = PlayerPrefs.GetInt("Subtitles", 1) == 1;
        ToggleSubtitles();

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
        mixer.SetFloat("Master Volume", Mathf.Log10(volSlider.value) * 20);
    }

    // Toggles subtitles
    public void ToggleSubtitles()
    {
        subtitlesManager.subtitlesEnabled = subtitlesToggle.isOn;
    }

    // Toggles fullscreen
    public void ToggleFullscreen()
    {
        // When fullscreen is enabled
        if (fsToggle.isOn)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
            resDropdown.interactable = false;
            vCover.EnableVoidCover(true);
            StartCoroutine(Adjust());
        }
        // Sets game resolution
        else
        {
            SetResolution();
            vCover.EnableVoidCover(false);
            UpdateScalers(.5f);
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

        Screen.SetResolution(width, height, false);
    }

    // Open settings menu
    public void OpenSettingsMenu()
    {
        iMenu.SmallMenuOpenClose(settingsMenu, !settingsMenu.interactable);
    }

    // Updates aspect ratio when changing window size - Script from Max O'Didily 
    private IEnumerator Adjust()
    {
        Vector2 current;

        while (fsToggle.isOn)
        {
            yield return null;

            current = new Vector2(Screen.width, Screen.height);
            // Skips doing the logic if the screen size has not changed
            if (current == lastScreenSize)
                continue;

            // Target aspect ratio
            float targetaspect = 16f / 9f;
            // Gets current screen size
            float windowaspect = (float)Screen.width / (float)Screen.height;
            // Gets the scale height
            float scaleheight = windowaspect / targetaspect;
            float scale;

            // If screen is taller than target aspect ratio
            if (scaleheight < 1f)
            {
                scale = 0;
                Rect rect = cam.rect;

                rect.width = 1f;
                rect.height = scaleheight;
                rect.x = 0f;
                rect.y = (1f - scaleheight) / 2f;

                cam.rect = rect;
            }
            // If screen is wider than target aspect ratio
            else
            {
                scale = 1;
                float scalewidth = 1f / scaleheight;

                Rect rect = cam.rect;

                rect.width = scalewidth;
                rect.height = 1f;
                rect.x = (1f - scalewidth) / 2f;
                rect.y = 0f;

                cam.rect = rect;
            }

            UpdateScalers(scale);
        }
    }

    // Updates scales to match current scale
    void UpdateScalers(float scale)
    {
        foreach (CanvasScaler canvasScaler in canvasScalers)
            canvasScaler.matchWidthOrHeight = scale;
    }

    // Saves setting values
    private void OnDisable()
    {
        // Saves prefs
        PlayerPrefs.SetFloat("Volume", volSlider.value);
        PlayerPrefs.SetInt("Full Screen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("Resolution Option", resDropdown.value);
        PlayerPrefs.SetInt("Subtitles", subtitlesToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
