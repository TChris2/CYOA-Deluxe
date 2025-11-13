using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Pops up achievement onscreen
public class AchievementPopup : MonoBehaviour
{
    // Queue of achievements to display onscreen
    [SerializeField]
    private List<AchievementInfo> achieveQueue;
    private Animator popupAni;
    [Header("Achievement UI")]
    public Image popupIcon;
    [SerializeField]
    private TMP_Text popupLabel;
    [SerializeField]
    private TMP_Text popupDesc;
    public Sprite LockedIcon;

    public IEnumerator AchievePopup(AchievementInfo achievement)
    {
        // Gets animator
        popupAni = GetComponent<Animator>();

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

    // Displays achievement info
    public void DisplayInfo(Sprite icon, string achievement, string description)
    {
        popupIcon.sprite = icon;
        popupLabel.text = achievement;
        popupDesc.text = description;
    }
    
    // Removes current item from the queue
    // Used as an animation event at the end of the Popdown animation
    public void RemoveFromQueue()
    {
        achieveQueue.Remove(achieveQueue[0]);
    }
}
