using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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
    private MapMenuFunctions mapMenuF;
    CanvasGroup pauseMenu;

    void Start()
    {
        pauseMenu = GetComponent<CanvasGroup>();
        mapMenuF = GameObject.Find("Map Menu").GetComponent<MapMenuFunctions>();
        ButtonManager bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();

        // Adds functions to the buttons
        RetryBtn.onClick.AddListener(() => { bm.iMenu.Resume(); bm.LoadPrevChoice(); });
        RetryStartBtn.onClick.AddListener(() => mapMenuF.LoadChoiceMap("Retry_", false));
        MapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu(pauseMenu));
    }
}
