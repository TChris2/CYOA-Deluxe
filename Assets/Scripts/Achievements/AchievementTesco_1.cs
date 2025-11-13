using UnityEngine;

// Unlock conditions for the achievement Tesco_1 - The End Is Never
public class AchievementTesco_1 : MonoBehaviour
{
    void Awake()
    {
        // Get initial components
        SaveManager sm = GameObject.Find("SaveManager").GetComponent<SaveManager>();

        AchievementInfo achievement;
        // Only runs remaining code if the player has not already unlocked the achievement
        if (sm.achieveDict.TryGetValue("Tesco_1", out achievement) && !achievement.hasUnlocked)
        {
            // Gets remaining scripts
            ButtonManager bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();

            // Checks the previous choices the player has made to ensure they have not left the loop
            switch (bm.prevChoice)
            {
                // Start of the loop
                case "Tesco_1_1_1_1_1_1":
                    // Sets the intial value
                    PlayerPrefs.SetInt("Achievement - Tesco Loop", 1);
                    Debug.Log($"Loop vids played - {PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1}");
                    break;
                // If the player continues the loop
                case "Tesco_1_1_1_1_1_1_1":
                case "Tesco_1_1_1_1_1_1Alt":
                case "Tesco_1_1_1_1_1_1_1Alt":
                    // Increases the counter by one each time the player goes further into the loop
                    PlayerPrefs.SetInt("Achievement - Tesco Loop", PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1);
                    Debug.Log($"Loop vids played - {PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1}");
                    break;
                // Resets the value to 0 if the player breaks the loop
                default:
                    PlayerPrefs.SetInt("Achievement - Tesco Loop", 0);
                    Debug.Log($"Loop count is reset");
                    break;
            }
            // Ensures the value gets the saved across the loop
            PlayerPrefs.Save();

            // If the player meets the conditions for the achievement
            if (PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) >= 9)
            {
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
    }
}
