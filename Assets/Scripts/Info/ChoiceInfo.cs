using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Stores choice info
/// </summary>
[CreateAssetMenu(fileName = "ChoiceInfo", menuName = "Scriptable Objects/ChoiceInfo")]
public class ChoiceInfo : ScriptableObject
{
    // Base choice info
    [Header("Choice Info")]
    [Tooltip("Id for the choice")]
    public string choiceID;
    [Tooltip("Selected Choice")]
    public string choice;
    [Tooltip("Whether the player has done the choice")]
    public bool hasComplete;
    [Tooltip("If the choice only appears on the player's first run through a choice")]
    public bool firstRunOnly;
    [Tooltip("State of choice")]
    public List<ChoiceState> choiceState;
    [Tooltip("Vid associated with choice")]
    [Header("Vid Info")]
    public VideoClip vid;
    [Tooltip("Subtitles for the vid")]
    public SubtitleInfo subtitles;
    [Tooltip("Time when the retry menu pops up if the choice is a gameover or ending")]
    public float vidEndTime;
    [Tooltip("Objects spawned during the choice")]
    public List<ObjectInfo> objs;
    // Map menu info
    [Header("Map Menu")]
    [Tooltip("If the choice is on the map")]
    public bool isOnMap;
    [Tooltip("Name of the choice displayed in the map menu\nDefaults to the choice string if left empty")]
    public string mapName;
    [Tooltip("Screenshot of the video which will be displayed in the map menu")]
    public Sprite thumbnail;
    [Tooltip("When on the map menu you want your choice to shown as on a different one")]
    public ChoiceInfo mapDisplayChoice;
    // Stores next choices on the maps
    [HideInInspector]
    public List<ChoiceInfo> mapNextChoices;
    [Header("Stat Tracking")]
    [Tooltip("Ids of next choices the player can make from the current choice\nUsed to display completed choices in the map menu")]
    public List<ChoiceInfo> nextChoices;
    [Tooltip("Ids of achievements related to the choice\nCounts toward choice completion when displayed in the map menu")]
    public List<AchievementInfo> achievements;
    public List<AchievementInfo> achievementHints;
    [Tooltip("Ids of letters related to the choice\nCounts toward choice completion when displayed in the map menu")]
    public List<LetterID> letterIDs;
    [Tooltip("Ids of weapons used during the choice")]
    public List<string> weaponsUsed;
    // Flags whether the info display in its menu needs to be updated
    [HideInInspector]
    public bool updateDisplay = true;

    /// <summary>
    /// Adds info to new instance of ChoiceInfo
    /// </summary>
    public void AddInfo(ChoiceInfo choiceInfo)
    {
        choiceID = choiceInfo.choiceID;
        choice = choiceInfo.choice;
        hasComplete = choiceInfo.hasComplete;
        firstRunOnly = choiceInfo.firstRunOnly;
        choiceState = choiceInfo.choiceState;
        vid = choiceInfo.vid;
        subtitles = choiceInfo.subtitles;
        vidEndTime = choiceInfo.vidEndTime;
        objs = choiceInfo.objs;
        mapName = choiceInfo.mapName;
        thumbnail = choiceInfo.thumbnail;
        isOnMap = thumbnail != null;
        mapDisplayChoice = choiceInfo.mapDisplayChoice;
        mapNextChoices = new List<ChoiceInfo>();
        nextChoices = choiceInfo.nextChoices;
        achievements = choiceInfo.achievements;
        achievementHints = choiceInfo.achievementHints;
        letterIDs = choiceInfo.letterIDs;
        weaponsUsed = choiceInfo.weaponsUsed;
        updateDisplay = choiceInfo.updateDisplay;
    }

    // Trims strings of empty space    
    void OnValidate()
    {
        choiceID = choiceID.Trim();
        choice = choice.Trim();
        mapName = mapName.Trim();
        if (weaponsUsed.Count != 0)
            for (int i = 0; i < weaponsUsed.Count; i++)
                weaponsUsed[i] = weaponsUsed[i].Trim();
    }
}

/// <summary>
/// Stores info on objects spawned during the choice vid
/// </summary>
[System.Serializable]
public class ObjectInfo
{
    [Tooltip("Object spawned")]
    public GameObject obj;
    [Tooltip("Time when the object will popup onscreen")]
    public float popupTime;
    [Tooltip("Determines if an object can be skipped or not when using Skip")]
    public bool isSkippable;
    [Tooltip("If the object does not appear on the player's first complete run and only in subsequent ones")]
    public bool subsequentRunsOnly;
    [Tooltip("Delay when child objects of the object popup onscreen between each other")]
    public float childPopupDelay;
    [Tooltip("Time when the object will despawn\nIf set 0 the object will not despawn")]
    public float despawnTime;
}

/// <summary>
/// Simplified version of the class used to save the vital information
/// </summary>
[System.Serializable]
public class ChoiceSaveData
{
    public string choiceID;
    public bool hasComplete;

    /// <summary>
    /// Adds info to new instance of ChoiceSaveData
    /// </summary>
    public ChoiceSaveData(string choiceID, bool hasComplete)
    {
        this.choiceID = choiceID;
        this.hasComplete = hasComplete;
    }
}

// State of choice
public enum ChoiceState
{
    // Default - Current choice leads to another set of choices
    Choice,
    ChoiceTimed,
    // Current choice leads to a game over
    GameOver,
    // Current choice leads to an ending
    Ending,
    // Not a proper choice, used primarily for tracking stats of ChoiceTimed do nothing options,
    // if choiceID with this state is loaded it will load the choice before it 
    Reference
}