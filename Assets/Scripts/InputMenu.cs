using UnityEngine;

public class InputMenu : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup pMenu;
    [SerializeField]
    private CanvasGroup dMenu;
    public bool isPaused;
    MapMenuFunctions mMenuF;
    [SerializeField]
    private Animator pMenuAni;
    public bool isRetryMenu;

    void Start()
    {
        mMenuF = GameObject.Find("Map Menu").GetComponent<MapMenuFunctions>();
    }

    // Opens Debug Menu
    public void DebugMenu()
    {
        // Debug.Log("Debug menu");

        if (!dMenu.interactable)
        {
            MenuOpenClose(dMenu, true);
            mMenuF.completeOverride = true;
        }
        else
        {
            MenuOpenClose(dMenu, false);
            mMenuF.completeOverride = false;
        }
    }

    // Opens Pause Menu
    public void PauseMenu()
    {
        if (!isRetryMenu)
        {
            isPaused = pMenu.alpha == 1 ? true : false;
            // Debug.Log(isPaused);

            if (!isPaused)
                Pause();
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
        // Closes additional menus player is in currently
        if (!pMenu.interactable)
            CloseMenus();
        // Resumes the game
        else
        {
            // Debug.Log("Resuming game");
            // Closes the pause screen
            MenuOpenClose(pMenu, false);

            Time.timeScale = 1;
            AudioListener.pause = false;
            isPaused = false;
        }
    }

    // Closes additionals menu player is in currently
    void CloseMenus()
    {
        if (mMenuF.mapMenu.interactable)
        {
            // Debug.Log("Closing Map Menu");
            mMenuF.CloseMapMenu();
        }

        /*
        // Closes sub type menu of sub option menu
        if (subOpMenuBtns.isSubTypeMenuOpen)
        {
            // Debug.Log("Closing Sub Type Menu");
            subOpMenuBtns.CloseSubTypeMenu();
        }

        // Closes sub option menu yes no screen
        else if (subOpMenuBtns.ynScreen.interactable)
        {
            // Debug.Log("Closing Sub Option Menu Yes No Screen");
            subOpMenuBtns.CloseSubOpMenu();
        }
        */
    }

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
