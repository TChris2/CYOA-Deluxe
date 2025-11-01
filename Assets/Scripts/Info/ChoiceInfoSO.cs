using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// Stores choice info in scriptable object format
[CreateAssetMenu(fileName = "ChoiceInfoSO", menuName = "Scriptable Objects/ChoiceInfoSO")]
public class ChoiceInfoSO : ScriptableObject
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
}