using UnityEngine;
using System.Collections;

// Achievements which are completed once a player completes a certian choice
public class CompletionAchievements : MonoBehaviour
{
    [SerializeField]
    private string AchieveID;

    [SerializeField]
    [Range(0f,20f)]
    private float popupOffset = 10f;
    void Awake()
    {
        // Get initial components
        SaveManager sm = FindAnyObjectByType<SaveManager>();

        AchievementInfo achievement;
        // Only runs remaining code if the player has not already unlocked the achievement
        if (sm.achieveDict.TryGetValue(AchieveID.Trim(), out achievement) && !achievement.hasUnlocked)
        {
            StartCoroutine(AchievePopupDelay(achievement));
        }
    }

    // Waits until the user gets to the ending of the vid
    private IEnumerator AchievePopupDelay(AchievementInfo achievement)
    {
        // Gets remaining scripts
        GameManager gm = FindAnyObjectByType<GameManager>();

        while (gm.videoPlay.time < gm.currentChoice.vidEndTime - popupOffset)
        {
            // Debug.Log(videoPlay.time);
            yield return null;
        }

        Debug.Log($"Achievement {achievement.achieveID} Unlocked!");
        // Marked the achievement as unlocked
        achievement.hasUnlocked = true;
        // Tells the game that it needs to update its display in the achievements menu
        achievement.updateDisplay = true;
        // Changes the achievement's state from Hidden to Shown
        achievement.achieveState = AchievementState.Shown;
        // Tells the game to display the achievement popup on screen
        AchievementPopup achievePopup = GameObject.Find("Achievement Popup").GetComponent<AchievementPopup>();
        StartCoroutine(achievePopup.AchievePopup(achievement));
    }
}
