using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Main Game")]
    // Video player
    public VideoPlayer videoPlay;
    // Displays current choice
    public ChoiceInfo currentChoice;
    // Store prev choice id
    public string prevChoice;
    // Skipping in vids
    [SerializeField]
    private Animator fadeTextAni;
    TMP_Text skipText;
    [SerializeField]
    private GameObject objHolder;
    // Used to determine when a choice is visable on screen
    [HideInInspector]
    public bool choiceVisable;
    [HideInInspector]
    public bool isSkipping;
    bool isDebugSkipping;
    // Scripts
    [HideInInspector]
    public InputMenu iMenu;
    [HideInInspector]
    public SaveManager sm;
    List<Coroutine> coroutines = new List<Coroutine>();
    // Debug menu
    [SerializeField]
    private TMP_InputField timeInput;
    [SerializeField]
    private TMP_InputField testInput;
    [SerializeField]
    private TMP_Text currentTime;
    public bool canBePaused;
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
    [SerializeField]
    private bool isFinale;
    [Header("Stats Page")]
    [SerializeField]
    private Animator statsAni;
    [SerializeField]
    private List<TMP_Text> statsText;


    void Start()
    {
        // Gets components
        sm = FindAnyObjectByType<SaveManager>();
        skipText = fadeTextAni.GetComponent<TMP_Text>();
        iMenu = GetComponent<InputMenu>();

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

        Debug.Log($"LoadChoice started with the id {id}");

        // Stores prev choice, only stores prev choice in normal gameplay
        if (!isDebugSkipping && currentChoice != null)
            prevChoice = currentChoice.choiceID;

        // Debug.Log($"LoadChoice - prevChoice {prevChoice.ToString()}"); 

        // Disables at start of choice
        choiceVisable = false;

        // For debugging if retry menu is still open
        if (iMenu.isRetryMenu)
        {
            iMenu.isRetryMenu = false;
            iMenu.Resume();
        }

        // Stops vids
        videoPlay.Stop();

        // Clears existing coroutines to spawn objects
        if (coroutines.Count > 0)
        {
            foreach (Coroutine c in coroutines)
                if (c != null)
                    StopCoroutine(c);
            coroutines.Clear();
        }

        // Deletes existing buttons in object holder
        if (objHolder.transform.childCount > 0)
        {
            foreach (Transform child in objHolder.transform)
                Destroy(child.gameObject);
        }

        // Attempts to get choice info
        if (sm.choiceDict.TryGetValue(id, out currentChoice))
        {
            if (!currentChoice.vid)
            {
                Debug.Log("No video detected");
            }

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

            // Opens Retry Menu variant of the pause menu at ending or gameover
            if ((currentChoice.choiceState.Contains(ChoiceState.GameOver) || currentChoice.choiceState.Contains(ChoiceState.Ending))
                && !currentChoice.choiceState.Contains(ChoiceState.Choice))
                coroutines.Add(StartCoroutine(RetryMenuPopup(currentChoice.vidEndTime)));

            // If the choice contains any achieveIDs to see if has to update its achieveState
            if (currentChoice.achieveIDs.Count > 0)
            {
                foreach (string achieveID in currentChoice.achieveIDs)
                {
                    if (sm.achieveDict.TryGetValue(achieveID, out AchievementInfo achievement))
                    {
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
            
            if (currentChoice.choiceState.Contains(ChoiceState.GameOver))
            {
                // Debug.Log("Death Detected");
                sm.stats.deaths += 1;
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

            // Debug.Log("Playing vid");
            videoPlay.Play();
        }
        else
        {
            Debug.Log($"ID - {id} - not found in the system when checking in LoadChoice()");
        }
    }

    public IEnumerator GetVidTime()
    {
        while (iMenu.dMenu.interactable)
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

        // Marks the choice as completed when the player gets to a retry menu
        currentChoice.hasComplete = true;

        // Debug.Log("Openning retry menu");
        iMenu.OpenRetryMenu();
    }

    // Spawns object at the specific time
    public IEnumerator SpawnObject(ObjectInfo obj)
    {
        // Debug.Log("In object spawn coroutine");

        // Spawns object
        GameObject gameObj = Instantiate(obj.obj, objHolder.transform);

        //Debug.Log($"{gameObj.name}'s object type is {obj.objType}");

        // Adds function to the buttons
        switch (obj.objType)
        {
            // Function for the choice buttons
            case ObjectType.ChoiceBtn:
                Button[] choiceBtns = gameObj.GetComponentsInChildren<Button>();

                foreach (Button btn in choiceBtns)
                {
                    string objName = btn.gameObject.name.Trim();
                    // Only checks objects with potential of being an id
                    if (objName.Contains("_"))
                    {
                        // Debug.Log($"{objName} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                        if (sm.choiceDict.ContainsKey(objName))
                        {
                            string choiceIDString = objName;

                            btn.onClick.AddListener(() => LoadChoice(choiceIDString));
                        }
                        else
                        {
                            Debug.Log($"ID - {objName} - not found in the system when checking in SpawnObject()");
                        }
                    }
                }
                break;
            case ObjectType.SecretBtn:
                Button secretBtn = gameObj.GetComponentInChildren<Button>();

                // checks to see if letter has already been obtained
                if (LetterID.TryParse(secretBtn.name, true, out LetterID id) &&
                    sm.letterDict.TryGetValue(id, out LetterInfo letter))
                {
                    if (letter.hasObtained)
                    {
                        Debug.Log($"LetterID {letter.letterID} - {letter.letter} has already been obtained, deleting object");
                        Destroy(gameObj);
                        yield break;
                    }
                    else
                    {
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

        foreach (Transform child in gameObj.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Spawns the button after the video reaches it specific timestamp
        while (videoPlay.time < obj.popupTime)
        {
            // Debug.Log(videoPlay.time);
            yield return null;
        }

        // Spawns object within timestamp
        if (videoPlay.time <= obj.popupTime + 1)
        {
            // Debug.Log($"Popping up {obj.objType} {obj.objID}");

            // Marks the choice as completed once the player gets to a choice
            if (obj.objType == ObjectType.ChoiceBtn)
            {
                currentChoice.hasComplete = true;
                // Prevents skipping after the choice is on screen
                choiceVisable = true;
            }

            if (!currentChoice.choiceState.Contains(ChoiceState.ChoiceTimed))
                canBePaused = true;

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
    
    // Opens finale unlock screen
    IEnumerator FinaleUnlockScreen(LetterInfo unlockedLetter)
    {
        Debug.Log($"Opening Finale unlock screen with letter {unlockedLetter.letterID} - {unlockedLetter.letter}");
        LetterInfo letter;
        GameObject unlockedLetterIcon = null;

        // Pauses the current video
        videoPlay.Pause();

        if (letterIcons == null)
            letterIcons = letterIconsGroup.GetComponentsInChildren<Animator>(true);

        // Reactivates the icon group if the group object has been deactviated if the player has reset is save progress
        if (letterCount != sm.letterDict.Count && !letterIconsGroup.activeSelf)
            letterIconsGroup.SetActive(true);

        foreach (Animator letterIcon in letterIcons)
        {
            // Debug.Log(letterIcon.name);
            // Stores unlocked icon
            if (letterIcon.name == unlockedLetter.letterID.ToString())
            {
                unlockedLetterIcon = letterIcon.gameObject;
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
                    // Starts bob animation at a random point so they are all not synchronized & disables particles system
                    letterIcon.GetComponent<Animator>().Play("Letter Bob", 0, UnityEngine.Random.value);
                    letterCount += 1;
                }
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

        // Activates the newly unlocked letter
        yield return new WaitForSeconds(1f);
        // Debug.Log("Enabling Object");

        unlockedLetterIcon.SetActive(true); 
        unlockedLetter.hasObtained = true;
        letterCount += 1;

        yield return new WaitForSeconds(10f);

        if (letterCount < sm.letterDict.Count)
        {
            StartCoroutine(TextPopIn($"<size=280>{sm.letterDict.Count - letterCount} Remain", .3f));

            yield return new WaitForSeconds(8f);

            StartCoroutine(TextPopIn($"<size=200><i>The Finale Awaits", .05f));

            yield return new WaitForSeconds(6f);
            
            finaleUnlockScreen.alpha = 0;
            finaleUnlockLabel.maxVisibleCharacters = 0;
            videoPlay.Play();
        }
        else
        {
            yield return new WaitForSeconds(2f);

            letterIconsGroup.SetActive(false);

            yield return new WaitForSeconds(2f);

            letterCircleGroup.SetActive(true);
        }
    }

    public IEnumerator StartFinale()
    {
        Debug.Log("Finale Started");
        isFinale = true;
        videoPlay.Stop();
        videoPlay.clip = finaleVids[0];
        videoPlay.time = 0;
        videoPlay.loopPointReached += PopUpStats;

        finaleUnlockScreen.alpha = 0;
        
        videoPlay.Play();
        
        yield return null;
    }

    void PopUpStats(VideoPlayer vp)
    {
        videoPlay.loopPointReached -= PopUpStats;
        
        StartCoroutine(ShowStats());
    }

    IEnumerator ShowStats()
    {
        Debug.Log("Show Stats");

        if (!sm.stats.weaponDict.ContainsKey("Boots"))
            sm.stats.weaponDict["Boots"] = 1;

        statsText[0].text = sm.stats.gameMode;
        statsText[1].text = sm.stats.mostUsedMon;
        statsText[2].text = sm.stats.mostUsedMove;
        statsText[3].text = sm.stats.weaponDict.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key.ToString();
        statsText[4].text = sm.stats.weaponDict["Boots"].ToString();
        statsText[5].text = sm.stats.deaths.ToString();
        var (choicesCompleted, choicesCompletedTotal) = sm.stats.ChoicesCompleted(sm.choiceDict);
        statsText[6].text = $"{choicesCompleted}/{choicesCompletedTotal}";
        var (endingsCompleted, endingsCompletedTotal) = sm.stats.EndingsCompleted(sm.choiceDict);
        statsText[7].text = $"{endingsCompleted}/{endingsCompletedTotal}";
        statsText[8].text = sm.stats.Completion(sm.choiceDict, sm.achieveDict);
        TimeSpan time = TimeSpan.FromSeconds(sm.stats.playTime);
        statsText[9].text = $"{time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";

        yield return new WaitForSeconds(2f);

        statsAni.Play("Fade In");

        // Add code for yeild return for music here to auto play post credits
    }
    
    public void CloseStats()
    {
        statsAni.Play("Fade Out");
        
        StartCoroutine(PlayPostCredits());
    }

    IEnumerator PlayPostCredits()
    {
        yield return new WaitForSeconds(3f);
        
        videoPlay.Stop();
        videoPlay.clip = finaleVids[1];
        videoPlay.time = 0;
        videoPlay.loopPointReached += ReturnToTitleScreen;
        
        videoPlay.Play();
    }

    void ReturnToTitleScreen(VideoPlayer vp)
    {
        SceneManager.LoadScene("Title Screen");
    }

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

    // Debug Test
    public void LoadTestChoice()
    {
        string id = testInput.text != "" ? testInput.text : "Start_";
        LoadChoice(id);
    }

    // Debug Time
    public void SetVidTime()
    {   
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
        if (!iMenu.isPaused && !isFinale && videoPlay.isPlaying && !choiceVisable && currentChoice.choiceState.Contains(ChoiceState.Choice)
            && !(currentChoice.choiceState.Contains(ChoiceState.GameOver) || currentChoice.choiceState.Contains(ChoiceState.Ending)))
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
                SkipVidTime(GetSkipTime(currentChoice));
            }
        }
    }

    // Gets skip timestamp for vid
    float GetSkipTime(ChoiceInfo choice)
    {
        // Debug.Log("GetSkipTime()");
        float choiceTime = float.PositiveInfinity;

        // Finds the first choice in the vid
        foreach (ObjectInfo obj in choice.objs)
        {
            if (obj.objType == ObjectType.ChoiceBtn)
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
    void SkipVidTime(float timestamp)
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
}
