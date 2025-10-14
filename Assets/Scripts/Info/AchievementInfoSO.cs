using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores achievement info in scriptable object format
[CreateAssetMenu(fileName = "AchievementInfoSO", menuName = "Scriptable Objects/AchievementInfoSO")]
public class AchievementInfoSO : ScriptableObject
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