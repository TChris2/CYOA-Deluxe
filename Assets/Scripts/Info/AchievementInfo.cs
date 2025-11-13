using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores achievement info
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
    [Tooltip("ChoiceIDs tied to the achievement\nIf the player has already completed any of theses ids it will display the unlock condition for the achievement if the AchievementState is set to Locked or Hidden")]
    public List<String> choiceIDs;
    [Tooltip("Icon of the achievement which is displayed in the achievement menu and in the achievement popup")]
    public Sprite icon;
    // Flags whether the info display in its menu needs to be updated
    [HideInInspector]
    public bool updateDisplay = true;
}

// Simplified version of the class used to save the vital information to json
[System.Serializable]
public class AchievementSaveData
{
    public string achieveID; 
    public string achievement;
    public AchievementState achieveState;
    public bool hasUnlocked;

    public AchievementSaveData(string achieveID, string achievement, AchievementState achieveState, bool hasUnlocked)
    {
        this.achieveID = achieveID;
        this.achievement = achievement;
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