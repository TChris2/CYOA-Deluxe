using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Functionality for the map menu
public class MapMenu : MonoBehaviour
{
    // Main contents of the map menu
    [SerializeField]
    private Transform mapContents;
    // Prefab of the wya icon
    [SerializeField]
    private GameObject wyaIcon;
    // Keeps track of instantiate icons 
    private GameObject wyaIconStorage;
    CanvasGroup mapMenu;
    // Scripts
    SaveManager sm;
    GameManager gm;
    InputMenu iMenu;
    [Header("Map Menu")]
    HashSet<string> portalMaps = new HashSet<string> { "Portal_", "Minecraft_", "BOTW_" };
    HashSet<string> outsideMaps = new HashSet<string> { "Outside_", "Tesco_", "Duck_", "Race_", "Doctor_" };
    [SerializeField]
    private CanvasGroup portalMap;
    [SerializeField]
    private CanvasGroup outsideMap;
    [SerializeField]
    private Button portalMapBtn;
    [SerializeField]
    private Button outsideMapbtn;
    [Header("Side Bar Choice Info")]
    // Choice name
    [SerializeField]
    private TMP_Text choiceLabel;
    // Displays whether the player has fully completed that choice
    [SerializeField]
    private Image choiceCheckmark;
    // Color tied to that choice's route
    [SerializeField]
    private Image choiceLabelBG;
    // Thumbnail of that choice
    [SerializeField]
    private Image choiceThumbnail;
    // Displays how many of the next choices the player has already completed
    [SerializeField]
    private TMP_Text choicesCompletedLabel;

    void Start()
    {
        // Gets components
        mapMenu = GetComponent<CanvasGroup>();
        sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();

        // Adds LoadChoiceMap and DisplayInfo functions to each map button
        AddMapBtnFunctions();
    }

    // For Title Screen scene to start game
    public void StartGame()
    {
        LoadChoiceMap("Start_", false);
    }

