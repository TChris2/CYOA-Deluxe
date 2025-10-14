using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores achievement info
[System.Serializable]
public class AchievementInfo
{
    // IDs for achievements
    public AchievementID choiceID;
    // Name of the achievement   
    public string achievement;
    // Description of the achievement, describes the unlock condition
    public string description;
    // Keeps track of whether the player has met the unlock conditions for the achievement
    public bool hasUnlocked;
    // Achievement state
    public AchievementState achieveState;
}

// Achievement state
public enum AchievementState
{
    // Default - Player has not unlocked the conditions for the achievement, unlock conditions can be seen in the menu
    Locked,
    // Player has not unlocked the conditions for the achievement, unlock conditions cannot be seen in the menu
    Hidden,
    // Player has unlocked the conditions for the achievement
    Unlocked
}

// List of Achievement ids
public enum AchievementID
{
    None
}