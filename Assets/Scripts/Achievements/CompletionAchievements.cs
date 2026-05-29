using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Achievements which are completed once a player completes specific choices
public class CompletionAchievements : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Choices which have to be completed to unlock the achievement")]
    private List<CompletionAchievement> achievementList = new List<CompletionAchievement>();
    AchievementManager am;

    void Start()
    {
        am = GetComponentInParent<AchievementManager>();

        foreach (CompletionAchievement completionAchievement in achievementList)
        {
            completionAchievement.achieveID = completionAchievement.achievement.achieveID.Trim();
            CompletionAchievement completionAchieve = completionAchievement;
            InitializeAchievement(completionAchievement, () => CheckCompletion(completionAchieve));
        }
    }

    /// <summary>
    /// Intializes the achievement
    /// </summary>
    void InitializeAchievement(CompletionAchievement completionAchievement, Func<bool> achieveLogic)
    {
        am.AddAchievementLogic(completionAchievement.achieveID, achieveLogic);
    }

    // Checks if all choices tied to the achievement have been completed
    bool CheckCompletion(CompletionAchievement completionAchievement)
    {
        Debug.Log($"CompletionAchievements: Checking completion for achievement {completionAchievement.achieveID}");
        
        foreach (ChoiceInfo choice in completionAchievement.choices)
        {
            string choiceID = choice.choiceID.Trim();
            
            if (am.sm.choiceDict.ContainsKey(choice.choiceID.Trim()))
            {
                if (!am.sm.choiceDict[choiceID].hasComplete)
                {
                    Debug.Log($"CompletionAchievements: Player has not yet completed {choice.choiceID} for the achievement {completionAchievement.achieveID}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"CompletionAchievements: Error choice is {choice.choiceID} {choice.choiceID.Length} not in ChoiceDict");
            }
        }

        // Debug.Log($"CompletionAchievements: Player has completed all choices for the achievement {achievement.achieveID}");
        return true;
    }
}

/// <summary>
/// Contains the achievement & which choices that have to be completed to unlock it
/// </summary>
[System.Serializable]
public class CompletionAchievement
{
    public AchievementInfo achievement;
    [Tooltip("Choices which have to be completed to unlock the achievement")]
    public List<ChoiceInfo> choices = new List<ChoiceInfo>();
    [HideInInspector]
    public string achieveID;
}
