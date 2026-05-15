using System.Collections;
using UnityEngine;

// Unlock conditions for the achievement Doctor_1 - NO
public class AchievementDoctor_1 : Achievement
{   
    [SerializeField]
    private float closeGameDelay;

    void Start()
    {
        GetComponents();

        // Only runs remaining code if the player has not already unlocked the achievement
        if (sm.achieveDict.ContainsKey(achieveID) && !sm.achieveDict[achieveID].hasUnlocked)
        {
            StartCoroutine(CloseGame(sm.achieveDict[achieveID]));
        }
    }

    // Closes the game after a specific amt of time passes
    private IEnumerator CloseGame(AchievementInfo achievement)
    {
        yield return new WaitForSeconds(closeGameDelay);

        AchievementUnlock(achievement);

        Application.Quit();
    }
}
