using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Video;

public class SubtitlesManager : MonoBehaviour
{
    public bool subtitlesEnabled;
    [SerializeField]
    TMP_Text subtitlesText;
    CanvasGroup subtitlesCg;
    [SerializeField]
    LayoutElement subtitlesLayout;
    RectTransform subtitlesRect;
    [SerializeField]
    private VideoPlayer videoPlayer;
    public SubtitleInfo currentSubtitles;
    public SubtitleEntry currentEntry;
    
    void Start()
    {
        subtitlesCg = GetComponent<CanvasGroup>();
        subtitlesRect = subtitlesLayout.GetComponent<RectTransform>();
    }

    void FixedUpdate()   
    {
        // Skips doing subtitle logic if not enabled, the vid is done, or if there are not subtitles for the vid
        if (!subtitlesEnabled || !videoPlayer.isPlaying || currentSubtitles == null || currentSubtitles.subtitleEntries.Count == 0) 
        { 
            if (subtitlesCg.alpha != 0)
            {
                subtitlesCg.alpha = 0;
                currentEntry = null;
            }
            return; 
        }
        

        SubtitleEntry entry = GetEntryAtTime((float)videoPlayer.time);

        if (entry != null)
        { 
            // Only update if subtitle has changed and the video is progressing normally
            if (entry == currentEntry) 
                return;

            if (subtitlesCg.alpha != 1)
                subtitlesCg.alpha = 1;

            currentEntry = entry;
            UpdateSubtitles(entry?.text);
        }
        else
        {
            subtitlesCg.alpha = 0;
        }
    }

    /// <summary>
    /// Finds the subtitle entry that matches the current video time,
    /// returns null if no subtitle should be displayed
    /// </summary>
    SubtitleEntry GetEntryAtTime(float time)
    {
        foreach (SubtitleEntry entry in currentSubtitles.subtitleEntries)
        {
            if (time >= entry.displayStartTime && time <= entry.displayEndTime)
                return entry;
        }
        return null;
    } 

    /// <summary>
    /// Updates subtitles
    /// </summary>
    void UpdateSubtitles(string text)
    {
        if (text == null) return;
        subtitlesText.text = text;

        subtitlesLayout.enabled = subtitlesText.preferredWidth > subtitlesLayout.preferredWidth ? true : false;    
        // Updates layout to ensure no formatting errors
        LayoutRebuilder.ForceRebuildLayoutImmediate(subtitlesRect);
    }
}
