using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Functionality for the achievement menu
public class AchieveMenuFunctions : MonoBehaviour
{
    // Stores the default scale of the achievement menu
    [SerializeField]
    private Transform achieveContents;
    // Canvas groups
    [HideInInspector]
    public CanvasGroup achieveMenu;
    // Stores the canvas group of the previous menu
    [HideInInspector]
    public CanvasGroup prevMenu;
    // Scripts
    SaveManager sm;
    InputMenu iMenu;

    void Start()
    {
        // Gets comps
        achieveMenu = GetComponent<CanvasGroup>();
        sm = GameObject.Find("SaveManager").GetComponent<SaveManager>();
    }

    // Updates achievement displays depending on the progress the player has made
    void UpdateAchievements()
    {
        // Gets all the achievement displays in the menu
        AchievementPopup[] achieveDisplays = achieveContents.GetComponentsInChildren<AchievementPopup>();

        // Checks each display
        foreach (AchievementPopup achieveDisplay in achieveDisplays)
        {
            // Checks to make sure display has an id that's in the system
            if (sm.achieveDict.TryGetValue(achieveDisplay.gameObject.name, out AchievementInfo achievement))
            {
                // Only updates the display when achievement needs to be updated or when the override is enabled
                if (achievement.updateDisplay || iMenu.completeOverride)
                {
                    // If the override is enabled it will display the achievement at full completion
                    if (iMenu.completeOverride)
                    {
                        achieveDisplay.DisplayInfo(achievement.icon, achievement.achievement, achievement.description);
                        achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 100);
                    }
                    // Normal updating procedure
                    else
                    {
                        // Checks the achievement's state to see how much info should be displayed
                        switch (achievement.achieveState)
                        {
                            // Player cannot see any info about the achievement
                            case AchievementState.Locked:
                                achieveDisplay.DisplayInfo(achieveDisplay.LockedIcon, "???", " ");
                                achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 100);
                                break;
                            // Player can see the unlock conditions for the achievement but not its icon
                            case AchievementState.Hidden:
                                achieveDisplay.DisplayInfo(achieveDisplay.LockedIcon, achievement.achievement, achievement.description);
                                achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 100);
                                break;
                            // Player can fully see the achievement
                            case AchievementState.Shown:
                                achieveDisplay.DisplayInfo(achievement.icon, achievement.achievement, achievement.description);
                                achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 50);
                                break;
                        }

                        // If the player has unlocked the achievement
                        if (achievement.hasUnlocked)
                        {
                            achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 100);
                        }

                        // Tells the game the display does not need to be updated atm
                        achievement.updateDisplay = false;
                    }
                }
            }
            else
            {
                Debug.Log($"AchieveID - {achieveDisplay.gameObject.name} - not found in the system when checking in UpdateachievementBtns()");
            }
        }
    }

    // Gets necessary components from the current scene if the script does not already have it
    void GetComponents()
    {
        if (!iMenu)
        {
            iMenu = GameObject.Find("Local UI").GetComponent<InputMenu>();
        }
    }

    // Opens achievement Menu
    public void OpenAchieveMenu(CanvasGroup menu)
    {
        // Stores previous menu and disables it
        prevMenu = menu;
        prevMenu.interactable = false;

        // Gets necessary components from the current scene if the script does not already have it
        GetComponents();

        // Closes settings menu if opened
        if (iMenu.pMenuF.settingsMenu.interactable)
            MenuOpenClose(iMenu.pMenuF.settingsMenu, false);

        // Updates achievement menu buttons based on player progression
        UpdateAchievements();

        // Opens achievement menu
        MenuOpenClose(achieveMenu, true);
    }

    // Closes achievement menu
    public void CloseAchieveMenu()
    {
        prevMenu.interactable = true;
        MenuOpenClose(achieveMenu, false);
    }

    // Opens or closes selected menus
    public void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        menu.interactable = isOpen;
        menu.alpha = isOpen ? 1 : 0;
        menu.blocksRaycasts = isOpen;
    }
}
