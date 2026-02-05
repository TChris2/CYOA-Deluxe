using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Functionality for the pause menu
public class PauseMenuFunctions : MonoBehaviour
{
    [SerializeField]
    private Button RetryBtn;
    [SerializeField]
    private Button RetryStartBtn;
    [SerializeField]
    private Button MapBtn;
    [SerializeField]
    private Button AchieveBtn;
    [SerializeField]
    private Button SettingsBtn;
    [SerializeField]
    private Button TileScreenBtn;
    public CanvasGroup settingsMenu;
    

    void Start()
    {
        // Gets comps
        MapMenuFunctions mapMenuF = FindAnyObjectByType<MapMenuFunctions>();
        AchieveMenuFunctions achieveMenuF = FindAnyObjectByType<AchieveMenuFunctions>();
        InputMenu iMenu = FindAnyObjectByType<InputMenu>();

        // Adds functions to the buttons
        // Back to Title Screen
        TileScreenBtn.onClick.AddListener(() => { SceneManager.LoadScene("Title Screen"); 
            // Resets vars when returning to title screen
            Time.timeScale = 1; AudioListener.pause = false; iMenu.isPaused = false;});
        // Retry options
        RetryBtn.onClick.AddListener(() => { iMenu.Resume(); iMenu.gm.LoadPrevChoice(); });
        RetryStartBtn.onClick.AddListener(() => mapMenuF.LoadChoiceMap("Retry_", false));
        // Achievement menu
        AchieveBtn.onClick.AddListener(() => achieveMenuF.OpenAchieveMenu());
        // Map menu
        MapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu());
        // Settings
        SettingsBtn.onClick.AddListener(() => iMenu.MenuOpenClose(settingsMenu, !settingsMenu.interactable));
    }
}
