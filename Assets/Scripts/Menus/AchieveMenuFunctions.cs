using UnityEngine;
using UnityEngine.SceneManagement;

// Functionality for the achievement menu
public class AchieveMenuFunctions : MonoBehaviour
{
    // Stores the default scale of the achievement menu
    [SerializeField]
    private Transform achieveContents;
    [SerializeField]
    private GameObject achievementDisplay;
    CanvasGroup achieveMenu;
    // Scripts
    SaveManager sm;
    InputMenu iMenu;
    
    void Start()
    {
        // Gets components
        achieveMenu = GetComponent<CanvasGroup>();
        sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();

        // Loads all achievements stored in memory
        LoadAchievements();
    }

    // Loads all achievements stored in memory
    void LoadAchievements()
    {
        // Instantiates each achievement into the menu
        foreach(AchievementInfo achievement in sm.achieveDict.Values)
        {
            GameObject achieveDisplay = Instantiate(achievementDisplay, achieveContents);
            achieveDisplay.name = achievement.achieveID;
        }
    }

    // Updates achievement displays depending on the progress the player has made
    void UpdateAchievements()
    {
        // Debug.Log("Updating Achievements");

        // Gets all the achievement displays in the menu
        AchievementInfoDisplay[] achieveDisplays = achieveContents.GetComponentsInChildren<AchievementInfoDisplay>();

        // Checks each display
        foreach (AchievementInfoDisplay achieveDisplay in achieveDisplays)
        {
            // Checks to make sure display has an id that's in the system
            if (sm.achieveDict.TryGetValue(achieveDisplay.gameObject.name, out AchievementInfo achievement))
            {
                // Only updates the display when achievement needs to be updated or when the override is enabled
                if (achievement.updateDisplay || iMenu.completeOverride)
                {
                    // Debug.Log($"Updating Achievement {achievement.achieveID}");

                    // If the override is enabled it will display the achievement at full completion
                    if (iMenu.completeOverride)
                    {
                        achieveDisplay.DisplayInfo(achievement.icon, achievement.achievement, achievement.description);
                        achieveDisplay.popupIcon.color = Color.HSVToRGB(0, 0, 100);
                        // Allows game to update the achievement back to its proper status
                        achievement.updateDisplay = true;
                    }
                    // Normal updating procedure
                    else
                    {
                        // Debug.Log(achievement.achieveState);

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

    // Opens achievement Menu
    public void OpenAchieveMenu()
    {
        // Adds menu to menu list
        iMenu.openMenus.Add(achieveMenu);

        // Disables previous menu
        iMenu.openMenus[iMenu.openMenus.Count - 2].interactable = false;
        
        if (SceneManager.GetActiveScene().name == "Main Game")
        {
            // Closes settings menu if opened
            if (iMenu.pMenuF.settingsMenu.interactable)
                iMenu.MenuOpenClose(iMenu.pMenuF.settingsMenu, false);
        }

        // Updates achievement menu buttons based on player progression
        UpdateAchievements();

        // Opens achievement menu
        iMenu.MenuOpenClose(iMenu.openMenus[iMenu.openMenus.Count - 1], true);
    }
}
