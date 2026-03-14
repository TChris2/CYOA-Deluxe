using System.Collections;
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
    [HideInInspector]
    public Animator pMenuAni;
    // Scripts
    [HideInInspector]
    public GameManager gm;
    [HideInInspector]
    public PauseMenuFunctions pMenuF;
    // Override to access everything in a menu without already completing it
    public bool completeOverride;
    [Header("Menu Storage")]
    public List<CanvasGroup> openMenus;

    // Enables or disables the Debug Menu
    public void DebugMenu()
    {
        string state = completeOverride ? "Enabling" : "Disabling";
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
            // Debug.Log(isPaused);

            // Pauses the game and opens the pause menu
            if (!isPaused && (gm.canBePaused && !gm.isFinale || completeOverride))
                Pause();
            // Resumes the game
            else if (isPaused)
                Resume();
        }
        else
        {
            if (openMenus.Count > 1)
                CloseMenu();
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
        openMenus.Add(pMenu);
        MenuOpenClose(pMenu, true);
    }

    // Resumes the game if the player is not in a sub menu
    public void Resume()
    {
        // Debug.Log("Resume()");
        // Closes the menu player is in currently
        if (openMenus.Count > 1)
            CloseMenu();
        // Resumes the game
        else
        {
            // Debug.Log("Resuming game");

            // If settings menu is open at the time
            if (pMenuF.settingsMenu.interactable)
            {
                // Debug.Log("Closing Settings Menu");
                MenuOpenClose(pMenuF.settingsMenu, false);
            }

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
    }

    // Closes the menu player is in currently
    public void CloseMenu()
    {
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
        pMenuAni.Play("Menu Fade In", 0, 0);
        openMenus.Add(pMenu);
        MenuOpenClose(pMenu, true);
    }

    // Opens or closes selected menus
    public void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        menu.interactable = isOpen;
        menu.alpha = isOpen ? 1 : 0;
        menu.blocksRaycasts = isOpen;
    }
}
