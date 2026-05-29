using UnityEngine;

// Unlock conditions for the achievement Tesco_1 - The End Is Never The End Is Never
public class AchievementTesco_1 : Achievement
{
    GameManager gm;

    void Start()
    {
        GetComponents();
        InitializeAchievement(CheckLoop);
    }

    bool CheckLoop()
    {
        // If gm has not yet been assigned
        if (!gm)
            gm = FindAnyObjectByType<GameManager>();
        
        // Checks the previous choices the player has made to ensure they have not left the loop
        switch (gm.prevChoice)
        {
            // Start of the loop
            case "Tesco_1_1_1_1_1_1":
                // Sets the intial value
                PlayerPrefs.SetInt("Achievement - Tesco Loop", 1);
                Debug.Log($"AchievementTesco_1: {achieveID} - Loop vids played - {PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1}");
                break;
            // If the player continues the loop
            case "Tesco_1_1_1_1_1_1_1":
            case "Tesco_1_1_1_1_1_1Alt":
            case "Tesco_1_1_1_1_1_1_1Alt":
                // Increases the counter by one each time the player goes further into the loop
                PlayerPrefs.SetInt("Achievement - Tesco Loop", PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1);
                Debug.Log($"AchievementTesco_1: {achieveID} - Loop vids played - {PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) + 1}");
                break;
            // Resets the value to 0 if the player breaks the loop
            default:
                PlayerPrefs.SetInt("Achievement - Tesco Loop", 0);
                Debug.Log($"AchievementTesco_1: Loop count is reset");
                break;
        }
        
        // Ensures the value gets the saved across the loop
        PlayerPrefs.Save();

        // If the player meets the conditions for the achievement
        if (PlayerPrefs.GetInt("Achievement - Tesco Loop", 0) >= 9)
            return true;
        else
            return false;
    }
}
