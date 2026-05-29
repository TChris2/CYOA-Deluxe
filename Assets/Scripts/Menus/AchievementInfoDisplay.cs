using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays achievement info in the achievement menu
/// </summary>
public class AchievementInfoDisplay : MonoBehaviour
{
    [Header("Achievement UI")]
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TMP_Text label;
    [SerializeField]
    private Image labelLine;
    [SerializeField]
    private TMP_Text desc;

    /// <summary>
    /// Displays achievement info
    /// </summary>
    public void DisplayInfo(Sprite inputIcon, string inputAchievement, string inputDescription)
    {
        // Debug.Log("AchievementInfoDisplay: Displaying Info");
        icon.sprite = inputIcon;
        label.text = inputAchievement;
        desc.text = inputDescription;
    }

    public void UpdateColor(Color iconColor, Color textColor)
    {
        icon.color = iconColor;
        label.color = textColor;
        labelLine.color = textColor;
        desc.color = textColor;   
    }
}
