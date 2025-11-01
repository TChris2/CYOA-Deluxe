using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Internal;
using Unity.VisualScripting;

// NOTE TO SELF WHEN WORKING ON MAP MOVE SCRIPT OVER TO MAIN CANVAS AND CREATE SHORT SCRIPTS WHICH IS ESSENTIALLY JUST START FUNCTION
// AND SENDS ID AND VIDEOPLAYER INFO
public class ButtonManager : MonoBehaviour
{
    // Video player
    public VideoPlayer videoPlay;
    // Displays current choice
    public ChoiceInfo currentChoice;
    // Store prev choice id
    [SerializeField]
    private ChoiceID prevChoice;
    // Skipping in vids
    [SerializeField]
    private Animator fadeTextAni;
    [SerializeField]
    private GameObject objHolder;
    TMP_Text skipText;
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
    [SerializeField]
    private TMP_InputField timeInput;
    [SerializeField]
    private TMP_InputField testInput;

    void Start()
    {
        // Gets components
        sm = GameObject.Find("SaveManager").GetComponent<SaveManager>();
        skipText = fadeTextAni.GetComponent<TMP_Text>();
        iMenu = GetComponent<InputMenu>();

        // Loads current choice id
        string id = PlayerPrefs.GetString("Current ChoiceID", "Start_");

        // Loads choice info
        LoadChoice(id);
    }

    public void LoadPrevChoice()
    {
        // Debug.Log($"LoadPrevChoice - prevChoice {prevChoice.ToString()}"); 
        LoadChoice(prevChoice.ToString());
    }
    

    // Loads choice infor
    public void LoadChoice(string id)
    {
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
        if (System.Enum.TryParse(id, out ChoiceID choiceID))
        {
            if (sm.choiceDict.TryGetValue(choiceID, out currentChoice))
            {
                // Checks to see if the dict already has the video loaded
                if (currentChoice.CheckVid())
                {
                    // Debug.Log("Video detected");
                }
                // Gets vid if the video is not already loaded
                else
                {
                    // Debug.Log("No video detected, loading vid from resources");
                    string idString = currentChoice.choiceID.ToString();
                    string[] parts = idString.Split('_');

                    // Debug.Log($"Videos/{parts[0]}/{idString}");
                    currentChoice.SetVid(Resources.Load<VideoClip>($"Videos/{parts[0]}/{idString}"));
                }

                // Sets video to the player
                videoPlay.clip = currentChoice.vid;
                videoPlay.time = 0;

                // Marks the choice as completed
                currentChoice.hasComplete = true;

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
                if (currentChoice.choiceState == ChoiceState.GameOver || currentChoice.choiceState == ChoiceState.Ending)
                    coroutines.Add(StartCoroutine(RetryMenuPopup(currentChoice.vidEndTime)));

                // Skips to first choice if enabled
                    if (isSkipping)
                        GetSkipTime(currentChoice);

                // Debug.Log("Playing vid");
                videoPlay.Play();
            }
            else
                Debug.LogWarning($"ChoiceID '{id}' not found in dictionary");
        }
        else
            Debug.LogError($"Invalid ChoiceID string: {id}, returning default");
    }

    private IEnumerator RetryMenuPopup(float timestamp)
    {
        // Debug.Log($"Retry menu will popup in {timestamp}s");

        while (videoPlay.time < timestamp)
        {
            // Debug.Log(videoPlay.time);
            yield return null;
        }

        if (videoPlay.time <= timestamp + 1)
        {
            // Debug.Log("Openning retry menu");
            iMenu.OpenRetryMenu();
        }
        else
            Debug.Log($"Time excedded not popping up retry menu {videoPlay.time}");
    }

    // Spawns object at the specific timecode
    public IEnumerator SpawnObject(ObjectInfo obj)
    {
        // Debug.Log("In object spawn coroutine");

        // Spawns object
        GameObject gameObj = Instantiate(obj.obj, objHolder.transform);

        // Adds function to the buttons
        switch (obj.objType)
        {
            // Function for the choice buttons
            case ObjectType.ChoiceBtn:
                Button[] choiceBtns = gameObj.GetComponentsInChildren<Button>();

                foreach (Button btn in choiceBtns)
                {
                    if (System.Enum.TryParse(btn.gameObject.name, out ChoiceID choiceID))
                    {
                        // Debug.Log($"{btn.gameObject.name} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                        if (sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))
                        {
                            string choiceIDString = choiceID.ToString();

                            btn.onClick.AddListener(() => LoadChoice(choiceIDString));
                        }
                    }
                }
                break;
            case ObjectType.SecretBtn:
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

            // Enables choice visable to prevent skipping after the choice is on screen
            choiceVisable = true;

            foreach (Transform child in gameObj.transform)
            {
                child.gameObject.SetActive(true);

                yield return new WaitForSeconds(obj.childPopupDelay);
            }

            // If the object despawns
            if (obj.despawnTime != 0)
            {
                // Destroys object after its despawn time is up
                StartCoroutine(DespawnObject(gameObj, obj.despawnTime));
            }
        }
        else
        {
            // Debug.Log($"Time excedded at {videoPlay.time}, deleting object");
            Destroy(gameObj);
        }
    }

    // Destroys object after its despawn time is up
    public IEnumerator DespawnObject(GameObject gameObj, float objDespawnTime)
    {
        yield return new WaitForSeconds(objDespawnTime);
        Destroy(gameObj);
    }
    
    public void LoadTestChoice()
    {
        string id = testInput.text != "" ? testInput.text : "Start_";
        LoadChoice(id);
    }

    public void SetVidTime()
    {   
        // Defaults to zero if input is empty
        if (!float.TryParse(timeInput.text, out float timestamp))
            timestamp = 0;
        
        isDebugSkipping = true;
        LoadChoice(currentChoice.choiceID.ToString());
        isDebugSkipping = false;
        SkipVidTime(timestamp);
    }

    // Skips to choices in vid
    public void Skip()
    {
        // Debug.Log($"Skip - !iMenu.isPaused {!iMenu.isPaused} && videoPlay.isPlaying {videoPlay.isPlaying} && !choiceVisable {!choiceVisable} && currentChoice.choiceState == ChoiceState.Choice {currentChoice.choiceState == ChoiceState.Choice}");
        if (!iMenu.isPaused && videoPlay.isPlaying && !choiceVisable && currentChoice.choiceState == ChoiceState.Choice)
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
        // Sets the bool to false after each use
        isSkipping = false;

        // Debug.Log("Skipping time in vid");

        // Disables text
            fadeTextAni.Play("Invisible Text");
        // Sets time in the vid
        videoPlay.time = timestamp;
    }
}
