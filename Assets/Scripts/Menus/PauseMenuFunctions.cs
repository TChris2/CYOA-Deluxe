using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    CanvasGroup pauseMenu;
    public CanvasGroup settingsMenu;
    

    void Start()
    {
        // Gets comps
        pauseMenu = GetComponent<CanvasGroup>();
        MapMenuFunctions mapMenuF = FindAnyObjectByType<MapMenuFunctions>();
        AchieveMenuFunctions achieveMenuF = FindAnyObjectByType<AchieveMenuFunctions>();
        GameManager gm = FindAnyObjectByType<GameManager>();

        // Adds functions to the buttons
        // Retry options
        RetryBtn.onClick.AddListener(() => { gm.iMenu.Resume(); gm.LoadPrevChoice(); });
        RetryStartBtn.onClick.AddListener(() => mapMenuF.LoadChoiceMap("Retry_", false));
        // Map menu
        MapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu(pauseMenu));
        // Achievement menu
        AchieveBtn.onClick.AddListener(() => achieveMenuF.OpenAchieveMenu(pauseMenu));
        // Settings
        SettingsBtn.onClick.AddListener(() => mapMenuF.MenuOpenClose(settingsMenu, !settingsMenu.interactable));
    }
}
