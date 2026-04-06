using UnityEngine;

// Parent class of achievement scripts, holds general info and functions
public class Achievement : MonoBehaviour
{
    [SerializeField]
    protected AchievementInfo achievementInfo;
    protected string achieveID;
    protected SaveManager sm;

    // Gets general components
    protected void GetComponents()
    {
        sm = FindAnyObjectByType<SaveManager>();

        // Trims string in case of empty space
        achieveID = achievementInfo.achieveID.Trim();
    }

    // Unlocks achievement and adds it to the achievement popup queue
    protected void AchievementUnlock(AchievementInfo achievement, bool isPopup)
    {
        Debug.Log($"Achievement {achievement.achieveID} Unlocked!");
        // Marked the achievement as unlocked
        achievement.hasUnlocked = true;
        // Tells the game that it needs to update its display in the achievements menu
        achievement.updateDisplay = true;
        // Changes the achievement's state from Locked or Hidden to Shown
        achievement.achieveState = AchievementState.Shown;
        // Tells the game whether to display the achievement popup on screen
        if (isPopup)
            StartCoroutine(sm.achievePopup.AchievePopup(achievement));
    }
}
