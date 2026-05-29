using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    // Stores logic for each achievement
    public Dictionary<string, Func<bool>> achieveLogicDict = new Dictionary<string, Func<bool>>();
    [SerializeField]
    private List<AchievementInfo> generalAchievements = new List<AchievementInfo>();
    // List of achievement set to be popped up when they reach either a choice, ending, or both
    [SerializeField]
    private List<AchievementInfo> choicePopupQueue = new List<AchievementInfo>();
    [SerializeField]
    private List<AchievementInfo> endingPopupQueue = new List<AchievementInfo>();
    [SerializeField]
    private List<AchievementInfo> generalPopupQueue = new List<AchievementInfo>();
    [SerializeField]
    private AchievementPopup achievePopup;
    [HideInInspector]
    public SaveManager sm;
    GameManager gm;

    void Start()
    {
        sm = GetComponentInParent<SaveManager>();

        // Adds logic for general achievements
        AddAchievementLogic("General_1", () => sm.stats.deaths > 0);
        AddAchievementLogic("General_2", () => {
            int completed, total;
            (completed, total) = sm.stats.FailsCompleted(sm.choiceDict);
            return completed == total;
        });
        AddAchievementLogic("General_3", () => {
            int completed, total;
            (completed, total) = sm.stats.EndingsCompleted(sm.choiceDict);
            return completed > 0;
        });
        AddAchievementLogic("General_4", () => {
            int completed, total;
            (completed, total) = sm.stats.EndingsCompleted(sm.choiceDict);
            return completed == total;
        });
        AddAchievementLogic("General_5", () => {
            int completed, total;
            (completed, total) = sm.stats.ChoicesCompleted(sm.choiceDict);
            return completed == total;
        });
        AddAchievementLogic("General_6", () => true);
        AddAchievementLogic("General_7", () => 
            sm.stats.Completion(sm.choiceDict) >= 100
        );
    }

    /// <summary>
    /// Adds achievement logic to achieveLogicDict
    /// </summary>
    public void AddAchievementLogic(string achieveID, Func<bool> achieveLogic)
    {
        // If the ID is not in achieveDict
        if (!sm.achieveDict.ContainsKey(achieveID))
        {
            Debug.LogWarning($"AchievementManager: AchieveID {achieveID} not found in achieveDict");
        }

        // Checks for duplicate ids
        if (achieveLogicDict.ContainsKey(achieveID))
        {
            Debug.LogWarning($"AchievementManager: Duplicate AchieveID detected, {achieveID} in achieveLogicDict");
        }

        // Debug.Log($"AchievementManager: Adding logic for achievement {achieveID}");
        
        // Adds achievement logic to the dict
        achieveLogicDict.Add(achieveID, achieveLogic);
    }

    #region Achievement Logic

    /// <summary>
    /// Clears popup queues
    /// </summary>
    public void ClearPopupQueues()
    {
        choicePopupQueue.Clear();
        endingPopupQueue.Clear();
        generalPopupQueue.Clear();
    }

    /// <summary>
    /// Checks to see if the condition for the achievement has been met
    /// </summary>
    public void CheckAchievement(string achieveID)
    {
        // If the ID is not in achieveDict
        if (!sm.achieveDict.ContainsKey(achieveID))
        {
            Debug.LogWarning($"AchievementManager: AchieveID {achieveID} not found in achieveDict");
        }

        AchievementInfo achievement = sm.achieveDict[achieveID];

        // Checks to see if conditions for the achievement have been met if it has not already been unlocked
        if (!achievement.hasUnlocked)
        {
            if (achieveLogicDict.TryGetValue(achieveID, out Func<bool> achieveCondition))
            {
                if (achieveCondition())
                    AchievementUnlock(achievement);
                else
                {
                    Debug.Log($"AchievementManager: Conditions have not yet been met for achievement {achieveID}");
                }
            }
            else
            {
                Debug.Log($"AchievementManager: AchieveID {achieveID} not found in achieveLogicDict");
            }
        }
        else
        {                
            Debug.LogWarning($"AchievementManager: AchieveID {achieveID} already completed");
        }
    }
    
    /// <summary>
    /// Unlocks achievement and adds it to the achievement popup queue
    /// </summary>
    void AchievementUnlock(AchievementInfo achievement)
    {
        Debug.Log($"AchievementManager: Achievement {achievement.achieveID} Unlocked!");
        // Marked the achievement as unlocked
        achievement.hasUnlocked = true;
        // Tells the game that it needs to update its display in the achievements menu
        achievement.updateDisplay = true;
        // Changes the achievement's state from Locked or Hidden to Shown
        achievement.achieveState = AchievementState.Shown;

        // If the achievement is displayed on screen when unlocked
        if (achievement.isDisplayed)
        {
            AchievementPopupCheck(achievement);
        }
    }

    #endregion
    #region Popup Logic

    void AchievementPopupCheck(AchievementInfo achievement)
    {
        
        switch (achievement.popupTime)
        {
            case PopupTime.Choice:
                Debug.Log($"AchievementManager: Adding {achievement.achieveID} to choice popup queue");
                choicePopupQueue.Add(achievement);
                break;
            case PopupTime.Ending:
                Debug.Log($"AchievementManager: Adding {achievement.achieveID} to ending popup queue");
                endingPopupQueue.Add(achievement);
                break;
            case PopupTime.General:
                Debug.Log($"AchievementManager: Adding {achievement.achieveID} to general popup queue");
                generalPopupQueue.Add(achievement);
                break;
            case PopupTime.Immediate:
                achievePopup.PopupQueue(achievement);
                achievePopup.AchievePopup();
                break;
            case PopupTime.Custom:
                StartCoroutine(AchievePopupDelay(achievement));
                break;
            }
    }

    /// <summary>
    /// Loads the popup queue achievements into the main popup queue
    /// </summary>
    void LoadAchievePopups(List<AchievementInfo> queue, string queueName)
    {
        if (queue.Count == 0)
        {
            Debug.Log($"AchievementPopup: {queueName} popup queue is empty");
            return;
        }

        LoadQueue(queue);
        queue.Clear();

        if (generalPopupQueue.Count != 0)
        {
            LoadQueue(generalPopupQueue);
            generalPopupQueue.Clear();
        }

        achievePopup.AchievePopup();
    }

    public void LoadChoiceAchievePopups() => LoadAchievePopups(choicePopupQueue, "Choice");
    public void LoadEndingAchievePopups() => LoadAchievePopups(endingPopupQueue, "Ending");

    /// <summary>
    /// Loads the list of achievements into the main popup queue
    /// </summary>
    void LoadQueue(List<AchievementInfo> achieveQueue)
    {
        while (achieveQueue.Count > 0)
        {
            achievePopup.PopupQueue(achieveQueue[0]);
            achieveQueue.RemoveAt(0);
        }
    }

    /// <summary>
    /// Loads both popup queue achievements into the main popup queue
    /// </summary>
    public void LoadAllAchievePopups()
    {
        LoadChoiceAchievePopups();
        LoadEndingAchievePopups();
    }

    /// <summary>
    /// Waits until the child object is popped up on screen in GameManager
    /// </summary>
    IEnumerator AchievePopupDelay(AchievementInfo achievement)
    {
        Debug.Log($"AchievementManager: Achievement popup delay begun for {achievement.achieveID}");

        // If gm has not yet been assigned
        if (!gm)
            gm = FindAnyObjectByType<GameManager>();

        yield return null;

        while (gm.videoPlayer.time < achievement.customPopupTime)
            yield return null;

        achievePopup.PopupQueue(achievement);
        achievePopup.AchievePopup();
    }

    #endregion
    #region General Achievement Logic

    /// <summary>
    /// Checks if the player has met any of the requirements for the general achievements
    /// </summary>
    public void GeneralAchievementsCheck()
    {
        // Debug.Log("AchievementManager: General Achievements Check");

        // Goes through all of the general achievements
        foreach (AchievementInfo achievement in generalAchievements)
        {
            // Debug.Log($"AchievementManager: Checking {achievement.achieveID}");
            CheckAchievement(achievement.achieveID);
        }
    }   

    #endregion
}
