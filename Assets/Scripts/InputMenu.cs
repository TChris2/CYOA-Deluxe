using UnityEngine;

// Menu input functions
public class InputMenu : MonoBehaviour
{
    // Canvasgroups
    [SerializeField]
    private CanvasGroup pMenu;
    [SerializeField]
    private CanvasGroup dMenu;
    public bool isPaused;
    public bool isRetryMenu;
    [SerializeField]
    private Animator pMenuAni;
    // Scripts
    private ButtonManager bm;
    [HideInInspector]
    public PauseMenuFunctions pMenuF;
    MapMenuFunctions mMenuF;
    AchieveMenuFunctions achieveMenuF;
    // Override to access everything in a menu without already completing it
    public bool completeOverride;

    void Start()
    {
        // Get necessary components
        pMenuF = GameObject.Find("Pause Menu").GetComponent<PauseMenuFunctions>();
        mMenuF = GameObject.Find("Map Menu").GetComponent<MapMenuFunctions>();
        achieveMenuF = GameObject.Find("Achievement Menu").GetComponent<AchieveMenuFunctions>();
        bm = GetComponent<ButtonManager>();
        // Intially sets override depending on whether the debug menu is interactable or not
        completeOverride = dMenu.interactable;
    }

    // Opens Debug Menu
    public void DebugMenu()
    {
        // Debug.Log("Debug menu");

        if (!dMenu.interactable)
        {
            MenuOpenClose(dMenu, true);
            completeOverride = true;
        }
        else
        {
            MenuOpenClose(dMenu, false);
            completeOverride = false;
        }
    }

    // Opens Pause Menu
    public void PauseMenu()
    {
        if (!isRetryMenu)
        {
            isPaused = pMenu.alpha == 1 ? true : false;
            // Debug.Log(isPaused);

            // Pauses the game and opens the pause menu
            if (!isPaused && (bm.canBePaused || completeOverride))
                Pause();
            // Resumes the game
            else
                Resume();
        }
        else
        {
            // Debug.Log("Retry menu is currently open, cannot open pause menu");
        }
    }

    // Pauses the game
    private void Pause()
    {
        // Debug.Log("Pausing game");
        isPaused = true;
        Time.timeScale = 0;
        AudioListener.pause = true;

        pMenuAni.Play("Default");

        // Opens pause menu
        MenuOpenClose(pMenu, true);
    }

    // Resumes the game if the player is not in a sub menu
    public void Resume()
    {
        // Debug.Log("Attempting to resume game");
        // Closes the menu player is in currently
        if (!pMenu.interactable)
            CloseMenu();
        // Resumes the game
        else
        {
            // Debug.Log("Resuming game");

            // If settings menu is open at the time
            if (pMenuF.settingsMenu.interactable)
            {
                // Debug.Log("Closing Settings Menu");
                mMenuF.MenuOpenClose(pMenuF.settingsMenu, false);
            }

            // Closes the pause screen
            MenuOpenClose(pMenu, false);

            Time.timeScale = 1;
            AudioListener.pause = false;
            isPaused = false;
        }
    }

    // Close every open menu
    public void CloseAllMenus()
    {
        // Debug.Log("Closing all menus");
        if (mMenuF.mapMenu.interactable)
        {
            // Debug.Log("Closing Map Menu");
            mMenuF.CloseMapMenu();
        }
        if (achieveMenuF.achieveMenu.interactable)
        {
            // Debug.Log("Closing Achievement Menu");
            achieveMenuF.CloseAchieveMenu();
        }
    }

    // Closes the menu player is in currently
    void CloseMenu()
    {
        // Debug.Log("Closing a menu");
        if (mMenuF.mapMenu.interactable)
        {
            // Debug.Log("Closing Map Menu");
            mMenuF.CloseMapMenu();
        }
        else if (achieveMenuF.achieveMenu.interactable)
        {
            // Debug.Log("Closing Achievement Menu");
            achieveMenuF.CloseAchieveMenu();
        }
    }

    // Opens the retry menu
    public void OpenRetryMenu()
    {
        isRetryMenu = true;
        pMenuAni.Play("Menu Fade In", 0, 0);
        MenuOpenClose(pMenu, true);
    }

    // Opens or closes selected menus
    private void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        menu.interactable = isOpen;
        menu.alpha = isOpen ? 1 : 0;
        menu.blocksRaycasts = isOpen;
    }
}
