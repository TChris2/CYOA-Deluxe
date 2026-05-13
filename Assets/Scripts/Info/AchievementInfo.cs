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
    [Header("Achievement Menu")]
    [Tooltip("Description of the achievement")]
    public string description;
    [Tooltip("Icon of the achievement which is displayed in the achievement menu and in the achievement popup")]
    public Sprite icon;
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
        updateDisplay = achievementInfo.updateDisplay;
    }

    // Trims strings of empty space    
    void OnValidate()
    {
        achieveID.Trim();
        achievement.Trim();
        description.Trim();
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