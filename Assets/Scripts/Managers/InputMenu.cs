using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages menu inputs
/// </summary>
public class InputMenu : MonoBehaviour
{
    // Canvasgroups
    [HideInInspector]
    public CanvasGroup pMenu;
    [HideInInspector]
    public Animator pMenuAni;
    [HideInInspector]
    public CanvasGroup dMenu;
    // Tracks when the game is paused
    public bool isPaused;
    // Tracks when the retry menu is open
    public bool isRetryMenu;
    // Scripts
    MapMenu mapMenuF;
    [HideInInspector]
    public GameManager gm;
    [HideInInspector]
    public FinaleManager fm;
    [HideInInspector]
    public PauseMenu pMenuF;
    // Override to access everything in a menu without already completing it
    public bool completeOverride;
    [Header("Menu Storage")]
    public List<CanvasGroup> openMenus;
    public CanvasGroup smallMenu;    

    void Start()
    {
        mapMenuF = FindAnyObjectByType<MapMenu>();
    }

    /// <summary>
    /// Handles the Debug menu
    /// </summary>
    public void DebugMenu()
    {
        // string state = completeOverride ? "Enabling" : "Disabling";
        // Debug.Log($"InputMenu: {state} Debug Menu {completeOverride}");

        if (completeOverride)
        {
            MenuOpenClose(dMenu, true);
            if (SceneManager.GetActiveScene().name == "Main Game")
                StartCoroutine(gm.GetVidTime());
        }
        else
        {
            MenuOpenClose(dMenu, false);
        }
    }

    /// <summary>
    /// Handles the Pause menu
    /// </summary>
    public void PauseMenu()
    {
        // Opens the pause menu if the retry menu is not already open
        if (!isRetryMenu)
        {
            isPaused = pMenu.alpha == 1 ? true : false;
            // Debug.Log($"InputMenu: {isPaused} {gm.canBePaused} {!gm.isFinale} {completeOverride}");

            // Pauses the game and opens the pause menu
            if (!isPaused && (gm.canBePaused && !fm.isFinale || completeOverride))
                Pause();
            // Resumes the game
            else if (isPaused)
                ResumeCheck();
        }
        else
        {
            if (openMenus.Count > 1 || smallMenu != null)
                CloseMenu();
        }
    }

    /// <summary>
    /// Pauses the game and opens the pause menu
    /// </summary>
    private void Pause()
    {
        gm.fadeTextAni.Play("Invisible Text");
        
        // Debug.Log("InputMenu: Pausing game");
        isPaused = true;
        Time.timeScale = 0;
        AudioListener.pause = true;

        // Opens pause menu
        openMenus.Add(pMenu);
        MenuOpenClose(pMenu, true);
    }

    /// <summary>
    /// Checks to see if the game can be resumed
    /// </summary>
    public void ResumeCheck()
    {
        // Debug.Log("InputMenu: Resume Check");
        // Closes the menu player is in currently
        if (openMenus.Count > 1 || smallMenu != null)
            CloseMenu();
        // Resumes the game
        else
        {
            Resume();
        }
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void Resume()
    {
        // Debug.Log("InputMenu: Resuming game");

        // Closes remaining menus
        CloseAllMenus();

        Time.timeScale = 1;
        AudioListener.pause = false;
        isPaused = false;
    }

    /// <summary>
    /// Close every open menu
    /// </summary>
    public void CloseAllMenus()
    {
        // Debug.Log("InputMenu: Closing all menus");
        CloseSmallMenu();

        int menuTotal;

        if (openMenus.Contains(mapMenuF.mapMenu))
        {
            menuTotal = openMenus.Count - 1;
        }
        else
        {
            menuTotal = openMenus.Count;
        }

        if (isRetryMenu)
        {
            Debug.Log("Closing Retry Menu");
            pMenuAni.enabled = true;
            pMenuAni.Play($"Close Retry Menu");
            openMenus.Remove(pMenu);
            menuTotal -= 1;
        }

        for (int i = menuTotal - 1; i >= 0; i--)
        {
            // Closes menu
            MenuOpenClose(openMenus[i], false);
            // Removes menu from list
            openMenus.RemoveAt(i);
        }
    }

    /// <summary>
    /// Closes the menu player is in currently
    /// </summary>
    public void CloseMenu()
    {
        if (smallMenu != null)
        {
            CloseSmallMenu();
            return;
        }

        if (openMenus.Count == 0 || SceneManager.GetActiveScene().name == "Title Screen" && openMenus.Count == 1)
        {
            // Debug.Log($"InputMenu: Closing menus in {SceneManager.GetActiveScene().name} is currently unnecessary");
            return;
        }

        // Debug.Log($"InputMenu: Closing {openMenus[openMenus.Count - 1].name}");

        // Renables previous menu
        if (openMenus.Count >= 2)
            openMenus[openMenus.Count - 2].interactable = true;
        // Closes current menu
        MenuOpenClose(openMenus[openMenus.Count - 1], false);
        // Removes menu from list
        openMenus.RemoveAt(openMenus.Count - 1);
    }

    /// <summary>
    /// Opens the retry menu
    /// </summary>
    public void OpenRetryMenu()
    {
        isRetryMenu = true;
        pMenuAni.enabled = true;
        pMenuAni.Play("Open Retry Menu", 0, 0);
        openMenus.Add(pMenu);
    }

    /// <summary>
    /// Opens or closes selected menus
    /// </summary>
    public void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        // If the menu has an animator
        if (menu.GetComponent<Animator>())
        {
            string animation = isOpen ? "Open" : "Close";
            
            if (menu.GetComponent<Animator>().HasState(0, Animator.StringToHash($"{animation} Menu")))
            {
                // Debug.Log($"InputMenu: Playing {animation} Menu Animation");
                menu.GetComponent<Animator>().enabled = true;
                menu.GetComponent<Animator>().Play($"{animation} Menu");
            }
        }
        else
        {
            menu.interactable = isOpen;
            menu.alpha = isOpen ? 1 : 0;
            menu.blocksRaycasts = isOpen;
        }
    }

    /// <summary>
    /// Closes open small menus
    /// </summary>
    public void CloseSmallMenu()
    {
        if (smallMenu == null)
            return;
        if (smallMenu.interactable)
        {
            // Debug.Log("InputMenu: Closing Small Menu");
            smallMenu.transform.SetAsFirstSibling();
            MenuOpenClose(smallMenu, false);
            smallMenu = null;
        }
    }

    /// <summary>
    /// Opens or closes small menus
    /// </summary>
    public void SmallMenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        // Closes the active small menu
        CloseSmallMenu();

        // Opens next the small menu
        if (isOpen)
        {
            smallMenu = menu;
            MenuOpenClose(menu, isOpen);
        }
    }
        
    /// <summary>
    /// Opens a regular menu
    /// </summary>
    public void OpenRegularMenu(CanvasGroup menu)
    {
        Animator menuAni = openMenus[openMenus.Count - 1].GetComponent<Animator>();

        // Exits opening a new menu if the previous menu is still playing an animation
        if (menuAni && menuAni.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            return;

        // Adds menu to menu list
        openMenus.Add(menu);

        // Disables previous menu
        if (menuAni)
            menuAni.enabled = false;
        openMenus[openMenus.Count - 2].interactable = false;

        // Checks if there are any open small menus
        CloseSmallMenu();

        // Opens menu
        MenuOpenClose(openMenus[openMenus.Count - 1], true);
    }

    /// <summary>
    /// Closes retry menu
    /// </summary>
    public void CloseRetryMenu()
    {
        if (isRetryMenu)
        {
            isRetryMenu = false;
            Resume();
        }
    }
}
