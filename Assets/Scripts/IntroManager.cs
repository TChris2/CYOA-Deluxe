using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class IntroManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlay;
    // Skips to title screen
    [SerializeField]
    private Animator fadeTextAni;
    [SerializeField]
    private Animator titleScreenAni;
    TMP_Text skipText;
    [SerializeField]
    private CanvasGroup intro;
    public float skipTime = 72.5f;

    void Start()
    {
        // Enables the intro screen
        intro.alpha = 1;
        intro.blocksRaycasts = true;

        skipText = fadeTextAni.GetComponent<TMP_Text>();
        videoPlay.loopPointReached += TitleScreen;

        // If player is returning to title screen
        if (PlayerPrefs.GetInt("Skip Intro", 0) == 1)
            SkipVidTime(skipTime);
    }

    // Skips the intro
    public void Skip()
    {
        // Debug.Log("Skip()");
        // Debug.Log($"videoPlay.isPlaying {videoPlay.isPlaying}");
        if (videoPlay.isPlaying && videoPlay.time < skipTime)
        {
            // If the skip text is visable on screen
            if (skipText.color.a == 0)
            {
                // Debug.Log($"Skip - Text Popup");
                fadeTextAni.Play("Fade In");
            }
            // Skips if the player presses the skip button while the text is onscreen
            else
            {
                // Debug.Log($"Skip - SkipVidTime");
                SkipVidTime(skipTime);
            }
        }
    }

    // Pops up title screen after playing intro
    void TitleScreen(VideoPlayer vp)
    {
        StartCoroutine(PopUpTitleScreen());
    }

    IEnumerator PopUpTitleScreen()
    {
        yield return new WaitForSeconds(.1f);

        intro.alpha = 0;
        intro.blocksRaycasts = false;

        titleScreenAni.Play("Intro Start");
    }

    // Skips to the selected timestamp in the vid
    void SkipVidTime(float timestamp)
    {
        // Debug.Log("Skipping time in vid");

        // Disables text
        fadeTextAni.Play("Invisible Text");
        // Sets time in the vid
        videoPlay.time = timestamp;
    }

    void OnApplicationQuit()
    {
        // Resets Skip Intro's value
        PlayerPrefs.SetInt("Skip Intro", 0);
        PlayerPrefs.Save();
    }
}
