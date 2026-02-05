using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Displays achievement info in the achievement menu
public class AchievementInfoDisplay : MonoBehaviour
{
    [Header("Achievement UI")]
    public Image popupIcon;
    [SerializeField]
    private TMP_Text popupLabel;
    [SerializeField]
    private TMP_Text popupDesc;
    public Sprite LockedIcon;

    // Displays achievement info
    public void DisplayInfo(Sprite icon, string achievement, string description)
    {
        // Debug.Log("Displaying Info");
        popupIcon.sprite = icon;
        popupLabel.text = achievement;
        popupDesc.text = description;
    }
}
