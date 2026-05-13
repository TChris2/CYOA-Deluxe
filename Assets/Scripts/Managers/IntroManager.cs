using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

/// <summary>
/// Manages intro on Title Screen
/// </summary>
public class IntroManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlay;
    // Vids which play when the scene is first loaded
    [SerializeField]
    private VideoClip[] startVids;
    [SerializeField]
    private Sprite[] startFrames;
    [SerializeField]
    private SubtitleInfo startSubtitles;
    // Skips to title screen
    [SerializeField]
    private Animator fadeTextAni;
    [SerializeField]
    private Animator titleScreenAni;
    [SerializeField]
    private CanvasGroup intro;
    // Cover prev frame of the rendertexture until the vid is ready
    Image vidCover;
    TMP_Text skipText;
    public float skipTime = 72.5f;
    bool isSkippable;
    TransitionManager tm;
    SubtitlesManager subtitlesManager;

    void Start()
    {
        tm = FindAnyObjectByType<TransitionManager>();
        subtitlesManager = FindAnyObjectByType<SubtitlesManager>();
        // Enables the intro screen
        intro.alpha = 1;
        intro.blocksRaycasts = true;
        vidCover = intro.GetComponentInChildren<Image>();
        vidCover.enabled = true;

        skipText = fadeTextAni.GetComponent<TMP_Text>();

        videoPlay.loopPointReached += TitleScreen;
        videoPlay.prepareCompleted += PlayVid;

        if (tm.isTransitioning)
            tm.EndTransition();

        int i = PlayerPrefs.GetInt("Skip Intro", 0);
        isSkippable = i == 0 ? true : false;
        (videoPlay.clip, vidCover.sprite) = (startVids[i], startFrames[i]);
        subtitlesManager.currentSubtitles = i == 0 ? startSubtitles : null;
        videoPlay.time = 0;

        videoPlay.Prepare();
    }

    void PlayVid(VideoPlayer vp)
    {
        vidCover.enabled = false;
        // Debug.Log("Playing vid");
        vp.Play();
    }

    // Skips the intro
    public void Skip()
    {
        // Debug.Log("Skip()");
        // Debug.Log($"videoPlay.isPlaying {videoPlay.isPlaying}");
        if (isSkippable && videoPlay.isPlaying && videoPlay.time < skipTime)
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
        isSkippable = false;
        if (skipText.color.a != 0)
            fadeTextAni.Play("Invisible Text");
        StartCoroutine(PopUpTitleScreen());
    }

    IEnumerator PopUpTitleScreen()
    {
        yield return new WaitForSeconds(.1f);

        intro.alpha = 0;
        intro.blocksRaycasts = false;

        tm.fadeDuration = 2f;
        tm.fadeStartDelay = 1f;
        tm.onTransition += () => titleScreenAni.Play("Intro");
        tm.onTransition += tm.EndTransition;
        tm.FadeIn(FadeType.PlainBlack);
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
