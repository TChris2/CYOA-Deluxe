using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class FinaleManager : MonoBehaviour
{
    [Header("Finale Unlock Screen")]
    // Checks to see if the finale screen has been opened by the player during this session
    [SerializeField]
    private bool unlockScreenOpened;
    // The unlock screen to the finale
    [SerializeField]
    private CanvasGroup finaleUnlockScreen;
    // Stores letters
    [SerializeField]
    private GameObject letterIconsGroup;
    Animator[] letterIcons;
    // Plays finale transition
    [SerializeField]
    private GameObject finaleTransition;
    [SerializeField]
    private TMP_Text finaleUnlockLabel;
    [SerializeField]
    private VideoPlayer finaleUnlockVp;
    private CanvasGroup finaleUnlockVpCg;
    [SerializeField]
    private VideoClip[] finaleUnlockTransitionVids;
    LetterInfo currentUnlockedLetter;
    GameObject currentUnlockedLetterIcon;
    GameObject currentUnlockedLetterParticles;
    [SerializeField]
    private int letterCount;
    [Header("Finale")]
    [SerializeField]
    private VideoClip[] finaleVids;
    [SerializeField]
    private SubtitleInfo[] finaleSubtitles;
    public bool isFinale;
    [Header("Finale Stats Page")]
    [SerializeField]
    private Animator statsAni;
    [SerializeField]
    private List<TMP_Text> statsText;
    [SerializeField]
    private AudioSource statsAudioSource;
    // Scripts
    SaveManager sm;
    GameManager gm;
    SubtitlesManager subtitlesManager;
    InputMenu iMenu;
    VideoPlayer videoPlayer;

    #region General Stuff
    void Awake()
    {
        gm = GetComponent<GameManager>();
        finaleUnlockVpCg = finaleUnlockVp.GetComponent<CanvasGroup>();
        subtitlesManager = FindAnyObjectByType<SubtitlesManager>();

        videoPlayer = gm.videoPlayer;
        finaleUnlockVp.prepareCompleted += gm.PlayVid;
    }

    void Start()
    {
        sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();
    }

    /// <summary>
    /// Resets Finale Unlock variables
    /// </summary>
    public void ResetFinaleVars()
    {
        unlockScreenOpened = false;
        letterCount = 0;

        foreach(LetterInfo letter in sm.letterDict.Values)
        {
            if (letter.hasObtained)
                letterCount += 1;
        }
    }

    /// <summary>
    /// Logic for secret button to open Finale Unlock Screen
    /// </summary>   
    public void SecretButton(LetterInfo letter)
    {
        currentUnlockedLetter = letter;
        finaleUnlockVp.clip = finaleUnlockTransitionVids[0]; 
        finaleUnlockVp.time = 0;

        // Adds needed listeners
        finaleUnlockVp.prepareCompleted += FinaleUnlockScreenEnter;
        finaleUnlockVp.loopPointReached += DisplayFinaleUnlockScreen;

        Debug.Log($"{letter.letter}");
        finaleUnlockVp.Prepare();
    }

    #endregion
    #region Finale Unlock Screen

    /// <summary>
    /// Plays transition to Finale Unlock Screen
    /// </summary>    
    void FinaleUnlockScreenEnter(VideoPlayer vp)
    {   
        // Pauses the current video
        videoPlayer.Pause();

        Debug.Log("FinaleUnlockScreenTransition");

        // Activates the screen
        finaleUnlockVpCg.alpha = 1;
        finaleUnlockScreen.alpha = 1;
        finaleUnlockScreen.blocksRaycasts = true;

        StartCoroutine(LoadFinaleUnlockLetters());
    }

    /// <summary>
    /// Plays transition out of Finale Unlock Screen
    /// </summary>    
    void FinaleUnlockScreenExit(VideoPlayer vp)
    {   
        finaleUnlockVpCg.alpha = 1;
    }

    /// <summary>
    /// Loads letters in the bg as the vid transition plays
    /// </summary>
    IEnumerator LoadFinaleUnlockLetters()
    {
        LetterInfo letter;

        // Gets letter icons
        if (letterIcons == null)
            letterIcons = letterIconsGroup.GetComponentsInChildren<Animator>(true);

        // Reactivates the icon group if the group object has been deactviated if the player has reset is save progress
        if (letterCount != sm.letterDict.Count && !letterIconsGroup.activeSelf)
            letterIconsGroup.SetActive(true);

        foreach (Animator letterIcon in letterIcons)
        {
            // Debug.Log(letterIcon.name);

            GameObject letterParticles = letterIcon.transform.Find("Particles").gameObject;

            // Stores unlocked icon
            if (letterIcon.name == currentUnlockedLetter.letterID.ToString())
            {
                currentUnlockedLetterIcon = letterIcon.gameObject;
                currentUnlockedLetterParticles = letterParticles;
            }
            // Checks to see which letters have already been obtained
            if (!unlockScreenOpened && LetterID.TryParse(letterIcon.name, true, out LetterID id) &&
                sm.letterDict.TryGetValue(id, out letter))
            {
                // Enables the letter if it has already been obtained without entry animation
                if (letter.hasObtained)
                {
                    // Debug.Log($"LetterID {letter.letterID} - {letter.letter} has already been obtained, activating object");
                    letterIcon.gameObject.SetActive(true);
                    letterParticles.SetActive(false);

                    // Starts bob animation at a random point so they are all not synchronized & disables particles system
                    letterIcon.GetComponent<Animator>().Play("Letter Bob", 0, UnityEngine.Random.value);
                    letterCount += 1;
                }
                // Disables letter icon if it has not already been obtained
                else
                {
                    // Debug.Log($"LetterID {letter.letterID} - {letter.letter} has not been obtained, deactivating object");
                    letterIcon.gameObject.SetActive(false); 
                }
            }
        }

        yield return null;
    }

    /// <summary>
    /// Wrapper to start FinaleUnlockScreen 
    /// </summary>
    void DisplayFinaleUnlockScreen(VideoPlayer vp)
    {
        finaleUnlockVpCg.alpha = 0;
        StartCoroutine(FinaleUnlockScreen());
    }

    /// <summary>
    /// Displays finale unlock screen
    /// </summary>
    IEnumerator FinaleUnlockScreen()
    {
        Debug.Log($"Opening Finale unlock screen with letter {currentUnlockedLetter.letterID} - {currentUnlockedLetter.letter}");

        // Removes previous listeners
        finaleUnlockVp.prepareCompleted -= FinaleUnlockScreenEnter;
        finaleUnlockVp.loopPointReached -= DisplayFinaleUnlockScreen;

        // If it is the first time the player activates the finale unlock screen
        if (!unlockScreenOpened)
        {
            unlockScreenOpened = true;
        }

        // Activates the newly unlocked letter
        yield return new WaitForSeconds(1.5f);
        // Debug.Log("Enabling Object");

        currentUnlockedLetterIcon.SetActive(true); 
        currentUnlockedLetterParticles.SetActive(true);
        currentUnlockedLetter.hasObtained = true;
        letterCount += 1;

        yield return new WaitForSeconds(10f);

        // If the player has not collected every letter
        if (letterCount < sm.letterDict.Count)
        {
            StartCoroutine(TextPopIn($"<size=280>{sm.letterDict.Count - letterCount} Remain", .3f));

            yield return new WaitForSeconds(8f);

            StartCoroutine(TextPopIn($"<size=200><i>The Finale Awaits", .05f));

            yield return new WaitForSeconds(6f);

            finaleUnlockVp.prepareCompleted += FinaleUnlockScreenExit;
            finaleUnlockVp.loopPointReached += CloseFinaleUnlockScreen;

            finaleUnlockVp.clip = finaleUnlockTransitionVids[1];
            finaleUnlockVp.time = 0;

            finaleUnlockVp.Prepare();
        }
        // If the player has collected every letter
        else
        {
            // Turns true here so if the player closes the game during the finale they don't get softlocked
            sm.choiceDict["Finale_"].hasComplete = true;

            // Gets letter icons
            if (letterIcons == null)
                letterIcons = letterIconsGroup.GetComponentsInChildren<Animator>(true);

            foreach (Animator letterIcon in letterIcons)
                letterIcon.Play("Letter Glow");
        
            yield return new WaitForSeconds(5f);

            foreach (Animator letterIcon in letterIcons)
            {
                if (letterIcon.gameObject.activeSelf)
                {
                    letterIcon.Play("Letter White");
                    letterIcon.Play("Letter Snap");
                }
            }

            yield return new WaitForSeconds(1.5f);

            // Disables main group of letters
            letterIconsGroup.SetActive(false);

            // Plays finale unlocking animation
            finaleTransition.SetActive(true);
        }
    }

    /// <summary>
    /// Closes finale unlock screen and returns the player to the main game
    /// </summary>
    void CloseFinaleUnlockScreen(VideoPlayer vp)
    {
        // Returns player back to main game and unpauses the video player
        finaleUnlockScreen.alpha = 0;
        finaleUnlockScreen.blocksRaycasts = false;
        finaleUnlockVpCg.alpha = 0;

        finaleUnlockLabel.maxVisibleCharacters = 0;
        currentUnlockedLetter = null;
        currentUnlockedLetterIcon = null;
        currentUnlockedLetterParticles = null;

        finaleUnlockVp.prepareCompleted -= FinaleUnlockScreenExit;
        finaleUnlockVp.loopPointReached -= CloseFinaleUnlockScreen;

        // Plays the remaining video if it has not already finished
        if (videoPlayer.time < videoPlayer.length - .2f)
            videoPlayer.Play();
    }

    /// <summary>
    /// Pops Finale Unlock Screen text onscreen
    /// </summary>
    IEnumerator TextPopIn(string text, float delay)
    {
        finaleUnlockLabel.maxVisibleCharacters = 0;

        finaleUnlockLabel.text = text;

        finaleUnlockLabel.ForceMeshUpdate();

        for (int i = 0; i < finaleUnlockLabel.textInfo.characterCount; i++)
        {
            // Reveals current character
            finaleUnlockLabel.maxVisibleCharacters += 1;

            // Skips delay for spaces
            if (finaleUnlockLabel.textInfo.characterInfo[i].character == ' ')
                continue;

            yield return new WaitForSeconds(delay);
        }
    }

    #endregion
    #region Finale

    /// <summary>
    /// Starts finale from finaleTransition Animator
    /// </summary>
    public void StartFinale()
    {
        // Debug.Log("Finale Started");
        videoPlayer.Stop();

        // If the user is in a retry menu when it starts
        iMenu.CloseRetryMenu();
        
        videoPlayer.prepareCompleted += EndFinaleTransition;
        PlayFinale();
    }

    void EndFinaleTransition(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= EndFinaleTransition;

        finaleUnlockScreen.alpha = 0;
        finaleUnlockScreen.blocksRaycasts = false;
        finaleTransition.SetActive(false);
    }

    /// <summary>
    /// Plays first part of the finale
    /// </summary>
    public void PlayFinale()
    {
        isFinale = true;
        gm.ClearPreviousInfo();
        videoPlayer.clip = finaleVids[0];
        subtitlesManager.currentSubtitles = finaleSubtitles[0];
        videoPlayer.time = 0;
        // Displays stats after video finishes
        videoPlayer.loopPointReached += PopUpFinaleStats;
        videoPlayer.prepareCompleted -= gm.LoadVidInfo;
        videoPlayer.Prepare();
    }
    
    /// <summary>
    /// Starts the pop up of the finale stats after playing the first part of the finale
    /// </summary>
    void PopUpFinaleStats(VideoPlayer vp)
    {
        // Removes listener after it is done
        videoPlayer.loopPointReached -= PopUpFinaleStats;
        
        StartCoroutine(ShowFinaleStats());
    }
    
    /// <summary>
    /// Shows the finale stats on screen
    /// </summary>
    IEnumerator ShowFinaleStats()
    {
        // Debug.Log("Show Stats");
        // Marks the finale as complete
        gm.GeneralAchievementsCheck();

        // Popups achievement for beating the finale
        gm.CheckAchievement("General_6", () => true);

        // Displays stats
        sm.stats.DisplayStatsAll(statsText, sm);

        yield return new WaitForSeconds(2f);

        // Plays stats fade in animation
        statsAni.Play("Fade In");
        statsAudioSource.Play();
    }
    
    /// <summary>
    /// Closes stat screen from next button on the stats screen
    /// </summary>
    public void CloseFinaleStats()
    {
        statsAni.Play("Fade Out");
        
        StartCoroutine(PlayPostCredits());
    }

    /// <summary>
    /// Plays the post credits scene
    /// </summary>
    IEnumerator PlayPostCredits()
    {
        // Debug.Log("Playing Post Credits");
        yield return new WaitForSeconds(3f);
        statsAudioSource.Stop();
        videoPlayer.clip = finaleVids[1];
        subtitlesManager.currentSubtitles = finaleSubtitles[1];
        videoPlayer.time = 0;
        // Returns player to the title screen after post credits scene finishes
        videoPlayer.loopPointReached += ReturnToTitleScreen;
        
        videoPlayer.Prepare();
    }

    /// <summary>
    /// Returns to the title screen after playing the post credits scene
    /// </summary>
    void ReturnToTitleScreen(VideoPlayer vp)
    {
        // Debug.Log("Returning to title screen");
        SceneManager.LoadScene("Title Screen");
        // Resets Skip Intro's value
        PlayerPrefs.SetInt("Skip Intro", 1); 
        PlayerPrefs.Save();
    }

    #endregion
}
