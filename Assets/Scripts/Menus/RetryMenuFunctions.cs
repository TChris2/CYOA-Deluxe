using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Functionality for the retry menu
public class RetryMenuFunctions : MonoBehaviour
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

    void Start()
    {
        CanvasGroup pauseMenu = GetComponent<CanvasGroup>();
        MapMenuFunctions mapMenuF = GameObject.Find("Map Menu").GetComponent<MapMenuFunctions>();
        ButtonManager bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();
        InputMenu iMenu = bm.GetComponent<InputMenu>();
        iMenu.isRetryMenu = true;


        // Adds functions to the buttons
        RetryBtn.onClick.AddListener(() => bm.LoadPrevChoice());
        RetryStartBtn.onClick.AddListener(() => mapMenuF.LoadChoiceMap("Retry_", false));
        MapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu(pauseMenu));
    }
}
