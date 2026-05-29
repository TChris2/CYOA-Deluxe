using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Functionality for the Achievement menu
/// </summary>
public class AchievementMenu : MonoBehaviour
{
    // Stores the default scale of the achievement menu
    [SerializeField]
    private Transform achieveContents;
    [SerializeField]
    private Sprite LockedIcon;
    [SerializeField]
    private GameObject achievementDisplay;
    // Determines the order of achievements
    [SerializeField]
    private List<AchievementInfo> achieveOrder;
    CanvasGroup achieveMenu;
    // Scripts
    SaveManager sm;
    InputMenu iMenu;
    AchievementInfoDisplay[] achieveDisplays;
    
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
        if (achieveOrder.Count != sm.achieveDict.Count)
        {
            Debug.LogError("AchievementMenu: Error, achieveOrder & achieveDict lengths are not equal");
        }

        // Instantiates each achievement into the menu
        foreach(AchievementInfo id in achieveOrder)
        {
            if (sm.achieveDict.TryGetValue(id.achieveID, out AchievementInfo achievement))
            {
                GameObject achieveDisplay = Instantiate(achievementDisplay, achieveContents);
                achieveDisplay.name = achievement.achieveID;
            }
        }
    }

    // Updates achievement displays depending on the progress the player has made
    void UpdateAchievements()
    {
        // Debug.Log("AchievementMenu: Updating Achievements");

        // Gets all the achievement displays in the menu
        if (achieveDisplays == null)
            achieveDisplays = achieveContents.GetComponentsInChildren<AchievementInfoDisplay>();

        // Checks each display
        foreach (AchievementInfoDisplay achieveDisplay in achieveDisplays)
        {
            // Checks to make sure display has an id that's in the system
            if (sm.achieveDict.TryGetValue(achieveDisplay.gameObject.name, out AchievementInfo achievement))
            {
                // Only updates the display when achievement needs to be updated or when the override is enabled
                if (achievement.updateDisplay || iMenu.completeOverride)
                {
                    // Debug.Log($"AchievementMenu: Updating Achievement {achievement.achieveID}");

                    // Defaults for displaying achievement info
                    float iconGrayscale = 1;
                    float textGrayscale = 1;
                    Sprite inputIcon = achievement.icon; 
                    string inputAchievement = achievement.achievement;
                    string inputDescription = achievement.description;

                    // Normal updating procedure
                    if (!iMenu.completeOverride)
                    {
                        // Debug.Log($"AchievementMenu: {achievement.achieveState}");

                        // Checks the achievement's state to see how much info should be displayed
                        switch (achievement.achieveState)
                        {
                            // Player cannot see any info about the achievement
                            case AchievementState.Locked:
                                inputIcon = LockedIcon; 
                                inputAchievement = "???";
                                inputDescription = " ";
                                break;
                            // Player can see the unlock conditions for the achievement but not its icon
                            case AchievementState.Hidden:
                                inputIcon = LockedIcon; 
                                break;
                            // Player can fully see the achievement
                            case AchievementState.Shown:
                                // If the player has not unlocked the achievement
                                if (!achievement.hasUnlocked)
                                {
                                    iconGrayscale = .35f;
                                    textGrayscale = .35f;
                                }
                                break;
                        }

                        // Tells the game the display does not need to be updated atm
                        achievement.updateDisplay = false;
                    }
                    // Reverts the achievement back to its previous status if the achievement has not yet been completed
                    else if (!achievement.hasUnlocked)
                    {
                        achievement.updateDisplay = true;
                    }

                    achieveDisplay.DisplayInfo(inputIcon, inputAchievement, inputDescription);
                    achieveDisplay.UpdateColor(Color.HSVToRGB(0, 0, iconGrayscale), Color.HSVToRGB(0, 0, textGrayscale));
                }
            }
            else
            {
                Debug.Log($"AchievementMenu: AchieveID - {achieveDisplay.gameObject.name} - not found in the system when checking in UpdateachievementBtns()");
            }
        }
    }

    /// <summary>
    /// Opens Achievement Menu
    /// </summary>
    public void OpenAchieveMenu()
    {
        // Updates achievement menu buttons based on player progression
        UpdateAchievements();

        iMenu.OpenRegularMenu(achieveMenu);
    }
}
