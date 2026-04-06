using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Functionality for the pause menu
public class PauseMenu : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField]
    private Button titleScreenBtn;
    [SerializeField]
    private Button retryBtn;
    [SerializeField]
    private Button retryStartBtn;
    [Header("Right Side")]
    [SerializeField]
    private Button statsBtn;
    [SerializeField]
    private Button mapBtn;
    [SerializeField]
    private Button achieveBtn;
    [SerializeField]
    private Button settingsBtn;

    void Start()
    {
        // Gets comps
        MapMenu mapMenuF = FindAnyObjectByType<MapMenu>();
        AchievementMenu achieveMenuF = FindAnyObjectByType<AchievementMenu>();
        InputMenu iMenu = FindAnyObjectByType<InputMenu>();
        StatsMenu statsMenuF = FindAnyObjectByType<StatsMenu>();
        SettingsMenu settingsMenuF = FindAnyObjectByType<SettingsMenu>();

        // Adds functions to the buttons
        // Back to Title Screen
        titleScreenBtn.onClick.AddListener(() => 
        { 
            SceneManager.LoadScene("Title Screen"); 
            // Resets vars when returning to title screen
            Time.timeScale = 1; AudioListener.pause = false; iMenu.isPaused = false;
            // Resets Skip Intro's value
            PlayerPrefs.SetInt("Skip Intro", 1); PlayerPrefs.Save();
        });
        // Retry options
        retryBtn.onClick.AddListener(() => { iMenu.Resume(); iMenu.gm.LoadPrevChoice(); });
        retryStartBtn.onClick.AddListener(() => mapMenuF.LoadChoiceMap("Retry_", false));
        // Map menu
        mapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu());
        statsBtn.onClick.AddListener(() => statsMenuF.OpenStatsMenu());
        // Achievement menu
        achieveBtn.onClick.AddListener(() => achieveMenuF.OpenAchieveMenu());
        // Settings
        settingsBtn.onClick.AddListener(() => settingsMenuF.OpenSettingsMenu());
    }
}
