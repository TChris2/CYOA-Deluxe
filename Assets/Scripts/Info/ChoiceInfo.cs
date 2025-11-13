using System.Collections;
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
    [Tooltip("Ids of next choices the player can make from the current choice\nUsed to display completed choices in the map menu")]
    public List<string> nextChoiceIDs;
    [Tooltip("Ids of achievements related to the choice\nCounts toward choice completion when displayed in the map menu")]
    public List<string> achieveIDs;
    // Flags whether the info display in its menu needs to be updated
    [HideInInspector]
    public bool updateDisplay = true;
}

// Stores info on buttons spawned during the choice vid
[System.Serializable]
public class ObjectInfo
{
    [Tooltip("Object spawned")]
    public GameObject obj;
    [Tooltip("Time when the object will popup onscreen")]
    public float popupTime;
    [Tooltip("Type of object")]
    public ObjectType objType;
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
    public string choice;
    public string mapName;
    public bool hasComplete;

    public ChoiceSaveData(string choiceID, string choice, string mapName, bool hasComplete)
    {
        this.choiceID = choiceID;
        this.choice = choice;
        this.mapName = mapName;
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
    Ending
}

// Type of object
public enum ObjectType
{
    // Default - Choice btn
    ChoiceBtn,
    // Hidden Btn
    SecretBtn,
    // Miscellaneous Objects
    Other,
}