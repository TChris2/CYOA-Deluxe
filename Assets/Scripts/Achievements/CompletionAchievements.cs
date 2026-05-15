using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Achievements which are completed once a player completes specific choices
public class CompletionAchievements : Achievement
{
    [SerializeField]
    [Tooltip("Choices which have to be completed to unlock the achievement")]
    private List<ChoiceInfo> choices = new List<ChoiceInfo>();

    void Start()
    {
        GetComponents();

        // Only runs remaining code if the player has not already unlocked the achievement
        if (sm.achieveDict.ContainsKey(achieveID) && !sm.achieveDict[achieveID].hasUnlocked && CheckCompletion())
        {
            StartCoroutine(AchievePopupDelay(sm.achieveDict[achieveID]));
        }
        else
        {
            if (sm.achieveDict[achieveID].hasUnlocked)
            {
                Debug.LogWarning($"Achievement {achieveID} has already been unlocked");
            }
        }
    }

    // Checks if all choices tied to the achievement have been completed
    public bool CheckCompletion()
    {
        foreach (ChoiceInfo choice in choices)
        {
            string choiceID = choice.choiceID.Trim();
            
            if (sm.choiceDict.ContainsKey(choice.choiceID.Trim()))
            {
                if (!sm.choiceDict[choiceID].hasComplete)
                {
                    Debug.Log($"Player has not yet completed {choice.choiceID} for the achievement {achievementInfo.achieveID}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"Error choice is {choice.choiceID} {choice.choiceID.Length} not in ChoiceDict");
            }
        }

        // Debug.Log($"Player has completed all choices for the achievement {achievement.achieveID}");
        return true;
    }
}
