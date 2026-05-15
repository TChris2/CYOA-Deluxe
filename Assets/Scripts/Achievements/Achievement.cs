using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Parent class of achievement scripts, holds general info and functions
/// </summary>
public class Achievement : MonoBehaviour
{
    [SerializeField]
    protected AchievementInfo achievementInfo;
    protected string achieveID;
    protected SaveManager sm;

    /// <summary>
    /// Gets general components for achievements
    /// </summary>
    protected void GetComponents()
    {
        sm = FindAnyObjectByType<SaveManager>();

        // Trims string in case of empty space
        achieveID = achievementInfo.achieveID.Trim();
    }

    /// <summary>
    /// Waits until the child object is popped up on screen in GameManager
    /// </summary>
    protected IEnumerator AchievePopupDelay(AchievementInfo achievement)
    {
        // Gets remaining scripts
        GameObject child = transform.GetChild(0).gameObject;

        if (!child)
            Debug.LogError("Error, child object does not exist");

        yield return null;

        while (!child.activeSelf)
            yield return null;

        AchievementUnlock(achievement);
        StartCoroutine(sm.achievePopup.AchievePopup(achievement));
    }

    /// <summary>
    /// Unlocks achievement and adds it to the achievement popup queue
    /// </summary>
    protected void AchievementUnlock(AchievementInfo achievement)
    {
        Debug.Log($"Achievement {achievement.achieveID} Unlocked!");
        // Marked the achievement as unlocked
        achievement.hasUnlocked = true;
        // Tells the game that it needs to update its display in the achievements menu
        achievement.updateDisplay = true;
        // Changes the achievement's state from Locked or Hidden to Shown
        achievement.achieveState = AchievementState.Shown;
    }
}
