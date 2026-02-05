using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Pops up achievement onscreen and display their info in the achievement menu
public class AchievementPopup : MonoBehaviour
{
    // Queue of achievements to display onscreen
    [SerializeField]
    private List<AchievementInfo> achieveQueue;

    // Pops the achievement popup onscreen
    public IEnumerator AchievePopup(AchievementInfo achievement)
    {
        // Gets components
        Animator popupAni = GetComponent<Animator>();
        AchievementInfoDisplay achieveDisplay = GetComponent<AchievementInfoDisplay>();

        // Adds achievement to the queue
        achieveQueue.Add(achievement);

        // Waits until the queue is clear before playing the popup for the achievement
        while (achievement.achieveID != achieveQueue[0].achieveID)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // Displays achievement info
        achieveDisplay.DisplayInfo(achievement.icon, achievement.achievement, achievement.description);

        popupAni.Play("Popup");
    }
    
    // Removes current item from the queue
    // Used as an animation event at the end of the Popdown animation
    public void RemoveFromQueue()
    {
        achieveQueue.Remove(achieveQueue[0]);
    }
}
