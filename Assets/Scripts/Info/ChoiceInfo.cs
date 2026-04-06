using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// Stores choice info
[CreateAssetMenu(fileName = "ChoiceInfo", menuName = "Scriptable Objects/ChoiceInfo")]
public class ChoiceInfo : ScriptableObject
{
    // Base choice info
    [Header("Choice Info")]
    [Tooltip("Id for the choice")]
    public string choiceID;
    [Tooltip("Choice")]
    public string choice;
    [Tooltip("Vid associated with choice")]
    public VideoClip vid;
    [Tooltip("State of choice")]
    public List<ChoiceState> choiceState;
    [Tooltip("Time when the retry menu pops up if the choice is a gameover or ending")]
    public float vidEndTime;
    [Tooltip("Whether the player has done the choice")]
    public bool hasComplete;
    [Tooltip("Objects spawned during the choice")]
    public List<ObjectInfo> objs;
    // Map menu info
    [Header("Map Menu")]
    [Tooltip("Name of the choice displayed in the map menu\nDefaults to the choice string if left empty")]
    public string mapName;
    [Tooltip("Screenshot of the video which will be displayed in the map menu")]
    public Sprite thumbnail;
    [Header("Stat Tracking")]
    [Tooltip("Ids of next choices the player can make from the current choice\nUsed to display completed choices in the map menu")]
    public List<ChoiceInfo> nextChoices;
    [Tooltip("Ids of achievements related to the choice\nCounts toward choice completion when displayed in the map menu")]
    public List<AchievementInfo> achievements;
    [Tooltip("Ids of letters related to the choice\nCounts toward choice completion when displayed in the map menu")]
    public List<LetterID> letterIDs;
    [Tooltip("Ids of weapons used during the choice")]
    public List<string> weaponsUsed;
    // Flags whether the info display in its menu needs to be updated
    [HideInInspector]
    public bool updateDisplay = true;

    // Adds info to new instance of ChoiceInfo
    public void AddInfo(ChoiceInfo choiceInfo)
    {
        choiceID = choiceInfo.choiceID;
        choice = choiceInfo.choice;
        vid = choiceInfo.vid;
        choiceState = choiceInfo.choiceState;
        vidEndTime = choiceInfo.vidEndTime;
        hasComplete = choiceInfo.hasComplete;
        objs = choiceInfo.objs;
        mapName = choiceInfo.mapName;
        thumbnail = choiceInfo.thumbnail;
        nextChoices = choiceInfo.nextChoices;
        achievements = choiceInfo.achievements;
        letterIDs = choiceInfo.letterIDs;
        weaponsUsed = choiceInfo.weaponsUsed;
    }

    // Trims strings of empty space    
    void OnValidate()
    {
        choiceID.Trim();
        choice.Trim();
        mapName.Trim();
        if (weaponsUsed.Count != 0)
            for (int i = 0; i < weaponsUsed.Count; i++)
                weaponsUsed[i].Trim();
    }
}

// Stores info on buttons spawned during the choice vid
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

// Simplified version of the class used to save the vital information to json
[System.Serializable]
public class ChoiceSaveData
{
    public string choiceID;
    public bool hasComplete;

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