using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu input functions
public class InputMenu : MonoBehaviour
{
    // Canvasgroups
    [HideInInspector]
    public CanvasGroup pMenu;
    [HideInInspector]
    public CanvasGroup dMenu;
    // Tracks when the game is paused
    public bool isPaused;
    // Tracks when the retry menu is open
    public bool isRetryMenu;
    // Scripts
    [HideInInspector]
    public GameManager gm;
    [HideInInspector]
    public PauseMenu pMenuF;
    // Override to access everything in a menu without already completing it
    public bool completeOverride;
    [Header("Menu Storage")]
    public List<CanvasGroup> openMenus;
    public CanvasGroup smallMenu;    

    // Enables or disables the Debug Menu
    public void DebugMenu()
    {
        // string state = completeOverride ? "Enabling" : "Disabling";
        // Debug.Log($"{state} Debug Menu {completeOverride}");

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

    // Opens Pause Menu
    public void PauseMenu()
    {
        // Opens the pause menu if the retry menu is not already open
        if (!isRetryMenu)
        {
            isPaused = pMenu.alpha == 1 ? true : false;
            // Debug.Log($"{isPaused} {gm.canBePaused} {!gm.isFinale} {completeOverride}");

            // Pauses the game and opens the pause menu
            if (!isPaused && (gm.canBePaused && !gm.isFinale || completeOverride))
                Pause();
            // Resumes the game
            else if (isPaused)
                Resume();
        }
        else
        {
            if (openMenus.Count > 1 || smallMenu != null)
                CloseMenu();
        }
    }

    // Pauses the game
    private void Pause()
    {
        gm.fadeTextAni.Play("Invisible Text");
        
        // Debug.Log("Pausing game");
        isPaused = true;
        Time.timeScale = 0;
        AudioListener.pause = true;

        // Opens pause menu
        openMenus.Add(pMenu);
        MenuOpenClose(pMenu, true);
    }

    // Resumes the game if the player is not in a sub menu
    public void Resume()
    {
        // Debug.Log("Resume()");
        // Closes the menu player is in currently
        if (openMenus.Count > 1 || smallMenu != null)
            CloseMenu();
        // Resumes the game
        else
        {
            // Debug.Log("Resuming game");

            // Closes the pause screen
            MenuOpenClose(openMenus[openMenus.Count - 1], false);
            // Removes menu from list
            openMenus.RemoveAt(openMenus.Count - 1);

            Time.timeScale = 1;
            AudioListener.pause = false;
            isPaused = false;
        }
    }

    // Close every open menu
    public void CloseAllMenus()
    {
        // Debug.Log("Closing all menus");
        int menuTotal;

        if (SceneManager.GetActiveScene().name == "Title Screen")
        {
            menuTotal = openMenus.Count - 1;
        }
        else
        {
            menuTotal = openMenus.Count;
        }

        for (int i = 0; i < menuTotal; i++)
        {
            // Closes menu
            MenuOpenClose(openMenus[openMenus.Count - 1], false);
            // Removes menu from list
            openMenus.RemoveAt(openMenus.Count - 1);
        }

        if (openMenus.Count == 1)
        {
            openMenus[0].interactable = true;
        }

        CloseSmallMenu();
    }

    // Closes the menu player is in currently
    public void CloseMenu()
    {
        if (smallMenu != null)
        {
            CloseSmallMenu();
            return;
        }

        if (openMenus.Count == 0 || SceneManager.GetActiveScene().name == "Title Screen" && openMenus.Count == 1)
        {
            // Debug.Log($"Closing menus in {SceneManager.GetActiveScene().name} is currently unnecessary");
            return;
        }

        // Debug.Log($"Closing {openMenus[openMenus.Count - 1].name}");

        // Renables previous menu
        openMenus[openMenus.Count - 2].interactable = true;
        // Closes current menu
        MenuOpenClose(openMenus[openMenus.Count - 1], false);
        // Removes menu from list
        openMenus.RemoveAt(openMenus.Count - 1);
    }

    // Opens the retry menu
    public void OpenRetryMenu()
    {
        isRetryMenu = true;
        pMenu.GetComponent<Animator>().Play("Open Retry Menu", 0, 0);
        openMenus.Add(pMenu);
    }

    // Opens or closes selected menus
    public void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        if (menu.GetComponent<Animator>())
        {
            string animation = isOpen ? "Open" : "Close";
            
            if (menu.GetComponent<Animator>().HasState(0, Animator.StringToHash($"{animation} Menu")))
            {
                // Debug.Log($"Playing {animation} Menu Animation");
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

    // Closes any open small menus
    public void CloseSmallMenu()
    {
        if (smallMenu == null)
            return;
        if (smallMenu.interactable)
        {
            // Debug.Log("Closing Small Menu");
            smallMenu.transform.SetAsFirstSibling();
            MenuOpenClose(smallMenu, false);
            smallMenu = null;
        }
    }

    // Opens or closes small menus
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
        
    public void OpenRegularMenu(CanvasGroup menu)
    {
        // Adds menu to menu list
        openMenus.Add(menu);

        // Disables previous menu
        openMenus[openMenus.Count - 2].GetComponent<Animator>().enabled = false;
        openMenus[openMenus.Count - 2].interactable = false;

        // Checks if there are any open small menus
        CloseSmallMenu();

        // Opens menu
        MenuOpenClose(openMenus[openMenus.Count - 1], true);
    }
}
