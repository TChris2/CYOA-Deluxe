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
    public void AchievePopup()
    {
        if (achieveQueue.Count == 0)
        {
            // Debug.Log("AchievementPopup: Achievement queue is empty");
            return;
        }

        Debug.Log($"AchievementPopup: Popping up {achieveQueue[0].achieveID}");

        // Displays achievement info
        DisplayInfo(achieveQueue[0].icon, achieveQueue[0].achievement, achieveQueue[0].description);

        popupAni.Play("Popup");
    }

    /// <summary>
    /// Adds achievement to popup queue
    /// </summary>
    public void PopupQueue(AchievementInfo achievement)
    {
        Debug.Log($"AchievementPopup: Adding {achievement.achieveID} to popup queue");

        achieveQueue.Add(achievement);
    }
    
    // Removes current item from the queue
    // Used as an animation event at the end of the Popdown animation
    public void RemoveFromQueue()
    {
        achieveQueue.Remove(achieveQueue[0]);

        // Popups the next achievement if there is still more in the queue
        if (achieveQueue.Count != 0)
            AchievePopup();
    }
}
