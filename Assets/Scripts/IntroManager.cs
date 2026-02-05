using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class IntroManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlay;
    // Skips to title screen
    [SerializeField]
    private Animator fadeTextAni;
    TMP_Text skipText;
    [SerializeField]
    private CanvasGroup intro;
    [SerializeField]
    private CanvasGroup titleScreen;
    [Header("Title Screen Buttons")]
    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private Button mapBtn;
    [SerializeField]
    private Button achieveBtn;
    [Header("Delete Save Menu")]
    [SerializeField]
    private CanvasGroup deleteSaveMenu;
    [SerializeField]
    private Button openDeleteSaveBtn;
    [SerializeField]
    private Button exitDeleteSaveBtn;
    [SerializeField]
    private Button deleteSaveBtn;
    // Scripts
    InputMenu iMenu;

    void Start()
    {
        // Enables the intro screen
        intro.alpha = 1;
        intro.blocksRaycasts = true;

        skipText = fadeTextAni.GetComponent<TMP_Text>();
        videoPlay.loopPointReached += TitleScreen;

        // Waits a frame before adding button functions to prevent getting components from the global objects which will be deleted
        StartCoroutine(AddBtnFunctions());
    }

    // Adds functions to title screen buttons
    IEnumerator AddBtnFunctions()
    {
        yield return null;

        MapMenuFunctions mapMenuF = FindAnyObjectByType<MapMenuFunctions>();
        AchieveMenuFunctions achieveMenuF = FindAnyObjectByType<AchieveMenuFunctions>();
        SaveManager sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();
        
        playBtn.onClick.AddListener(() => mapMenuF.StartGame());
        mapBtn.onClick.AddListener(() => mapMenuF.OpenMapMenu());
        achieveBtn.onClick.AddListener(() => achieveMenuF.OpenAchieveMenu());
        openDeleteSaveBtn.onClick.AddListener(() => OpenDeleteSaveMenu());
        exitDeleteSaveBtn.onClick.AddListener(() => iMenu.CloseMenu());
        deleteSaveBtn.onClick.AddListener(() => { sm.LoadSOData(); iMenu.CloseMenu(); });
    }

    // Opens delete save menu
    void OpenDeleteSaveMenu()
    {
        iMenu.openMenus.Add(deleteSaveMenu);

        // Disables previous menu
        iMenu.openMenus[iMenu.openMenus.Count - 2].interactable = false;

        // Opens achievement menu
        iMenu.MenuOpenClose(iMenu.openMenus[iMenu.openMenus.Count - 1], true);
    }

    // Skips the intro
    public void Skip()
    {
        // Debug.Log("Skip()");
        // Debug.Log($"videoPlay.isPlaying {videoPlay.isPlaying}");
        if (videoPlay.isPlaying)
        {
            // If the skip text is visable on screen
            if (skipText.color.a == 0)
            {
                // Debug.Log($"Skip - Text Popup");
                fadeTextAni.Play("Fade In");
            }
            // Skips if the player presses the skip button while the text is onscreen
            else
            {
                // Debug.Log($"Skip - SkipVidTime");
                SkipVidTime(9999);
            }
        }
    }

    // Pops up title screen after playing intro
    void TitleScreen(VideoPlayer vp)
    {
        StartCoroutine(PopUpTitleScreen());
    }

    IEnumerator PopUpTitleScreen()
    {
        yield return new WaitForSeconds(.1f);

        intro.alpha = 0;
        intro.blocksRaycasts = false;
    }

    // Skips to the selected timestamp in the vid
    void SkipVidTime(float timestamp)
    {
        // Debug.Log("Skipping time in vid");

        // Disables text
        fadeTextAni.Play("Invisible Text");
        // Sets time in the vid
        videoPlay.time = timestamp;
    }
}
