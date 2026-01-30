using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Intro : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlay;
    // Skips to title screen
    [SerializeField]
    private Animator fadeTextAni;
    TMP_Text skipText;
    [SerializeField]
    private CanvasGroup intro;

    void Start()
    {
        intro.alpha = 1;
        intro.blocksRaycasts = true;

        skipText = fadeTextAni.GetComponent<TMP_Text>();

        videoPlay.loopPointReached += TitleScreen;
    }

    public void Skip()
    {
        // Debug.Log("Skip()");
        // Debug.Log($"videoPlay.isPlaying {videoPlay.isPlaying}");
        if (videoPlay.isPlaying)
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
                SkipVidTime(9999);
            }
        }
    }

    void TitleScreen(VideoPlayer vp)
    {
        StartCoroutine(PopUpTitleScreen());
    }

    IEnumerator PopUpTitleScreen()
    {
        yield return new WaitForSeconds(1f);

        intro.alpha = 0;
        intro.blocksRaycasts = false;
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
}
