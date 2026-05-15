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
    // Displays current choice
    public ChoiceInfo currentChoice;
    // Store prev choice id
    public string prevChoice;
    // Determines when the game can be paused in normal gameplay
    public bool canBePaused;
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
        fm = GetComponent<FinaleManager>();
        videoPlayer.prepareCompleted += PlayVid;
        videoPlayer.prepareCompleted += LoadVidInfo;

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
    }

    /// <summary>
    /// Loads the player's previous choice
    /// </summary>
    public void LoadPrevChoice()
    {
        // Debug.Log($"LoadPrevChoice - prevChoice {prevChoice.ToString()}"); 
        if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
        {
            if (prevChoice == null)
            {
                prevChoice = "Start_";
                Debug.LogError("Error, prevChoice is null, defaulting to Start_");
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

        Debug.Log($"Loading Choice {id}");

        // Stores prev choice, only stores prev choice in normal gameplay
        if (!isDebugSkipping && currentChoice != null)
            prevChoice = currentChoice.choiceID;

        // Debug.Log($"PrevChoice {prevChoice.ToString()}"); 

        // Disables at start of choice
        choiceVisable = false;

        // For debugging if retry menu is still open
        iMenu.CloseRetryMenu();

        // Stops vids
        videoPlayer.Stop();

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
                    Debug.Log($"Current choice {currentChoice.choiceID} is a reference, loading {prevChoice.choiceID} instead");
                    currentChoice = prevChoice;
                }
                else
                {
                    Debug.Log($"Error previous choice of reference choice {currentChoice.choiceID} does not exist");
                }
            }

            currentChoice.hasComplete = true;

            if (currentChoice.choiceID == "Finale_")
            {
                fm.PlayFinale();
                return;
            }

            if (!currentChoice.vid)
            {
                Debug.Log("No video detected");
            }

            // Vid subtitles
            subtitlesManager.currentEntry = null;
            subtitlesManager.currentSubtitles = currentChoice.subtitles;

            // If the choice contains any achieveIDs to see if has to update its achieveState
            if (currentChoice.achievements.Count > 0)
            {
                foreach (AchievementInfo achievementInfo in currentChoice.achievements)
                {
                    if (sm.achieveDict.ContainsKey(achievementInfo.achieveID))
                    {
                        AchievementInfo achievement = sm.achieveDict[achievementInfo.achieveID];
                        if (!achievement.hasUnlocked && achievement.achieveState == AchievementState.Locked)
                        {
                            achievement.achieveState = AchievementState.Shown;
                            achievement.updateDisplay = true;
                        }
                    }
                    else
                    {
                        // Debug.Log($"AchieveID {id} not found in system in LoadChoice()");
                    }
                }
            }
            else
            {
                // Debug.Log($"No achievements found for ChoiceID {currentChoice.choiceID} in LoadChoice()");
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
                        // Debug.Log($"Weapon {id} not found in system in LoadChoice(), creating new entry");
                        sm.stats.weaponDict.Add(weapon, 1);
                    }
                }
            }
            else
            {
                // Debug.Log($"No achievements found for ChoiceID {currentChoice.choiceID} in LoadChoice()");
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
            Debug.Log($"ID - {id} - not found in the system when checking in LoadChoice()");
            LoadChoice("Start_");
        }
    }

    /// <summary>
    /// Loads the time based info such as objects when the video is ready
    /// </summary>
    public void LoadVidInfo(VideoPlayer vp)
    {
        // Debug.Log("LoadVidInfo");
        // Clears any coroutines from the previous choice
        ClearPreviousInfo();

        // Opens Retry Menu variant of the pause menu at ending or gameover
        if (currentChoice.vidEndTime > 0)
            coroutines.Add(StartCoroutine(RetryMenuPopup(currentChoice.vidEndTime)));

        // The choice has any objects
        if (currentChoice.objs != null)
        {
            // Debug.Log($"Loading {currentChoice.choice}'s objects");
            // Starts a coroutine for each object
            foreach (ObjectInfo obj in currentChoice.objs)
            {
                // Spawns object
                coroutines.Add(StartCoroutine(SpawnObject(obj)));
            }
        }
    }

    /// <summary>
    /// Plays video when the vid is ready
    /// </summary>
    public void PlayVid(VideoPlayer vp)
    {
        // Debug.Log("Playing vid");
        vp.Play();
    }

    /// <summary>
    /// Closes the map menu when the vid is ready
    /// </summary>
    void CloseMapMenu(VideoPlayer vp)
    {
        // Debug.Log("Closing Map menu");
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
        // Debug.Log($"Retry menu will popup in {timestamp}s");

        while (videoPlayer.time < timestamp)
        {
            // Debug.Log(videoPlayer.time);
            yield return null;
        }

        // Counts deaths if choice has ChoiceState GameOver
        if (currentChoice.choiceState.Contains(ChoiceState.GameOver))
        {
            Debug.Log($"Death Detected in {currentChoice.choiceID}");
            sm.stats.deaths += 1;
        } 

        if (currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
        {
            if (sm.choiceDict.TryGetValue($"{currentChoice.choiceID}_1", out ChoiceInfo referenceChoice))
            {
                referenceChoice.hasComplete = true;
                if (referenceChoice.choiceState.Contains(ChoiceState.GameOver))
                {
                    // Debug.Log($"Death Detected in reference choice {referenceChoice.choiceID}");
                    sm.stats.deaths += 1;
                }
            }
            else
            {
                // Debug.Log($"No reference choice detected in {currentChoice.choiceID}");
            }
        }

        GeneralAchievementsCheck();

        // Debug.Log("Opening retry menu");
        iMenu.OpenRetryMenu();
    }

    #endregion
    #region General Achievement Logic

    /// <summary>
    /// Checks if the player has met any of the requirements for the general achievements
    /// </summary>
    public void GeneralAchievementsCheck()
    {
        // Debug.Log("General Achievements Check");
        int completed, total;

        CheckAchievement("General_1", () => sm.stats.deaths > 0);
        CheckAchievement("General_2", () => {
            (completed, total) = sm.stats.FailsCompleted(sm.choiceDict);
            return completed == total;
        });
        CheckAchievement("General_3", () => {
            (completed, total) = sm.stats.EndingsCompleted(sm.choiceDict);
            return completed > 0;
        });
        CheckAchievement("General_4", () => {
            (completed, total) = sm.stats.EndingsCompleted(sm.choiceDict);
            return completed == total;
        });
        CheckAchievement("General_5", () => {
            (completed, total) = sm.stats.ChoicesCompleted(sm.choiceDict);
            return completed == total;
        });
        CheckAchievement("General_7", () => 
            sm.stats.Completion(sm.choiceDict, sm.achieveDict) >= 100
        );
    }

    /// <summary>
    /// Checks if condition for the general achievement has been met
    /// </summary>
    public void CheckAchievement(string id, Func<bool> condition)
    {
        if (sm.achieveDict.TryGetValue(id, out AchievementInfo achievement) 
            && !achievement.hasUnlocked && condition())
                AchievementUnlock(achievement);
    }   
    
    /// <summary>
    /// Unlocks achievement and adds it to the achievement popup queue
    /// </summary>
    void AchievementUnlock(AchievementInfo achievement)
    {
        Debug.Log($"Achievement {achievement.achieveID} Unlocked!");
        // Marked the achievement as unlocked
        achievement.hasUnlocked = true;
        // Tells the game that it needs to update its display in the achievements menu
        achievement.updateDisplay = true;
        // Changes the achievement's state from Locked or Hidden to Shown
        achievement.achieveState = AchievementState.Shown;
        
        StartCoroutine(sm.achievePopup.AchievePopup(achievement));
    }

    #endregion
    #region Object Logic

    /// <summary>
    /// Spawns an object at its specific time in the vid
    /// </summary>
    public IEnumerator SpawnObject(ObjectInfo obj)
    {
        // Debug.Log("In object spawn coroutine");

        // Spawns object
        GameObject gameObj = Instantiate(obj.obj, objHolder);

        // Disables every object in the parent spawned object
        foreach (Transform child in gameObj.transform)
        {
            // Deletes objects if they only appear in subsequent runs
            if (obj.subsequentRunsOnly && !sm.achieveDict["General_3"].hasUnlocked)
            {
                // Debug.Log("Object only appears in subsequent runs, deleting object");
                Destroy(gameObj);
                yield break;
            }

            // Debug.Log($"{child.name} {child.tag}");
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
                        // Debug.Log($"{objName} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                        if (sm.choiceDict.ContainsKey(objName))
                        {
                            string choiceIDString = objName;

                            if (sm.choiceDict[objName].firstRunOnly && sm.choiceDict[objName].hasComplete)
                                Destroy(choiceBtn.gameObject);

                            choiceBtn.onClick.AddListener(() => LoadChoice(choiceIDString));
                        }
                        else
                        {
                            Debug.Log($"ID - {objName} - not found in the system when checking in SpawnObject()");
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
                            Debug.Log($"LetterID {letter.letterID} - {letter.letter} has already been obtained, deleting object");
                            Destroy(gameObj);
                            yield break;
                        }
                        // Adds functionality unlock screen functionality
                        else
                        {
                            // Debug.Log($"Adding LetterID {letter.letterID} - {letter.letter}");
                            secretBtn.onClick.AddListener(() => 
                            {
                                fm.SecretButton(letter);
                                Destroy(gameObj);
                            });   
                        }
                    }
                    else
                    {
                        Debug.Log($"LetterID {secretBtn.name} not in system");
                    }
                    break;
            }

            child.gameObject.SetActive(false);
        }

        // Spawns the button after the video reaches it specific timestamp
        while (videoPlayer.time < obj.popupTime)
        {
            // Debug.Log(videoPlayer.time);
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

                    GeneralAchievementsCheck();
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
            // Debug.Log($"Time excedded at {videoPlayer.time}, deleting object");
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
        // Debug.Log("Skip()");
        // Debug.Log($"Skip - !iMenu.isPaused {!iMenu.isPaused} && videoPlayer.isPlaying {videoPlayer.isPlaying} && !choiceVisable {!choiceVisable} && currentChoice.choiceState == ChoiceState.Choice {currentChoice.choiceState}");
        if (!iMenu.isPaused && !fm.isFinale && videoPlayer.isPlaying && !choiceVisable && currentChoice.choiceState.Contains(ChoiceState.Choice))
        {
            // If the skip text is not visable on screen
            if (skipText.color.a == 0)
            {
                // Debug.Log($"Skip - Text Popup");
                fadeTextAni.Play("Fade In");
            }
            // Skips if the player presses the skip button while the text is onscreen
            else
            {
                // Debug.Log($"Skip - SkipVidTime");
                SkipVidTime(GetSkipTime(currentChoice));
            }
        }
    }

    /// <summary>
    /// Gets skip timestamp for vid
    /// </summary>
    float GetSkipTime(ChoiceInfo choice)
    {
        // Debug.Log("GetSkipTime()");
        float choiceTime = float.PositiveInfinity;

        // Finds the first non skippable object in the vid
        foreach (ObjectInfo obj in choice.objs)
        {
            if (!obj.isSkippable)
            {
                if (obj.popupTime < choiceTime)
                {
                    choiceTime = obj.popupTime;
                    // Debug.Log($"Skip - choiceTime {choiceTime}");
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
        // Debug.Log("SkipVidTime()");
        // Sets the bool to false after each use
        isSkipping = false;

        // Debug.Log("Skipping time in vid");

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
            // Debug.Log($"Skip - Text Popup");
            fadeTextAni.Play("Invisible Text");
        }
    }
    
    void OnApplicationQuit()
    {
        // Resets Skip Intro's value
        PlayerPrefs.SetInt("Skip Intro", 0);
        PlayerPrefs.Save();
    }
}