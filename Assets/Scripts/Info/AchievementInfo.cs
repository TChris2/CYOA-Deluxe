using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores achievement info
/// </summary>
[CreateAssetMenu(fileName = "AchievementInfo", menuName = "Scriptable Objects/AchievementInfo")]
public class AchievementInfo : ScriptableObject
{
    // Base achievement info
    [Header("Achievement Info")]
    [Tooltip("Id for the achievement")]
    public string achieveID;
    [Tooltip("Achievement")]
    public string achievement;
    [Tooltip("Initial State of achievement")]
    public AchievementState achieveState;
    [Tooltip("Whether the player has unlocked achievement")]
    public bool hasUnlocked;
    // Achievement menu info
    [Header("Display Info")]
    [Tooltip("Description of the achievement")]
    public string description;
    [Tooltip("Icon of the achievement which is displayed in the achievement menu and in the achievement popup")]
    public Sprite icon;
    [Tooltip("If the achievement when unlocked is displayed on screen")]
    public bool isDisplayed;
    [Tooltip("When the achievement in displayed on screen\nChoice - When a choice is appears on screen\nEnding - When the retry menu of an ending popups on screen\nCustom - Custom popup time, do not use if the achievement is tied to multiple vids")]
    public PopupTime popupTime;
    [Tooltip("Custom time when the achievement is displayed on screen if popupTime is set to Custom")]
    public float customPopupTime;
    // Flags whether the info display in its menu needs to be updated
    [HideInInspector]
    public bool updateDisplay = true;

    /// <summary>
    /// Adds info to new instance of AchievementInfo
    /// </summary>
    public void AddInfo(AchievementInfo achievementInfo)
    {
        achieveID = achievementInfo.achieveID;
        achievement = achievementInfo.achievement;
        achieveState = achievementInfo.achieveState;
        hasUnlocked = achievementInfo.hasUnlocked;
        description = achievementInfo.description;
        icon = achievementInfo.icon;
        isDisplayed = achievementInfo.isDisplayed;
        popupTime = achievementInfo.popupTime;
        customPopupTime = achievementInfo.customPopupTime;
        updateDisplay = achievementInfo.updateDisplay;
    }

    // Trims strings of empty space    
    void OnValidate()
    {
        achieveID = achieveID.Trim();
        achievement = achievement.Trim();
        description = description.Trim();
    }
}

/// <summary>
/// Simplified version of the class used to save the vital information
/// </summary>
[System.Serializable]
public class AchievementSaveData
{
    public string achieveID; 
    public AchievementState achieveState;
    public bool hasUnlocked;

    /// <summary>
    /// Adds info to new instance of AchievementSaveData
    /// </summary>
    public AchievementSaveData(string achieveID, AchievementState achieveState, bool hasUnlocked)
    {
        this.achieveID = achieveID;
        this.achieveState = achieveState;
        this.hasUnlocked = hasUnlocked;
    }
}

// Achievement state
public enum AchievementState
{
    // Default - Player cannot see any info about the achievement
    Locked,
    // Player can see the unlock conditions for the achievement but not its icon
    Hidden,
    // Player can fully see the achievement
    Shown
}

// Time an achievement is popped up on screen
public enum PopupTime
{
    // When a choice is appears on screen
    Choice,
    // When the retry menu of an ending popups on screen
    Ending,
    // If it can appear during either a choice or ending
    General,
    // Popups the achievement immediately
    Immediate,
    // Custom popup time, do not use if the achievement is tied to multiple vids
    Custom
}