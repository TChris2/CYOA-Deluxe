using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays achievement info in the achievement menu
/// </summary>
public class AchievementInfoDisplay : MonoBehaviour
{
    [Header("Achievement UI")]
    public Image popupIcon;
    [SerializeField]
    private TMP_Text popupLabel;
    [SerializeField]
    private TMP_Text popupDesc;
    public Sprite LockedIcon;

    /// <summary>
    /// Displays achievement info
    /// </summary>
    public void DisplayInfo(Sprite icon, string achievement, string description)
    {
        // Debug.Log("Displaying Info");
        popupIcon.sprite = icon;
        popupLabel.text = achievement;
        popupDesc.text = description;
    }
}
