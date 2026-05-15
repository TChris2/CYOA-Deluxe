using UnityEngine;

// Unlock conditions for the achievement Minecraft_1 - Squatter
public class AchievementMinecraft_1 : Achievement
{
    void Start()
    {
        GetComponents();

        // Only runs remaining code if the player has not already unlocked the achievement
        if (sm.achieveDict.ContainsKey(achieveID) && !sm.achieveDict[achieveID].hasUnlocked)
        {
            // Gets remaining scripts
            GameManager gm = FindAnyObjectByType<GameManager>();

            // Checks sees which town the player has moved into
            switch (gm.prevChoice)
            {
                case "Minecraft_2":
                    PlayerPrefs.SetInt("Achievement - Minecraft_1 - Spawn", 1);
                    break;
                case "Minecraft_2_1":
                    PlayerPrefs.SetInt("Achievement - Minecraft_1 - IBR", 1);
                    break;
                case "Minecraft_2_1_1":
                    PlayerPrefs.SetInt("Achievement - Minecraft_1 - DLC Island", 1);
                    break;
                case "Minecraft_2_1_1_1":
                case "Minecraft_2_1_1_1_2":
                    PlayerPrefs.SetInt("Achievement - Minecraft_1 - Furry Town", 1);
                    break;
            }
            // Ensures the value gets the saved across iterations
            PlayerPrefs.Save();

            // If the player meets the conditions for the achievement
            if (PlayerPrefs.GetInt("Achievement - Minecraft_1 - Spawn", 0) == 1 && 
                PlayerPrefs.GetInt("Achievement - Minecraft_1 - IBR", 0) == 1 &&
                PlayerPrefs.GetInt("Achievement - Minecraft_1 - DLC Island", 0) == 1 &&
                PlayerPrefs.GetInt("Achievement - Minecraft_1 - Furry Town", 0) == 1)
            {
                StartCoroutine(AchievePopupDelay(sm.achieveDict[achieveID]));
            }
        }
    }
}
