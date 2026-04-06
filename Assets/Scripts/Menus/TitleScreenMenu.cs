using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;

public class TitleScreenMenu : MonoBehaviour
{
    [Header("Main List Buttons")]
    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private Button mapBtn;
    [SerializeField]
    private Button achieveBtn;
    [Header("Sidebar Buttons")]
    [SerializeField]
    private Button creditsBtn;
    [SerializeField]
    private Button statsBtn;
    [SerializeField]
    private Button deleteSaveBtn;
    [SerializeField]
    private Button settingsBtn;
    [Header("Confirm Menu")]
    [SerializeField]
    private CanvasGroup confirmMenu;
    [SerializeField]
    private TMP_Text confirmText;
    [SerializeField]
    private Button yesBtn;
    [SerializeField]
    private Button noBtn;
    event Action yesAction;
    [Header("Credits Menu")]
    [SerializeField]
    private CanvasGroup creditsMenu;
    [SerializeField]
    private CanvasGroup[] creditPages;
    [SerializeField]
    private Button[] creditPageBtns;
    // Scripts
    InputMenu iMenu;
    SaveManager sm;
    
    void Start()
    {
        // Waits a frame before adding button functions to prevent getting components from the global objects which will be deleted
        StartCoroutine(AddBtnFunctions());

        yesAction?.Invoke();
    }

    // Adds functions to title screen buttons
    IEnumerator AddBtnFunctions()
    {
        yield return null;

        MapMenu mapMenuF = FindAnyObjectByType<MapMenu>();
        AchievementMenu achieveMenuF = FindAnyObjectByType<AchievementMenu>();
        StatsMenu statsMenuF = FindAnyObjectByType<StatsMenu>();
        SettingsMenu settingsMenuF = FindAnyObjectByType<SettingsMenu>();
        sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();
        
        // Main List
        playBtn.onClick.AddListener(() => mapMenuF.StartGame());
        mapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu());
        achieveBtn.onClick.AddListener(() => achieveMenuF.OpenAchieveMenu());
        // Side Bar
        creditsBtn.onClick.AddListener(() => OpenCreditsMenu());
        statsBtn.onClick.AddListener(() => statsMenuF.OpenStatsMenu());
        deleteSaveBtn.onClick.AddListener(() => { OpenConfirmMenu(confirmMenu, "Delete Save Data?"); 
            yesAction += () => { sm.LoadSOData(); iMenu.CloseMenu(); }; });
        settingsBtn.onClick.AddListener(() => settingsMenuF.OpenSettingsMenu());
        // Confirm Menu
        yesBtn.onClick.AddListener(() => yesAction?.Invoke());
        noBtn.onClick.AddListener(() => { yesAction = null; iMenu.CloseMenu(); });
        // Credits Menu
        creditPageBtns[0].onClick.AddListener(() => { CreditsPageOpenClose(1, false); CreditsPageOpenClose(0, true); });
        creditPageBtns[1].onClick.AddListener(() => { CreditsPageOpenClose(0, false); CreditsPageOpenClose(1, true); });
    }

    // Open credits menu
    void OpenCreditsMenu()
    {
        if (!creditsMenu.interactable)
        {
            CreditsPageOpenClose(0, false);
            CreditsPageOpenClose(1, true);
        }

        iMenu.SmallMenuOpenClose(creditsMenu, !creditsMenu.interactable);
    }

    // Opens a credit page in the credits menu
    void CreditsPageOpenClose(int pageNum, bool isOpen)
    {
        CanvasGroup page = pageNum == 0 ? creditPages[0] : creditPages[1];
        Button btn = pageNum == 0 ? creditPageBtns[1] : creditPageBtns[0];

        btn.interactable = isOpen;
        iMenu.MenuOpenClose(page, isOpen);
    }

    // Opens confirm menu
    void OpenConfirmMenu(CanvasGroup menu, string text)
    {
        iMenu.SmallMenuOpenClose(menu, !menu.interactable);

        confirmText.text = text;
    }
}
