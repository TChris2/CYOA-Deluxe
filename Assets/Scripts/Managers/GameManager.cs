using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages main game logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Main Game")]
    // Video player
    public VideoPlayer videoPlayer;
    [Tooltip("Checks to see if the videoplayer should be paused atm")]
    public bool pauseVideoPlayer;
    // Displays current choice
    public ChoiceInfo currentChoice;
    // Store prev choice id
    public string prevChoice;
    // Determines when the game can be paused in normal gameplay
    public bool canBePaused;
    public bool vidFinished;
    // Skipping in vids
    [SerializeField]
    public Animator fadeTextAni;
    [HideInInspector]
    public TMP_Text skipText;
    [SerializeField]
    private Transform objHolder;
    // Used to determine when a choice is visable on screen
    [HideInInspector]
    public bool choiceVisable;
    // If enabled skips directly to the choice in LoadChoice()
    [HideInInspector]
    public bool isSkipping;
    // When enabled prevents certain info from being stored when skipping in debug mode
    bool isDebugSkipping;
    // Scripts
    InputMenu iMenu;
    SaveManager sm;
    TransitionManager tm;
    AchievementManager am;
    FinaleManager fm;
    SubtitlesManager subtitlesManager;
    MapMenu mapMenuF;
    List<Coroutine> coroutines = new List<Coroutine>();
    [Header("Debug Menu")]
    [SerializeField]
    private TMP_InputField timeInput;
    [SerializeField]
    private TMP_InputField testInput;
    [SerializeField]
    private TMP_Text currentTime;
    
    #region Main Game
    void Start()
    {
        // Gets components
        sm = FindAnyObjectByType<SaveManager>();
        tm = FindAnyObjectByType<TransitionManager>();
        skipText = fadeTextAni.GetComponent<TMP_Text>();
        iMenu = FindAnyObjectByType<InputMenu>();
        mapMenuF = FindAnyObjectByType<MapMenu>();
        subtitlesManager = FindAnyObjectByType<SubtitlesManager>();
        am = FindAnyObjectByType<AchievementManager>();
        fm = GetComponent<FinaleManager>();
        videoPlayer.prepareCompleted += PlayVid;
        videoPlayer.prepareCompleted += LoadVidInfo;
        videoPlayer.loopPointReached += OnVidFinished;

        // Loads current choice id
        string id = PlayerPrefs.GetString("Current ChoiceID", "Start_");

        // Loads choice info
        LoadChoice(id);
    }

    void Update()
    {
        // Tracks player playtime while not paused
        if (!iMenu.isPaused)
            sm.stats.playTime += Time.deltaTime;
    }

    /// <summary>
    /// Resets Game Manager variables when debug LoadSOData is used
    /// </summary>
    public void ResetLocalVars()
    {
        fm.ResetFinaleVars();
        am.ClearPopupQueues();
    }

    /// <summary>
    /// Loads the player's previous choice
    /// </summary>
    public void LoadPrevChoice()
    {
        // Debug.Log($"GameManager: LoadPrevChoice - prevChoice {prevChoice.ToString()}"); 
        if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
        {
            if (prevChoice == null)
            {
                prevChoice = "Start_";
                Debug.LogError("GameManager: Error, prevChoice is null, defaulting to Start_");
            }

            LoadChoice(prevChoice.ToString());
        }
        else
            LoadChoice(currentChoice.choiceID.ToString());
    }

    /// <summary>
    /// Loads choice info
    /// </summary>
    public void LoadChoice(string id)
    {
        // Trims empty space from id
        id = id.Trim();

        canBePaused = false;

        Debug.Log($"GameManager: Loading Choice {id}");

        // Stores prev choice, only stores prev choice in normal gameplay
        if (!isDebugSkipping && currentChoice != null)
            prevChoice = currentChoice.choiceID;

        // Debug.Log($"GameManager: PrevChoice {prevChoice.ToString()}"); 

        // Disables at start of choice
        choiceVisable = false;

        // Stops vids
        videoPlayer.Stop();

        // Closes retry menu if it is still open
        if (iMenu.isRetryMenu)
            videoPlayer.prepareCompleted += CloseRetryMenu;
        // Closes the map menu when the vid is ready
        if (mapMenuF.inMapMenu)
            videoPlayer.prepareCompleted += CloseMapMenu;
        // Ends scene transition when the vid is ready
        if (tm.isTransitioning)
            videoPlayer.prepareCompleted += EndSceneTransition;

        // Attempts to get choice info
        if (sm.choiceDict.TryGetValue(id, out currentChoice))
        {
            // If the loaded choice a reference, it will load the choice before it
            if (currentChoice.choiceState.Contains(ChoiceState.Reference))
            {
                string[] parts = currentChoice.choiceID.Split('_');
                string prevChoiceID = string.Join("_", parts, 0, parts.Length - 1);

                if (sm.choiceDict.TryGetValue(prevChoiceID, out ChoiceInfo prevChoice))
                {
                    Debug.Log($"GameManager: Current choice {currentChoice.choiceID} is a reference, loading {prevChoice.choiceID} instead");
                    currentChoice = prevChoice;
                }
                else
                {
                    Debug.Log($"GameManager: Error previous choice of reference choice {currentChoice.choiceID} does not exist");
                }
            }

            currentChoice.hasComplete = true;
            if (currentChoice.mapDisplayChoice && !sm.choiceDict[currentChoice.mapDisplayChoice.choiceID].hasComplete)
            {
                Debug.Log($"GameManager: {currentChoice.choiceID} has {currentChoice.mapDisplayChoice.choiceID} as it's map display choice, marking {currentChoice.mapDisplayChoice.choiceID} as completed");
                sm.choiceDict[currentChoice.mapDisplayChoice.choiceID].hasComplete = true;
            }

            if (currentChoice.choiceID == "Finale_")
            {
                fm.PlayFinale();
                return;
            }

            if (!currentChoice.vid)
            {
                Debug.Log("GameManager: No video detected");
            }

            // Vid subtitles
            subtitlesManager.currentEntry = null;
            subtitlesManager.currentSubtitles = currentChoice.subtitles;

            // If the choice contains any achievemnts which have to do their logic
            if (currentChoice.achievements.Count > 0)
            {
                foreach (AchievementInfo achievement in currentChoice.achievements)
                {
                    Debug.Log($"GameManager: Checking achievement {achievement.achieveID}");
                    am.CheckAchievement(achievement.achieveID);
                }
            }
            else
            {
                // Debug.Log($"GameManager: No achievements found for ChoiceID {currentChoice.choiceID} in LoadChoice()");
            }

            // If the choice contains any achievement hints to see if has to update an achievement's achieveState
            if (currentChoice.achievementHints.Count > 0)
            {
                foreach (AchievementInfo achievementInfo in currentChoice.achievementHints)
                {
                    if (sm.achieveDict.ContainsKey(achievementInfo.achieveID))
                    {
                        AchievementInfo achievement = sm.achieveDict[achievementInfo.achieveID];
                        if (!achievement.hasUnlocked && achievement.achieveState == AchievementState.Locked)
                        {
                            Debug.Log($"GameManager: Achievement hint activated for {achievement.achieveID}");
                            achievement.achieveState = AchievementState.Shown;
                            achievement.updateDisplay = true;
                        }
                    }
                    else
                    {
                        // Debug.Log($"GameManager: AchieveID {id} not found in system in LoadChoice()");
                    }
                }
            }
            else
            {
                // Debug.Log($"GameManager: No achievement hints found for ChoiceID {currentChoice.choiceID} in LoadChoice()");
            }

            // If the choice contains any weapon stat tracking
            if (currentChoice.weaponsUsed.Count > 0)
            {
                foreach (string weapon in currentChoice.weaponsUsed)
                {
                    if (sm.stats.weaponDict.ContainsKey(weapon))
                    {
                        sm.stats.weaponDict[weapon] += 1;
                    }
                    else
                    {
                        // Debug.Log($"GameManager: Weapon {id} not found in system in LoadChoice(), creating new entry");
                        sm.stats.weaponDict.Add(weapon, 1);
                    }
                }
            }
            else
            {
                // Debug.Log($"GameManager: No weapons found for ChoiceID {currentChoice.choiceID} in LoadChoice()");
            }

            // Skips to first choice if enabled
            if (isSkipping)
                GetSkipTime(currentChoice);

            // Sets video to the player
            videoPlayer.clip = currentChoice.vid;
            videoPlayer.time = 0;

            videoPlayer.Prepare();
        }
        else
        {
            Debug.Log($"GameManager: ID - {id} - not found in the system when checking in LoadChoice()");
            LoadChoice("Start_");
        }
    }

    /// <summary>
    /// Loads the time based info such as objects when the video is ready
    /// </summary>
    public void LoadVidInfo(VideoPlayer vp)
    {
        // Debug.Log("GameManager: LoadVidInfo");
        // Clears any coroutines from the previous choice
        ClearPreviousInfo();

        // Opens Retry Menu variant of the pause menu at ending or gameover
        if (currentChoice.vidEndTime > 0)
            coroutines.Add(StartCoroutine(RetryMenuPopup(currentChoice.vidEndTime)));

        // The choice has any objects
        if (currentChoice.objs != null)
        {
            // Debug.Log($"GameManager: Loading {currentChoice.choice}'s objects");
            // Starts a coroutine for each object
            foreach (ObjectInfo obj in currentChoice.objs)
            {
                // Spawns object
                coroutines.Add(StartCoroutine(SpawnObject(obj)));
            }
        }
    }

    public void OnVidFinished(VideoPlayer vp)
    {
        vidFinished = true;
    }

    /// <summary>
    /// Plays video when the vid is ready
    /// </summary>
    public void PlayVid(VideoPlayer vp)
    {
        // Debug.Log("GameManager: Playing vid");
        pauseVideoPlayer = false;
        vidFinished = false;
        vp.Play();
    }

    /// <summary>
    /// Closes the retry menu when the vid is ready
    /// </summary>
    void CloseRetryMenu(VideoPlayer vp)
    {
        // Debug.Log("GameManager: Closing Retry menu");
        iMenu.CloseRetryMenu();
        vp.prepareCompleted -= CloseRetryMenu;
    }

    /// <summary>
    /// Closes the map menu when the vid is ready
    /// </summary>
    void CloseMapMenu(VideoPlayer vp)
    {
        // Debug.Log("GameManager: Closing Map menu");
        iMenu.CloseMenu();
        vp.prepareCompleted -= CloseMapMenu;
    }

    /// <summary>
    /// Closes the map menu when the vid is ready
    /// </summary>
    void EndSceneTransition(VideoPlayer vp)
    {
        tm.EndTransition();
        vp.prepareCompleted -= EndSceneTransition;
    }

    /// <summary>
    /// Displays current time in vid
    /// </summary>
    public IEnumerator GetVidTime()
    {
        yield return null;

        while (iMenu.completeOverride && videoPlayer != null)
        {
            currentTime.text = videoPlayer.time.ToString("0.00");
            yield return null;
        }
    }

    /// <summary>
    /// Popups retry menu
    /// </summary>
    private IEnumerator RetryMenuPopup(float timestamp)
    {
        // Debug.Log($"GameManager: Retry menu will popup in {timestamp}s");

        while (videoPlayer.time < timestamp)
        {
            // Debug.Log($"GameManager: {videoPlayer.time}");
            yield return null;
        }

        // Counts deaths if choice has ChoiceState GameOver
        if (currentChoice.choiceState.Contains(ChoiceState.GameOver))
        {
            Debug.Log($"GameManager: Death Detected in {currentChoice.choiceID}");
            sm.stats.deaths += 1;
        } 

        if (currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
        {
            if (sm.choiceDict.TryGetValue($"{currentChoice.choiceID}_1", out ChoiceInfo referenceChoice))
            {
                referenceChoice.hasComplete = true;
                if (referenceChoice.choiceState.Contains(ChoiceState.GameOver))
                {
                    // Debug.Log($"GameManager: Death Detected in reference choice {referenceChoice.choiceID}");
                    sm.stats.deaths += 1;
                }
            }
            else
            {
                // Debug.Log($"GameManager: No reference choice detected in {currentChoice.choiceID}");
            }
        }

        am.GeneralAchievementsCheck();
        am.LoadEndingAchievePopups();

        // Debug.Log("GameManager: Opening Retry Menu");
        iMenu.OpenRetryMenu();
    }

    #endregion
    #region Object Logic

    /// <summary>
    /// Spawns an object at its specific time in the vid
    /// </summary>
    public IEnumerator SpawnObject(ObjectInfo obj)
    {
        // Debug.Log("GameManager: In object spawn coroutine");

        // Skips spawning the object if they only appear in subsequent runs
        if (obj.subsequentRunsOnly && !sm.achieveDict["General_3"].hasUnlocked)
        {
            Debug.LogWarning("GameManager: Object only appears in subsequent runs, skipping spawning object");
            yield break;
        }

        // Spawns object
        GameObject gameObj = Instantiate(obj.obj, objHolder);

        // Disables every object in the parent spawned object
        foreach (Transform child in gameObj.transform)
        {
            // Debug.Log($"GameManager: {child.name} {child.tag}");
            // Adds function to the buttons
            switch (child.tag)
            {
                // Function for the choice buttons
                case "Choice Button":
                    Button choiceBtn = child.GetComponent<Button>();

                    string objName = choiceBtn.gameObject.name.Trim();
                    // Only checks objects with potential of being an id
                    if (objName.Contains("_"))
                    {
                        // Debug.Log($"GameManager: {objName} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                        if (sm.choiceDict.ContainsKey(objName))
                        {
                            string choiceIDString = objName;

                            if (sm.choiceDict[objName].firstRunOnly && sm.choiceDict[objName].hasComplete)
                                Destroy(choiceBtn.gameObject);

                            choiceBtn.onClick.AddListener(() => LoadChoice(choiceIDString));
                        }
                        else
                        {
                            Debug.Log($"GameManager: ID - {objName} - not found in the system when checking in SpawnObject()");
                        }
                    }
                    break;
                // Function for secret buttons
                case "Secret Button":
                    Button secretBtn = child.GetComponent<Button>();

                    // checks to see if letter has already been obtained
                    if (LetterID.TryParse(secretBtn.name, true, out LetterID id) &&
                        sm.letterDict.TryGetValue(id, out LetterInfo letter))
                    {
                        // Deletes object if it has already obtained
                        if (letter.hasObtained)
                        {
                            Debug.Log($"GameManager: LetterID {letter.letterID} - {letter.letter} has already been obtained, deleting object");
                            Destroy(gameObj);
                            yield break;
                        }
                        // Adds functionality unlock screen functionality
                        else
                        {
                            // Debug.Log($"GameManager: Adding LetterID {letter.letterID} - {letter.letter}");
                            secretBtn.onClick.AddListener(() => 
                            {
                                Destroy(gameObj);
                                fm.SecretButton(letter);
                            });   
                        }
                    }
                    else
                    {
                        Debug.Log($"GameManager: LetterID {secretBtn.name} not in system");
                    }
                    break;
            }

            child.gameObject.SetActive(false);
        }

        // Spawns the button after the video reaches it specific timestamp
        while (videoPlayer.time < obj.popupTime)
        {
            // Debug.Log($"GameManager: {videoPlayer.time}");
            yield return null;
        }

        // Displays object within timestamp
        if (videoPlayer.time <= obj.popupTime + 1)
        {
            if (!obj.isSkippable)
            {
                // Only does this logic once per loaded choice
                if (!choiceVisable)
                {
                    // Prevents skipping after the choice is on screen
                    choiceVisable = true;
                    
                    if (skipText.color.a != 0)
                        fadeTextAni.Play("Invisible Text");

                    // Allows the game to be paused when it reaches a non timed choice
                    if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
                        canBePaused = true;

                    am.GeneralAchievementsCheck();
                    am.LoadChoiceAchievePopups();
                }
            }

            foreach (Transform child in gameObj.transform)
            {
                child.gameObject.SetActive(true);

                yield return new WaitForSeconds(obj.childPopupDelay);
            }

            // If the object despawns
            if (obj.despawnTime != 0)
            {
                // Destroys object after its despawn time is up
                StartCoroutine(DespawnObject(gameObj, obj));
            }
        }
        else
        {
            // Debug.Log($"GameManager: Time excedded at {videoPlayer.time}, deleting object");
            Destroy(gameObj);
        }
    }

    /// <summary>
    /// Destroys object after its despawn time is up
    /// </summary>
    public IEnumerator DespawnObject(GameObject gameObj, ObjectInfo obj)
    {
        while (videoPlayer.time < obj.popupTime + obj.despawnTime)
        {
            yield return null;
        }

        Destroy(gameObj);
    }

    #endregion
    #region Skip & Debug

    /// <summary>
    /// Loads inputted choiceID
    /// </summary>
    public void LoadTestChoice()
    {
        string id = testInput.text != "" ? testInput.text : "Start_";
        LoadChoice(id);
    }

    /// <summary>
    /// Sets inputted time in vid
    /// </summary>
    public void SetVidTime()
    {   
        // Defaults to zero if input is empty
        if (!float.TryParse(timeInput.text, out float timestamp))
            timestamp = 0;
        
        if (!fm.isFinale)
        {
            isDebugSkipping = true;
            LoadChoice(currentChoice.choiceID.ToString());
            isDebugSkipping = false;
        }
        SkipVidTime(timestamp);
    }

    /// <summary>
    /// Skips to choices in vid
    /// </summary>
    public void Skip()
    {
        // Debug.Log("GameManager: Skip()");
        // Debug.Log($"GameManager: Skip - !iMenu.isPaused {!iMenu.isPaused} && videoPlayer.isPlaying {videoPlayer.isPlaying} && !choiceVisable {!choiceVisable} && currentChoice.choiceState == ChoiceState.Choice {currentChoice.choiceState}");
        if (!iMenu.isPaused && !fm.isFinale && videoPlayer.isPlaying && !choiceVisable && currentChoice.choiceState.Contains(ChoiceState.Choice))
        {
            // If the skip text is not visable on screen
            if (skipText.color.a == 0)
            {
                // Debug.Log($"GameManager: Skip - Text Popup");
                fadeTextAni.Play("Fade In");
            }
            // Skips if the player presses the skip button while the text is onscreen
            else
            {
                // Debug.Log($"GameManager: Skip - SkipVidTime");
                SkipVidTime(GetSkipTime(currentChoice));
            }
        }
    }

    /// <summary>
    /// Gets skip timestamp for vid
    /// </summary>
    float GetSkipTime(ChoiceInfo choice)
    {
        // Debug.Log("GameManager: GetSkipTime()");
        float choiceTime = float.PositiveInfinity;

        // Finds the first non skippable object in the vid
        foreach (ObjectInfo obj in choice.objs)
        {
            if (!obj.isSkippable)
            {
                if (obj.popupTime < choiceTime)
                {
                    choiceTime = obj.popupTime;
                    // Debug.Log($"GameManager: Skip - choiceTime {choiceTime}");
                }
            }
        }

        // Skips to the timestamp in the vid
        return choiceTime;
    }

    /// <summary>
    /// Skips to the selected timestamp in the vid
    /// </summary>
    public void SkipVidTime(float timestamp)
    {
        // Debug.Log("GameManager: SkipVidTime()");
        // Sets the bool to false after each use
        isSkipping = false;

        // Debug.Log("GameManager: Skipping time in vid");

        // Disables text
        fadeTextAni.Play("Invisible Text");
        // Sets time in the vid
        videoPlayer.time = timestamp;
    }

    #endregion

    /// <summary>
    /// Clears any previous info such as coroutines or objects from the previous choice
    /// </summary>
    public void ClearPreviousInfo()
    {
        if (coroutines.Count > 0)
        {
            foreach (Coroutine c in coroutines)
                if (c != null)
                    StopCoroutine(c);
            coroutines.Clear();
        }

        // Deletes existing buttons in object holder
        if (objHolder.childCount > 0)
        {
            foreach (Transform child in objHolder)
                Destroy(child.gameObject);
        }

        // If the skip text is visable on screen when selection is made
        if (skipText.color.a != 0)
        {
            // Debug.Log($"GameManager: Skip - Text Popup");
            fadeTextAni.Play("Invisible Text");
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Catches issue where the vid gets frozen
        if (!hasFocus)
        {
            videoPlayer.Pause();
        }
        else
        {
            if (!vidFinished && !videoPlayer.isPlaying && !pauseVideoPlayer)
            {
                // Debug.Log("GameManager: Resuming vid");
                videoPlayer.Play();
            }
        }
    }
    
    void OnApplicationQuit()
    {
        // Resets Skip Intro's value
        PlayerPrefs.SetInt("Skip Intro", 0);
        PlayerPrefs.Save();
    }
}