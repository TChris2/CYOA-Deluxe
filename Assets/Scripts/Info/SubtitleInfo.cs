using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores subtitle info for a vid
/// </summary>
[CreateAssetMenu(fileName = "SubtitleInfo", menuName = "Scriptable Objects/SubtitleInfo")]
public class SubtitleInfo : ScriptableObject
{
    [Tooltip("List of subtitle entries for a video")]
    public List<SubtitleEntry> subtitleEntries;
}

/// <summary>
/// Stores text info for subtitles
/// </summary>
[System.Serializable]
public class SubtitleEntry
{
    [Tooltip("Time in seconds when this subtitle appears")]
    public float displayStartTime;
    [Tooltip("Time in seconds when this subtitle disappears")]
    public float displayEndTime;
    [Tooltip("Dialogue line shown on screen")]
    public string text;
}