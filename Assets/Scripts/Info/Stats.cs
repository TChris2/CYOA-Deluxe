using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Store player stats
[System.Serializable]
public class Stats
{
    // Game mode - Static
    public string gameMode = "The King of Drift";
    // Most used pokemon - Static
    public string mostUsedMon = "Mareep";
    // Most used move - Static
    public string mostUsedMove = "Thunderbolt";
    // Most used weapon
    public Dictionary<string, int> weaponDict = new Dictionary<string, int>();
    [Tooltip("Total amount of deaths")]
    public int deaths;
    [Tooltip("Current playtime of player")]
    public float playTime;
    
    // Resets to default values
    public void Reset()
    {
        gameMode = "The King of Drift";
        mostUsedMon = "Mareep";
        mostUsedMove = "Thunderbolt";
        weaponDict.Clear();
        deaths = 0;
        playTime = 0;
    }

    // Gets amt of completed choices
    public (int, int) ChoicesCompleted(Dictionary<string, ChoiceInfo> choiceDict)
    {
        int completed = 0;
        int total = 0;

        foreach(ChoiceInfo choice in choiceDict.Values)
        {
            if (choice.hasComplete)
                completed += 1;
            total += 1;
        }

        return (completed, total);
    }

    // Gets amt of completed achievements
    public int AchievementsCompleted(Dictionary<string, AchievementInfo> achieveDict)
    {
        int completed = 0;

        foreach(AchievementInfo achievement in achieveDict.Values)
        {
            if (achievement.hasUnlocked)
                completed += 1;
        }

        return completed;
    }

    // Gets amt of completed endings
    public (int, int) EndingsCompleted(Dictionary<string, ChoiceInfo> choiceDict)
    {
        int completed = 0;
        int total = 0;

        foreach(ChoiceInfo choice in choiceDict.Values)
        {
            if (choice.choiceState.Contains(ChoiceState.Ending))
            {
                if (choice.hasComplete)
                    completed += 1;
                total += 1;
            }
        }

        return (completed, total);
    }

    // Gets completion percentage
    public string Completion(Dictionary<string, ChoiceInfo> choiceDict, Dictionary<string, AchievementInfo> achieveDict)
    {
        var (choicesCompleted, b) = ChoicesCompleted(choiceDict);
        int completed = choicesCompleted + AchievementsCompleted(achieveDict);
        int total = choiceDict.Count + achieveDict.Count;
        return $"{Mathf.Floor(((float)completed / total) * 100)}%";
    }
}