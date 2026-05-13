using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Functionality for the Pause menu
/// </summary>
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
        GameManager gm = FindAnyObjectByType<GameManager>();
        TransitionManager tm = FindAnyObjectByType<TransitionManager>();

        // Adds functions to the buttons
        // Back to Title Screen
        titleScreenBtn.onClick.AddListener(() => 
        { 
            // Skips Intro
            PlayerPrefs.SetInt("Skip Intro", 1); PlayerPrefs.Save();
            tm.fadeDuration = .8f;
            tm.onTransition += () => 
            {
                // Resets vars when returning to title screen
                Time.timeScale = 1; AudioListener.pause = false; iMenu.isPaused = false;
                tm.ChangeScene("Title Screen");
            };
            tm.FadeOut(FadeType.PlainBlack);
        });
        // Retry options
        retryBtn.onClick.AddListener(() => { iMenu.Resume(); gm.LoadPrevChoice(); });
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
