using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Main Game")]
    // Video player
    public VideoPlayer videoPlay;
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
    List<Coroutine> coroutines = new List<Coroutine>();
    [Header("Debug Menu")]
    [SerializeField]
    private TMP_InputField timeInput;
    [SerializeField]
    private TMP_InputField testInput;
    [SerializeField]
    private TMP_Text currentTime;
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
    private GameObject letterCircleGroup;
    [SerializeField]
    private TMP_Text finaleUnlockLabel;
    [SerializeField]
    private int letterCount;
    [Header("Finale")]
    [SerializeField]
    private VideoClip[] finaleVids;
    public bool isFinale;
    [Header("Finale Stats Page")]
    [SerializeField]
    private Animator statsAni;
    [SerializeField]
    private List<TMP_Text> statsText;
    [SerializeField]
    private AudioSource statsAudioSource;

    #region Main Game
    void Start()
    {
        // Gets components
        sm = FindAnyObjectByType<SaveManager>();
        skipText = fadeTextAni.GetComponent<TMP_Text>();
        iMenu = FindAnyObjectByType<InputMenu>();
        videoPlay.prepareCompleted += PlayVid;
        videoPlay.prepareCompleted += LoadVidInfo;

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

    // Resets Game Manager variables
    public void ResetLocalVars()
    {
        unlockScreenOpened = false;
        letterCount = 0;

        foreach(LetterInfo letter in sm.letterDict.Values)
        {
            if (letter.hasObtained)
                letterCount += 1;
        }
    }

    // Loads player's previous choice
    public void LoadPrevChoice()
    {
        // Debug.Log($"LoadPrevChoice - prevChoice {prevChoice.ToString()}"); 
        if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
            LoadChoice(prevChoice.ToString());
        else
            LoadChoice(currentChoice.choiceID.ToString());
    }


    // Loads choice info
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
        if (iMenu.isRetryMenu)
        {
            iMenu.isRetryMenu = false;
        }

        // Stops vids
        videoPlay.Stop();

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
                PlayFinale();
                return;
            }

            if (!currentChoice.vid)
            {
                Debug.Log("No video detected");
            }

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
                            achievement.achieveState = AchievementState.Hidden;
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
            videoPlay.clip = currentChoice.vid;
            videoPlay.time = 0;

            videoPlay.Prepare();
        }
        else
        {
            Debug.Log($"ID - {id} - not found in the system when checking in LoadChoice()");
            LoadChoice("Start_");
        }
    }

    // Loads the time based info such as objects
    void LoadVidInfo(VideoPlayer vp)
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

    void PlayVid(VideoPlayer vp)
    {
        // Debug.Log("Playing vid");
        vp.Play();
    }

    // Displays current time in vid
    public IEnumerator GetVidTime()
    {
        yield return null;

        while (iMenu.completeOverride && videoPlay != null)
        {
            currentTime.text = videoPlay.time.ToString("0.00");
            yield return null;
        }
    }

    // Popups retry menu
    private IEnumerator RetryMenuPopup(float timestamp)
    {
        // Debug.Log($"Retry menu will popup in {timestamp}s");

        while (videoPlay.time < timestamp)
        {
            // Debug.Log(videoPlay.time);
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

    // Checks if the player has met any of the requirements for the general achievements
    void GeneralAchievementsCheck()
    {
        // Debug.Log("General Achievements Check");
        int completed, total;

        CheckAchievement("General_1", () => {
            (completed, total) = sm.stats.FailsCompleted(sm.choiceDict);
            return completed > 0;
        });
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

    // Checks if condition for the general achievement has been met
    void CheckAchievement(string id, Func<bool> condition)
    {
        if (sm.achieveDict.TryGetValue(id, out AchievementInfo achievement) 
            && !achievement.hasUnlocked && condition())
                AchievementUnlock(achievement);
    }   
    
    // Unlocks achievement and adds it to the achievement popup queue
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

    // Spawns object at the specific time
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

                            if (child.GetComponent<RemoveChoice>() && sm.choiceDict[objName].hasComplete)
                                child.GetComponent<RemoveChoice>().DeleteChoice();

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
                            secretBtn.onClick.AddListener(() => {StartCoroutine(FinaleUnlockScreen(letter));
                                Destroy(gameObj);});   
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
        while (videoPlay.time < obj.popupTime)
        {
            // Debug.Log(videoPlay.time);
            yield return null;
        }

        // Displays object within timestamp
        if (videoPlay.time <= obj.popupTime + 1)
        {
            if (!obj.isSkippable)
            {
                // Prevents skipping after the choice is on screen
                choiceVisable = true;

                // Allows the game to be paused when it reaches a non timed choice
                if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
                    canBePaused = true;

                GeneralAchievementsCheck();
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
            // Debug.Log($"Time excedded at {videoPlay.time}, deleting object");
            Destroy(gameObj);
        }
    }

    // Destroys object after its despawn time is up
    public IEnumerator DespawnObject(GameObject gameObj, ObjectInfo obj)
    {
        while (videoPlay.time < obj.popupTime + obj.despawnTime)
        {
            yield return null;
        }

        Destroy(gameObj);
    }

    #endregion
    #region Finale
    
    // Opens finale unlock screen
    IEnumerator FinaleUnlockScreen(LetterInfo unlockedLetter)
    {
        Debug.Log($"Opening Finale unlock screen with letter {unlockedLetter.letterID} - {unlockedLetter.letter}");
        LetterInfo letter;
        GameObject unlockedLetterIcon = null;
        GameObject unlockedLetterParticles = null;

        // Pauses the current video
        videoPlay.Pause();

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
            if (letterIcon.name == unlockedLetter.letterID.ToString())
            {
                unlockedLetterIcon = letterIcon.gameObject;
                unlockedLetterParticles = letterParticles;
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

        // If it is the first time the player activates the finale unlock screen
        if (!unlockScreenOpened)
        {
            unlockScreenOpened = true;
        }

        // Activates the screen alongside 3d objects
        finaleUnlockScreen.alpha = 1;
        finaleUnlockScreen.blocksRaycasts = true;

        // Activates the newly unlocked letter
        yield return new WaitForSeconds(1f);
        // Debug.Log("Enabling Object");

        unlockedLetterIcon.SetActive(true); 
        unlockedLetterParticles.SetActive(true);
        unlockedLetter.hasObtained = true;
        letterCount += 1;

        yield return new WaitForSeconds(10f);

        // If the player has not collected every letter
        if (letterCount < sm.letterDict.Count)
        {
            StartCoroutine(TextPopIn($"<size=280>{sm.letterDict.Count - letterCount} Remain", .3f));

            yield return new WaitForSeconds(8f);

            StartCoroutine(TextPopIn($"<size=200><i>The Finale Awaits", .05f));

            yield return new WaitForSeconds(6f);
            
            // Returns player back to main game and unpauses the video player
            finaleUnlockScreen.alpha = 0;
            finaleUnlockScreen.blocksRaycasts = false;

            finaleUnlockLabel.maxVisibleCharacters = 0;
            // Plays the remaining video if it has not already finished
            if (videoPlay.time < videoPlay.length - .2f)
                videoPlay.Play();
        }
        // If the player has collected every letter
        else
        {
            yield return new WaitForSeconds(2f);

            // Disables main group of letters
            letterIconsGroup.SetActive(false);

            yield return new WaitForSeconds(2f);

            // Plays finale unlocking animation
            letterCircleGroup.SetActive(true);
        }
    }

    // Starts finale from letterCircleGroup Animator
    public IEnumerator StartFinale()
    {
        // Debug.Log("Finale Started");
        videoPlay.Stop();

        PlayFinale();

        finaleUnlockScreen.alpha = 0;
        finaleUnlockScreen.blocksRaycasts = false;
        
        yield return null;
    }

    // Plays first part of the finale
    void PlayFinale()
    {
        isFinale = true;
        ClearPreviousInfo();
        videoPlay.clip = finaleVids[0];
        videoPlay.time = 0;
        // Displays stats after video finishes
        videoPlay.loopPointReached += PopUpFinaleStats;
        videoPlay.prepareCompleted -= LoadVidInfo;
        videoPlay.Prepare();
    }

    void PopUpFinaleStats(VideoPlayer vp)
    {
        // Removes listener after it is done
        videoPlay.loopPointReached -= PopUpFinaleStats;
        
        StartCoroutine(ShowFinaleStats());
    }

    // Shows stats on screen
    IEnumerator ShowFinaleStats()
    {
        // Debug.Log("Show Stats");
        // Marks the finale as complete
        sm.choiceDict["Finale_"].hasComplete = true;
        GeneralAchievementsCheck();

        // Popups achievement for beating the finale
        CheckAchievement("General_6", () => true);

        // Displays stats
        sm.stats.DisplayStatsAll(statsText, sm);

        yield return new WaitForSeconds(2f);

        // Plays stats fade in animation
        statsAni.Play("Fade In");
        statsAudioSource.Play();
    }
    
    // Closes stat screen from button
    public void CloseFinaleStats()
    {
        statsAni.Play("Fade Out");
        
        StartCoroutine(PlayPostCredits());
    }

    // Plays the post credits scene
    IEnumerator PlayPostCredits()
    {
        // Debug.Log("Playing Post Credits");
        yield return new WaitForSeconds(3f);
        statsAudioSource.Stop();
        videoPlay.clip = finaleVids[1];
        videoPlay.time = 0;
        // Returns player to the title screen after post credits scene finishes
        videoPlay.loopPointReached += ReturnToTitleScreen;
        
        videoPlay.Prepare();
    }

    // Goes back to the title screen
    void ReturnToTitleScreen(VideoPlayer vp)
    {
        // Debug.Log("Returning to title screen");
        SceneManager.LoadScene("Title Screen");
        // Resets Skip Intro's value
        PlayerPrefs.SetInt("Skip Intro", 0); 
        PlayerPrefs.Save();
    }

    // Pops Finale Unlock Screen text onscreen
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
    #region Skip & Debug

    // Debug Test
    public void LoadTestChoice()
    {
        if (iMenu.isRetryMenu)
            iMenu.Resume();
        string id = testInput.text != "" ? testInput.text : "Start_";
        LoadChoice(id);
    }

    // Debug Time
    public void SetVidTime()
    {   
        if (iMenu.isRetryMenu)
            iMenu.Resume();
        // Defaults to zero if input is empty
        if (!float.TryParse(timeInput.text, out float timestamp))
            timestamp = 0;
        
        if (!isFinale)
        {
            isDebugSkipping = true;
            LoadChoice(currentChoice.choiceID.ToString());
            isDebugSkipping = false;
        }
        SkipVidTime(timestamp);
    }

    // Skips to choices in vid
    public void Skip()
    {
        // Debug.Log("Skip()");
        // Debug.Log($"Skip - !iMenu.isPaused {!iMenu.isPaused} && videoPlay.isPlaying {videoPlay.isPlaying} && !choiceVisable {!choiceVisable} && currentChoice.choiceState == ChoiceState.Choice {currentChoice.choiceState}");
        if (!iMenu.isPaused && !isFinale && videoPlay.isPlaying && !choiceVisable && currentChoice.choiceState.Contains(ChoiceState.Choice))
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

    // Gets skip timestamp for vid
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

    // Skips to the selected timestamp in the vid
    public void SkipVidTime(float timestamp)
    {
        // Debug.Log("SkipVidTime()");
        // Sets the bool to false after each use
        isSkipping = false;

        // Debug.Log("Skipping time in vid");

        // Disables text
        fadeTextAni.Play("Invisible Text");
        // Sets time in the vid
        videoPlay.time = timestamp;
    }

    #endregion

    // Clears any previous info such as coroutines or objects from the previous choice
    void ClearPreviousInfo()
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