using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Video;

// Stores choice info
[System.Serializable]
public class ChoiceInfo
{
    [Tooltip("Id for the choice")]
    public ChoiceID choiceID;
    [Tooltip("Choice")]
    public string choice;
    [Tooltip("Vid assciated with choice")]
    public VideoClip vid;
    [Tooltip("Shot of the video which will be displayed in the map menu")]
    public Sprite thumbnail;
    [Tooltip("State of choice")]
    public ChoiceState choiceState;
    [Tooltip("Time when the retry menu pops up if the choice is a gameover or ending")]
    public float vidEndTime;
    [Tooltip("Whether the player has done the choice")]
    public bool hasComplete;
    [Tooltip("Ids of next choices the player can make from the current choice")]
    public ChoiceID[] nextChoiceIDs;
    [Tooltip("Objects spawned during the choice")]
    public ObjectInfo[] objs;
    [Tooltip("Ids of any achievments related to the choice")]
    public AchievementID[] achieveIDs;

    // Checks if the entry already has its vid assigned to it or not
    public bool CheckVid() { if (vid) { return true; } else { return false; } }

    // Sets vid to the entry
    public void SetVid(VideoClip newObj) { vid = newObj; }

    // Exports vid to be used in scripts
    public VideoClip ExportVid() { return vid; }

    public ChoiceInfo(ChoiceID choiceID, string choice, VideoClip vid, Sprite thumbnail, ChoiceState choiceState, float vidEndTime, bool hasComplete, ChoiceID[] nextChoiceIDs,
        ObjectInfo[] objs)
    {
        this.choiceID = choiceID;
        this.choice = choice;
        this.vid = vid;
        this.thumbnail = thumbnail;
        this.choiceState = choiceState;
        this.vidEndTime = vidEndTime;
        this.hasComplete = hasComplete;
        this.nextChoiceIDs = nextChoiceIDs;
        this.objs = objs;
    }
}

// Stores info on buttons spawned during the choice vid
[System.Serializable]
public class ObjectInfo
{
    // The object spawned
    [Tooltip("Object spawned")]
    public GameObject obj;
    [Tooltip("Time when the object will popup onscreen")]
    public float popupTime;
    [Tooltip("Type of object")]
    public ObjectType objType;
    [Tooltip("Delay when child objects of the object popup onscreen between each other")]
    public float childPopupDelay;
    [Tooltip("Time when the object will despawn, if it is set 0 the object will not despawn")]
    public float despawnTime;
}

// Simplified version of the class used to save the vital information to json
[System.Serializable]
public class ChoiceSaveData
{
    public ChoiceID choiceID;
    public string choice;
    public bool hasComplete;

    public ChoiceSaveData(ChoiceID choiceID, string choice, bool hasComplete)
    {
        this.choiceID = choiceID;
        this.choice = choice;
        this.hasComplete = hasComplete;
    }
}

// State of choice
public enum ChoiceState
{
    // 0 - Current choice leads to another set of choices
    Choice,
    ChoiceTimed,
    // 1 - Current choice leads to a game over
    GameOver,
    // 2 - Current choice leads to an ending
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
    Miscellaneous,
}