    // Load choice from the map menu
    public void LoadChoiceMap(string id, bool inMapMenu)
    {
        // Allows the player to immediately skip to the start of the that choice's choices 
        // gm.isSkipping = true;

        // Loads choice use button manager if the player is already in the main scene   
        if (SceneManager.GetActiveScene().name == "Main Game")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetComponents();

            // Closes the map menu
            if (inMapMenu)
                iMenu.CloseMenu();

            // Closes the pause menu and resumes the game
            iMenu.Resume();

            // If the skip text is visable on screen when selection is made
            if (gm.skipText.color.a != 0)
            {
                // Debug.Log($"Skip - Text Popup");
                gm.fadeTextAni.Play("Invisible Text");
            }
            
            // Loads choice with selected id
            gm.LoadChoice(id);
        }
        // If the player is in the main menu
        else
        {
            // Closes the map menu
            if (inMapMenu)
                iMenu.CloseMenu();
                
            // Saves the chosen id to be loaded at start by button manager
            PlayerPrefs.SetString("Current ChoiceID", id);
            // Loads scene
            SceneManager.LoadScene("Main Game");
        }
    }

    // Adds LoadChoiceMap function to each map button
    void AddMapBtnFunctions()
    {
        // Gets all the choice buttons on the map
        Button[] mapBtns = gameObject.GetComponentsInChildren<Button>();

        // Adds function to each button
        foreach (Button btn in mapBtns)
        {
            // Only checks objects with potential of being an id
            if (btn.gameObject.name.Contains("_"))
            {
                if (sm.choiceDict.TryGetValue(btn.gameObject.name, out ChoiceInfo choice))
                {
                    // Loads choice from the map
                    btn.onClick.AddListener(() => LoadChoiceMap(btn.gameObject.name, true));

                    // Adds EventTrigger for pointer enter to display the choice's info
                    var trigger = btn.gameObject.AddComponent<EventTrigger>();
                    var entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    entry.callback.AddListener((data) =>
                    {
                        // Will displays the choice's information if the player has reached the choice
                        if (btn.interactable)
                        {
                            // Displays the choice's information
                            DisplayChoiceInfo(choice, btn.GetComponent<Image>().color);
                        }
                    });
                    trigger.triggers.Add(entry);
                }
                else
                {
                    Debug.Log($"ID - {btn.gameObject.name} - not found in the system when checking in AddMapBtnFunctions()");
                }
            }
        }
    }

    // Updates map menu buttons to show which choices the player has completed
    void UpdateMapBtns()
    {
        // Gets all the choice buttons on the map
        Button[] mapBtns = mapContents.GetComponentsInChildren<Button>();

        foreach (Button btn in mapBtns)
        {
            // Only checks buttons with potential of being an id
            if (btn.gameObject.name.Contains("_"))
            {   
                if (sm.choiceDict.TryGetValue(btn.gameObject.name, out ChoiceInfo choice))
                {
                    // Skips updating the choice map btn if the player has already 100% the choice
                    if (!choice.updateDisplay)
                    {
                        // Debug.Log($"ChoiceID {choice.choiceID} is fully complete, skipping updating map btn");
                        continue;
                    }

                    // Gets the checkmark image from the button
                    Image checkmark = btn.transform.Find("Checkmark").GetComponent<Image>();
                    
                    // Checks if the player has completed the choice or has the debug menu enabled
                    if (choice.hasComplete || iMenu.completeOverride)
                    {
                        // Enables the button
                        btn.interactable = true;

                        // Checks if the player have 100% the choice
                        var (isFullyComplete, _) = CheckChoiceCompletion(choice);

                        // If the player has 100% the choice, it will mark it as fully complete
                        if (isFullyComplete)
                        {
                            if (!iMenu.completeOverride)
                                choice.updateDisplay = false;
                            checkmark.enabled = true;
                        }
                        else
                        {
                            checkmark.enabled = false;
                        }
                    }
                    // If the player has not gotten to that choice yet the button is disabled
                    else
                    {
                        btn.interactable = false;
                        checkmark.enabled = false;
                    }
                }
                else
                {
                    Debug.Log($"ID - {btn.gameObject.name} - not found in the system when checking in UpdateMapBtns()");
                }
            }
        }
    }

    // Checks if the player has completed all the choices for a choice
    (bool, int) CheckChoiceCompletion(ChoiceInfo choice)
    {
        // Debug.Log($"Checking Choice {choice.choiceID}");

        int completedChoices = 0;

        // Goes through each nextChoiceID store in the choice
        foreach (ChoiceInfo choiceInfo in choice.nextChoices)
        {
            if (sm.choiceDict.ContainsKey(choiceInfo.choiceID))
            {  
                // Debug.Log($"Next Choice {nextChoice.choiceID} {nextChoice.hasComplete}");
                if (sm.choiceDict[choiceInfo.choiceID].hasComplete)
                    completedChoices += 1;
            }
            else
            {
                Debug.Log($"ID - {choiceInfo.choiceID} - not found in the system when checking in CheckChoiceCompletion() for choice {choice.choiceID}");
            }
        }

        // Checks whether the player has completed all the achievements tied to that choice
        bool achieveComplete = true;
        if (choice.achievements.Count > 0)
        {
            foreach (AchievementInfo achievementInfo in choice.achievements)
            {
                if (sm.achieveDict.ContainsKey(achievementInfo.achieveID))
                {   
                    // Marks it false if the player has not completed all the achievements
                    if (!sm.achieveDict[achievementInfo.achieveID].hasUnlocked)
                        achieveComplete = false;
                }
                else
                {
                    // Debug.Log($"AchieveID {id} not found in system in CheckChoiceCompletion()");
                }
            }
        }
        else
        {
            // Debug.Log($"No achievements found for ChoiceID {choice.choiceID} in CheckChoiceCompletion()");
        }

        // Checks whether the player has collected all the letters tied to that choice
        bool letterComplete = true;
        if (choice.letterIDs.Count > 0)
        {
            foreach (LetterID id in choice.letterIDs)
            {
                if (sm.letterDict.TryGetValue(id, out LetterInfo letter))
                {   
                    // Marks it false if the player has not completed all the achievements
                    if (!letter.hasObtained)
                        letterComplete = false;
                }
                else
                {
                    // Debug.Log($"LetterID {id} not found in system in CheckChoiceCompletion()");
                }
            }
        }
        else
        {
            // Debug.Log($"No letters found for ChoiceID {choice.choiceID} in CheckChoiceCompletion()");
        }

        // If the player has completed all the next choices it returns true, alongside the total of completed choices
        return (completedChoices == choice.nextChoices.Count && achieveComplete && letterComplete, completedChoices);
    }

    // Displays the info on the sidebar of what choice the player is currently highlighting
    void DisplayChoiceInfo(ChoiceInfo choice, Color color)
    {
        // Debug.Log($"Displaying choice {choice.choiceID}");
        // if (choice.mapName != "") {Debug.Log($"Map name {choice.mapName}");}

        // Displays the choice's map name, if the field is blank it defaults to the choice's choice field
        choiceLabel.text = choice.mapName != "" ? choice.mapName : choice.choice;

        // Checks the choice's completion
        var (isComplete, completedChoices) = CheckChoiceCompletion(choice);
        choiceCheckmark.enabled = isComplete;

        // Changes the color to that of the choice's route
        choiceLabelBG.color = color;

        // Display's the choice's thumbnail
        choiceThumbnail.sprite = choice.thumbnail;

        // Changes the text's style depending on whether the player has completed all the next choices
        string style = completedChoices == choice.nextChoices.Count ? "Complete" : "Normal";
        choicesCompletedLabel.text = $"Choices Completed: <style=\"{style}\">{completedChoices}/{choice.nextChoices.Count}</style>";
    }

    // Gets necessary components from the current scene if the script does not already have it
    void GetComponents()
    {
        if (!gm)
        {
            gm = FindAnyObjectByType<GameManager>();
        }
    }

    // Opens Map Menu
    public void OpenMapMenu()
    {
        if (SceneManager.GetActiveScene().name == "Main Game")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetComponents();
        }
        
        // Updates map menu buttons based on player progression
        UpdateMapBtns();

        // Displays which choice the player is currently at
        DisplayWya();

        iMenu.OpenRegularMenu(mapMenu);
    }

    // Displays which choice the player is currently at
    void DisplayWya()
    {
        string choiceID;

        // If the player is in the game it centers the choice on the current on the player is on
        if (SceneManager.GetActiveScene().name == "Main Game")
            choiceID = gm.currentChoice.choiceID;
        // If in main menu centers it on the start choice
        else
            choiceID = "Start_";

        // Converts option which are essentially the same vid to the one displayed on the map
        switch (choiceID)
        {
            case "Retry_":
                choiceID = "Start_";
                break;
        }

        // Gets position of the new wyaIcon
        Button[] mapBtns = mapContents.GetComponentsInChildren<Button>(true);
        Button targetBtn = null;
        ChoiceInfo mapChoice = null;

        // Checking map for current choice position
        targetBtn = MapBtnCheck(choiceID, mapBtns);

        if (targetBtn)
            mapChoice = sm.choiceDict[choiceID];

        // If current choice is not on the map, it searches for the previous choice
        if (!targetBtn)
        {
            string prevChoice = gm.prevChoice;
            targetBtn = MapBtnCheck(prevChoice, mapBtns);

            if (targetBtn)
                sm.choiceDict.TryGetValue(prevChoice, out mapChoice);

            // If at the end of a path it insteads searches for the previous choice
            if (!targetBtn || mapChoice)
            {
                string[] parts = gm.prevChoice.Split('_');
                prevChoice = string.Join("_", parts, 0, parts.Length - 1);
                
                while (!targetBtn && !mapChoice)
                {
                    // Debug.Log($"prevChoice {prevChoice}");

                    if (parts.Length == 2)
                        prevChoice = $"{prevChoice}_";

                    // Checking map for the previous choice position
                    targetBtn = MapBtnCheck(prevChoice, mapBtns);

                    if (targetBtn)
                    {
                        sm.choiceDict.TryGetValue(prevChoice, out mapChoice);
                        if (mapChoice)
                            break;
                        else
                            Debug.Log($"ID - {prevChoice} - not found in the system when checking in OpenMapMenu()");
                    }

                    parts = prevChoice.Split('_');
                    prevChoice = string.Join("_", parts, 0, parts.Length - 1);
                }
            }
        }

        // Places or spawns wyaIcon at target position
        if (wyaIconStorage)
        {
            // Debug.Log("Changing wya icon's position");
            wyaIconStorage.transform.position = new Vector2(targetBtn.transform.position.x, targetBtn.transform.position.y + 57.5f);
            // Sets as parent of target so it does not show up in both menus
            wyaIconStorage.transform.SetParent(targetBtn.transform, true);
        }
        else
        {
            // Debug.Log("Spawning wya icon");
            wyaIconStorage = Instantiate(wyaIcon, new Vector2(targetBtn.transform.position.x, targetBtn.transform.position.y + 57.5f),
                Quaternion.identity, targetBtn.transform);       
        }
        
        // Displays the current choice
        DisplayChoiceInfo(mapChoice, targetBtn.GetComponent<Image>().color);

        // Opens route map where the player is currently
        OpenRouteMap(mapChoice.choiceID);
    }

    void OpenRouteMap(string id)
    {
        string[] parts = id.Split('_');
        string choiceID = string.Join("_", parts, 0, 1);

        choiceID = $"{choiceID}_";

        // Opens outside map
        if (outsideMaps.Contains(choiceID) || !sm.choiceDict["Portal_"].hasComplete && sm.choiceDict["Outside_"].hasComplete)
        {
            // Debug.Log("Opening Outside map");
            iMenu.MenuOpenClose(portalMap, false);
            iMenu.MenuOpenClose(outsideMap, true);
        }
        // Opens portal map
        else
        {
            // Debug.Log("Opening Portal map");
            iMenu.MenuOpenClose(outsideMap, false);
            iMenu.MenuOpenClose(portalMap, true);
        }
        
        portalMapBtn.interactable = sm.choiceDict["Portal_"].hasComplete || iMenu.completeOverride;
        portalMapBtn.GetComponentInChildren<TMP_Text>().enabled = portalMapBtn.interactable;
        outsideMapbtn.interactable = sm.choiceDict["Outside_"].hasComplete || iMenu.completeOverride;
        outsideMapbtn.GetComponentInChildren<TMP_Text>().enabled = outsideMapbtn.interactable;
    }

    // Checking map for choice button position
    Button MapBtnCheck(string choiceID,Button[] mapBtns)
    {
        foreach (Button btn in mapBtns)
            if (btn.gameObject.name == choiceID)
                return btn;

        return null;
    }
}
