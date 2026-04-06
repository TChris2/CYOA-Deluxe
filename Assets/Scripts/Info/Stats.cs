using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

// Store player stats
[System.Serializable]
public class Stats
{
    [Tooltip("Game mode - Static Field")]
    public string gameMode = "The King of Drift";
    [Tooltip("Most used Pokemon - Static Field")]
    public string mostUsedMon = "Mareep";
    [Tooltip("Most used move - Static Field")]
    public string mostUsedMove = "Thunderbolt";
    // Most used weapon
    public Dictionary<string, int> weaponDict = new Dictionary<string, int>();
    [Tooltip("Total amount of deaths")]
    public int deaths;
    [Tooltip("Current playtime of player")]
    public float playTime;
    // Choices which do not contribute to totals
    HashSet<string> skipChoiceIDs = new HashSet<string> { "Start_", "Retry_", "Tesco_1_1_1_1_1_1Alt", "Tesco_1_1_1_1_1_1_1Alt", 
        "BOTW_1_1_1_2_1", "BOTW_1_1_2_1_1", "BOTW_1_1_1_2_2" };
    
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

    // Gets the amount of completed choices
    public (int, int) ChoicesCompleted(Dictionary<string, ChoiceInfo> choiceDict)
    {
        int completed = 0;
        int total = 0;

        foreach(ChoiceInfo choice in choiceDict.Values)
        {
            // skipChoiceIDs do not contribute to the stats
            if (!skipChoiceIDs.Contains(choice.choiceID))
            {
                if (choice.hasComplete)
                    completed += 1;
                total += 1;
            }
        }

        return (completed, total);
    }

    // Gets the amount of completed achievements
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

    // Gets the amount of completed endings
    public (int, int) EndingsCompleted(Dictionary<string, ChoiceInfo> choiceDict)
    {
        int completed = 0;
        int total = 0;

        foreach(ChoiceInfo choice in choiceDict.Values)
        {
            // Adds each choice which has the ChoiceState Ending attached
            if (choice.choiceState.Contains(ChoiceState.Ending))
            {
                if (choice.hasComplete)
                    completed += 1;
                total += 1;
            }
        }

        return (completed, total);
    }

    // Gets the amount of completed fails
    public (int, int) FailsCompleted(Dictionary<string, ChoiceInfo> choiceDict)
    {
        int completed = 0;
        int total = 0;

        foreach(ChoiceInfo choice in choiceDict.Values)
        {
            // Adds each choice which has the ChoiceState Ending attached
            if (choice.choiceState.Contains(ChoiceState.GameOver))
            {
                if (choice.hasComplete)
                    completed += 1;
                total += 1;
            }
        }

        return (completed, total);
    }

    // Gets completion percentage
    public float Completion(Dictionary<string, ChoiceInfo> choiceDict, Dictionary<string, AchievementInfo> achieveDict)
    {
        int choicesCompleted, b;

        (choicesCompleted, b) = ChoicesCompleted(choiceDict);
        // Adds the completed choices and achievements together
        int completed = choicesCompleted + AchievementsCompleted(achieveDict);
        // Debug.Log($"Choices Completed {choicesCompleted}, Total Choices {choiceDict.Count}, Choices Skipped {skipChoiceIDs.Count}");
        // Adds the total of choices and achievements, subtracts the skipChoiceIDs from the choice total
        int total = choiceDict.Count - skipChoiceIDs.Count + achieveDict.Count;
        return Mathf.Floor(((float)completed / total) * 100);
    }

    // Displays all stats
    public void DisplayStatsAll(List<TMP_Text> statsText, SaveManager sm)
    {
        int choicesCompleted, choicesCompletedTotal, endingsCompleted, endingsCompletedTotal;

        statsText[0].text = sm.stats.gameMode;
        statsText[1].text = sm.stats.mostUsedMon;
        statsText[2].text = sm.stats.mostUsedMove;
        // Returns the weapon with the highest value, if blank returns None
        statsText[3].text = sm.stats.weaponDict.Count != 0 ? 
            sm.stats.weaponDict.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key.ToString() : "None";
        // Returns Boots value in weaponDict, if blank returns 0
        statsText[4].text = sm.stats.weaponDict.ContainsKey("Boots") ? sm.stats.weaponDict["Boots"].ToString() : "0";
        statsText[5].text = sm.stats.deaths.ToString();
        // Gets the amount of completed choices
        (choicesCompleted, choicesCompletedTotal) = ChoicesCompleted(sm.choiceDict);
        statsText[6].text = $"{choicesCompleted}/{choicesCompletedTotal}";
        // Gets the amount of completed endings
        (endingsCompleted, endingsCompletedTotal) = EndingsCompleted(sm.choiceDict);
        statsText[7].text = $"{endingsCompleted}/{endingsCompletedTotal}";
        // Gets completion percentage
        statsText[8].text = $"{Completion(sm.choiceDict, sm.achieveDict)}%";
        // Converts play time to hour:minute:second format
        TimeSpan time = TimeSpan.FromSeconds(sm.stats.playTime);
        statsText[9].text = $"{time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }

    // Displays short version of stats
    public void DisplayStatsShort(List<TMP_Text> statsText, SaveManager sm)
    {
        int choicesCompleted, choicesCompletedTotal, endingsCompleted, endingsCompletedTotal;

        statsText[0].text = sm.stats.deaths.ToString();
        // Gets the amount of completed choices
        (choicesCompleted, choicesCompletedTotal) = ChoicesCompleted(sm.choiceDict);
        statsText[1].text = $"{choicesCompleted}/{choicesCompletedTotal}";
        // Gets the amount of completed endings
        (endingsCompleted, endingsCompletedTotal) = EndingsCompleted(sm.choiceDict);
        statsText[2].text = $"{endingsCompleted}/{endingsCompletedTotal}";
        // Gets completion percentage
        statsText[3].text = $"{Completion(sm.choiceDict, sm.achieveDict)}%";
        // Converts play time to hour:minute:second format
        TimeSpan time = TimeSpan.FromSeconds(sm.stats.playTime);
        statsText[4].text = $"{time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }
}