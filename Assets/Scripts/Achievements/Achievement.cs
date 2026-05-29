using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Parent class of achievement scripts, holds general info and functions
/// </summary>
public class Achievement : MonoBehaviour
{
    [SerializeField]
    protected AchievementInfo achievement;
    protected string achieveID;
    protected AchievementManager am;

    /// <summary>
    /// Gets general components for achievements
    /// </summary>
    protected void GetComponents()
    {
        am = GetComponentInParent<AchievementManager>();

        // Trims string in case of empty space
        achieveID = achievement.achieveID.Trim();
    }

    /// <summary>
    /// Intializes the achievement
    /// </summary>
    protected void InitializeAchievement(Func<bool> achieveLogic)
    {
        am.AddAchievementLogic(achieveID, achieveLogic);
    }
}
