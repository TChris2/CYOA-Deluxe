using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pops up achievement onscreen and display their info in the achievement menu
/// </summary>
public class AchievementPopup : AchievementInfoDisplay
{
    // Queue of achievements to display onscreen
    [SerializeField]
    private List<AchievementInfo> achieveQueue;
    Animator popupAni;

    void Start()
    {
        // Gets components
        popupAni = GetComponent<Animator>();
    }

    /// <summary>
    /// Pops the achievement popup onscreen
    /// </summary>
    public IEnumerator AchievePopup(AchievementInfo achievement)
    {
        // Adds achievement to the queue
        achieveQueue.Add(achievement);

        // Waits until the queue is clear before playing the popup for the achievement
        while (achievement.achieveID != achieveQueue[0].achieveID)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // Displays achievement info
        DisplayInfo(achievement.icon, achievement.achievement, achievement.description);

        popupAni.Play("Popup");
    }
    
    // Removes current item from the queue
    // Used as an animation event at the end of the Popdown animation
    public void RemoveFromQueue()
    {
        achieveQueue.Remove(achieveQueue[0]);
    }
}
