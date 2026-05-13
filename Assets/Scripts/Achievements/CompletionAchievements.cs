using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Achievements which are completed once a player completes specific choices
public class CompletionAchievements : Achievement
{
    [SerializeField]
    private List<ChoiceInfo> choices = new List<ChoiceInfo>();
    [SerializeField]
    [Range(0f,20f)]
    private float popupOffset = 10f;

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

    // Waits until the user gets to the ending of the vid
    private IEnumerator AchievePopupDelay(AchievementInfo achievement)
    {
        // Gets remaining scripts
        GameManager gm = FindAnyObjectByType<GameManager>();

        while (gm.videoPlayer.time < gm.currentChoice.vidEndTime - popupOffset)
        {
            // Debug.Log(videoPlay.time);
            yield return null;
        }

        AchievementUnlock(achievement, true);
    }
}
