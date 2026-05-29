// #define DEBUG_MakeConnections

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;

/// <summary>
/// Functionality for the map menu
/// </summary>
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
    [HideInInspector]
    public CanvasGroup mapMenu;
    // Scripts
    SaveManager sm;
    GameManager gm;
    TransitionManager tm;
    InputMenu iMenu;
    [Header("Map Menu")]
    HashSet<string> portalMaps = new HashSet<string> { "Portal_", "Minecraft_", "BOTW_" };
    HashSet<string> outsideMaps = new HashSet<string> { "Outside_", "Tesco_", "Duck_", "Race_", "Doctor_" };
    [SerializeField]
    private CanvasGroup portalMap;
    [SerializeField]
    private CanvasGroup outsideMap;
    [SerializeField]
    private GameObject portalMapBtn;
    [SerializeField]
    private GameObject outsideMapbtn;
    [SerializeField]
    private GameObject startMapBtn;
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
    [SerializeField] 
    private GameObject mapArrow;
    [HideInInspector]
    public bool inMapMenu;
    // Checks to see if the map is ready to be interacted with
    bool isMapReady;
    Button[] mapBtns;

    void Start()
    {
        // Gets components
        mapMenu = GetComponent<CanvasGroup>();
        sm = FindAnyObjectByType<SaveManager>();
        tm = FindAnyObjectByType<TransitionManager>();
        iMenu = FindAnyObjectByType<InputMenu>();

        // Adds LoadChoiceMap and DisplayInfo functions to each map button
        AddMapBtnFunctions();
    }

    #region Loading Selected Map Choice

    /// <summary>
    /// For Title Screen scene to start game
    /// </summary>
    public void StartGame()
    {
        PlayerPrefs.SetString("Current ChoiceID", "Start_");
        tm.actionDelay = 2.5f;
        tm.onTransition += () => tm.ChangeScene("Main Game");
        tm.FadeOut(FadeType.PlainBlack);
    }

    /// <summary>
    /// Load choice from the map menu
    /// </summary>
    public void LoadChoiceMap(string id, bool inMenu)
    {
        inMapMenu = inMenu;
        // Allows the player to immediately skip to the start of the that choice's choices 
        // gm.isSkipping = true;

        // Loads choice use button manager if the player is already in the main scene   
        if (SceneManager.GetActiveScene().name == "Main Game")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetComponents();

            // Closes the pause menu and resumes the game
            iMenu.Resume();
            
            // Loads choice with selected id
            gm.LoadChoice(id);
        }
        // If the player is in the main menu
        else if (SceneManager.GetActiveScene().name == "Title Screen")
        {
            // Saves the chosen id to be loaded at start by button manager
            PlayerPrefs.SetString("Current ChoiceID", id);
            SceneManager.LoadScene("Main Game");
        }
    }

    #endregion
    #region Initializing Map Btns 

    /// <summary>
    /// Adds LoadChoiceMap function to each map button
    /// </summary>
    void AddMapBtnFunctions()
    {
        // Gets all the choice buttons on the map
        if (mapBtns == null)
            mapBtns = gameObject.GetComponentsInChildren<Button>();

        #if DEBUG_MakeConnections
            Vector2 startPos, endPos;
        #endif

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
                        if (btn.interactable && isMapReady)
                        {
                            // Displays the choice's information
                            DisplayChoiceInfo(choice, btn.GetComponent<Image>().color);
                        }
                    });
                    trigger.triggers.Add(entry);

                    #if DEBUG_MakeConnections
                        if (choice.nextChoices.Count > 0)
                        {
                            foreach (ChoiceInfo nextChoice in choice.nextChoices)
                            {
                                // Find the matching button in mapBtns for this nextChoice
                                Button nextBtn = System.Array.Find(mapBtns, b => b.gameObject.name == nextChoice.choiceID);

                                if (nextBtn != null)
                                {
                                    startPos = btn.GetComponent<RectTransform>().position;
                                    endPos = nextBtn.GetComponent<RectTransform>().position;

                                    PlaceConnections(startPos, endPos, btn.transform.GetChild(1));
                                }
                            }
                        }
                    #endif
                }
                else
                {
                    Debug.Log($"MapMenu: ID - {btn.gameObject.name} - not found in the system when checking in AddMapBtnFunctions()");
                }
            }
        }

        #if DEBUG_MakeConnections
            startPos = startMapBtn.GetComponent<RectTransform>().position;
            endPos = outsideMapbtn.GetComponent<RectTransform>().position;

            PlaceConnections(startPos, endPos, startMapBtn.transform.GetChild(1));
            endPos = portalMapBtn.GetComponent<RectTransform>().position;
            PlaceConnections(startPos, endPos, startMapBtn.transform.GetChild(1));
        #endif
    }

    /// <summary>
    /// Places connections for each of the choices
    /// </summary>
    public void PlaceConnections(Vector2 startPos, Vector2 endPos, Transform arrowHolder)
    {
        // Makes the arrows a child of the choice button
        GameObject obj = Instantiate(mapArrow, arrowHolder);
        RectTransform rt = obj.GetComponent<RectTransform>();

        // Convert world positions into the button's local space
        Vector2 startLocal = arrowHolder.InverseTransformPoint(startPos);
        Vector2 endLocal = arrowHolder.InverseTransformPoint(endPos);

        // Position at midpoint in local space
        rt.anchoredPosition = (startLocal + endLocal) / 2f;

        // Determines the length of the arrow
        float distance = Vector2.Distance(startLocal, endLocal);
        rt.sizeDelta = new Vector2(distance - 40, rt.sizeDelta.y);

        // Rotates the arrow
        Vector2 delta = endLocal - startLocal;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Pivot centered
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    #endregion
    #region Loading Map Btns

    /// <summary>
    /// Updates map menu buttons to show which choices the player has completed
    /// </summary>
    void UpdateMapBtns()
    {
        // Gets all the choice buttons on the map
        if (mapBtns == null)
            mapBtns = mapContents.GetComponentsInChildren<Button>(true);

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
                        // Debug.Log($"MapMenu: ChoiceID {choice.choiceID} is fully complete, skipping updating map btn");
                        continue;
                    }

                    // Gets the checkmark image from the button
                    Image checkmark = btn.transform.Find("Checkmark").GetComponent<Image>();
                    
                    // Checks if the player has completed the choice or has the debug menu enabled
                    if (choice.hasComplete || iMenu.completeOverride)
                    {
                        // Enables the button
                        btn.gameObject.SetActive(true);
                        
                        // Updates connections to next choices
                        if (choice.nextChoices.Count > 0)
                        {
                            // Gets all the next choices that are on the map
                            if (choice.mapNextChoices.Count == 0)
                            {
                                choice.mapNextChoices = choice.nextChoices
                                    .Where(c => sm.choiceDict.ContainsKey(c.choiceID) && sm.choiceDict[c.choiceID].isOnMap)
                                    .Select(c => sm.choiceDict[c.choiceID])
                                    .ToList();
                            }
                            
                            // Checks to see which choices the player have complete or not
                            for (int i = 0; i < choice.mapNextChoices.Count; i++)
                            {
                                bool isActive = choice.mapNextChoices[i].hasComplete || iMenu.completeOverride;
                                btn.transform.GetChild(1).GetChild(i).gameObject.SetActive(isActive);
                            }
                        }

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
                        btn.gameObject.SetActive(false);
                        checkmark.enabled = false;
                    }
                }
                else
                {
                    Debug.Log($"MapMenu: ID - {btn.gameObject.name} - not found in the system when checking in UpdateMapBtns()");
                }
            }
        }

        UpdateStartMapBtn();
    }

    /// <summary>
    /// Checks if the player has completed all the choices for a choice
    /// </summary>
    (bool, int) CheckChoiceCompletion(ChoiceInfo choice)
    {
        // Debug.Log($"MapMenu: Checking Choice {choice.choiceID}");

        int completedChoices = 0;

        // Goes through each nextChoiceID store in the choice
        foreach (ChoiceInfo choiceInfo in choice.nextChoices)
        {
            if (sm.choiceDict.ContainsKey(choiceInfo.choiceID))
            {  
                // Debug.Log($"MapMenu: Next Choice {nextChoice.choiceID} {nextChoice.hasComplete}");
                if (sm.choiceDict[choiceInfo.choiceID].hasComplete)
                    completedChoices += 1;
            }
            else
            {
                Debug.Log($"MapMenu: ID - {choiceInfo.choiceID} - not found in the system when checking in CheckChoiceCompletion() for choice {choice.choiceID}");
            }
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
                    // Debug.Log($"MapMenu: LetterID {id} not found in system in CheckChoiceCompletion()");
                }
            }
        }
        else
        {
            // Debug.Log($"MapMenu: No letters found for ChoiceID {choice.choiceID} in CheckChoiceCompletion()");
        }

        // If the player has completed all the next choices it returns true, alongside the total of completed choices
        return (completedChoices == choice.nextChoices.Count && letterComplete, completedChoices);
    }

    /// <summary>
    /// Displays the info on the sidebar of what choice the player is currently highlighting
    /// </summary>
    void DisplayChoiceInfo(ChoiceInfo choice, Color color)
    {
        // Debug.Log($"MapMenu: Displaying choice {choice.choiceID}");
        // if (choice.mapName != "") {Debug.Log($"MapMenu: Map name {choice.mapName}");}

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

    #endregion
    #region Opening Map Menu

    /// <summary>
    /// Gets necessary components from the current scene if the script does not already have it
    /// </summary>
    void GetComponents()
    {
        if (!gm)
        {
            gm = FindAnyObjectByType<GameManager>();
        }
    }

    /// <summary>
    /// Opens Map Menu
    /// </summary>
    public void OpenMapMenu()
    {
        if (SceneManager.GetActiveScene().name == "Main Game")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetComponents();
        }
        
        // Displays which choice the player is currently at
        DisplayWya();

        // Updates map menu buttons based on player progression
        UpdateMapBtns();

        iMenu.OpenRegularMenu(mapMenu);
    }

    /// <summary>
    /// Displays which choice the player is currently at
    /// </summary>
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

        // If the choice is to be displayed as another on the Map Menu
        if (gm != null && gm.currentChoice.mapDisplayChoice != null)
        {
            Debug.Log($"Map Menu: Choice {gm.currentChoice.choiceID} to be displayed as {gm.currentChoice.mapDisplayChoice.choiceID} on the Map Menu");
            choiceID = gm.currentChoice.mapDisplayChoice.choiceID;
        }

        // Gets position of the new wyaIcon
        if (mapBtns == null)
            mapBtns = mapContents.GetComponentsInChildren<Button>(true);
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
                    // Debug.Log($"MapMenu: prevChoice {prevChoice}");

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
                            Debug.Log($"MapMenu: ID - {prevChoice} - not found in the system when checking in OpenMapMenu()");
                    }

                    parts = prevChoice.Split('_');
                    prevChoice = string.Join("_", parts, 0, parts.Length - 1);
                }
            }
        }

        // Places or spawns wyaIcon at target position
        if (wyaIconStorage)
        {
            // Debug.Log("MapMenu: Changing wya icon's position");
            wyaIconStorage.transform.SetParent(targetBtn.transform, false);
            PositionWyaIcon(targetBtn);
        }
        else
        {
            // Debug.Log("MapMenu: Spawning wya icon");
            wyaIconStorage = Instantiate(wyaIcon, targetBtn.transform);
            PositionWyaIcon(targetBtn);      
        }
        
        // Displays the current choice
        DisplayChoiceInfo(mapChoice, targetBtn.GetComponent<Image>().color);

        // Opens route map where the player is currently
        OpenRouteMap(mapChoice.choiceID);
    }

    /// <summary>
    /// Positions the icon
    /// </summary>
    void PositionWyaIcon(Button targetBtn)
    {
        // Gets components
        RectTransform btnRect = targetBtn.GetComponent<RectTransform>();
        RectTransform iconRect = wyaIconStorage.GetComponent<RectTransform>();

        wyaIconStorage.transform.SetParent(targetBtn.transform, false);
        // Sets the new position
        iconRect.anchoredPosition = new Vector2(0f, btnRect.rect.height / 2f + 40.5f);
    }

    /// <summary>
    /// Opens route map where the player is currently
    /// </summary>
    void OpenRouteMap(string id)
    {
        string[] parts = id.Split('_');
        string choiceID = string.Join("_", parts, 0, 1);

        choiceID = $"{choiceID}_";
        
        bool isPortalMapOpen = !outsideMaps.Contains(choiceID);

        // Opens proper route map
        iMenu.MenuOpenClose(portalMap, isPortalMapOpen);
        iMenu.MenuOpenClose(outsideMap, !isPortalMapOpen);

        // Pops button to the opposing map on screen
        portalMapBtn.SetActive(sm.choiceDict["Portal_"].hasComplete || iMenu.completeOverride);
        outsideMapbtn.SetActive(sm.choiceDict["Outside_"].hasComplete || iMenu.completeOverride);
        
        UpdateStartMapBtn();
    }

    /// <summary>
    /// Updates the map arrows for Start_
    /// </summary>
    public void UpdateStartMapBtn()
    {   
        bool isPortalMapOpen = portalMap.interactable;
        bool isPortalMapUnlocked = sm.choiceDict["Portal_"].hasComplete || iMenu.completeOverride;
        bool isOutsideMapUnlocked = sm.choiceDict["Outside_"].hasComplete || iMenu.completeOverride;

        // Updates Start_'s regular arrows
        startMapBtn.transform.GetChild(1).GetChild(0).gameObject.SetActive(
            isPortalMapOpen && isPortalMapUnlocked);
        startMapBtn.transform.GetChild(1).GetChild(1).gameObject.SetActive(
            !isPortalMapOpen && isOutsideMapUnlocked);

        // Updates the arrows to the route map buttons
        startMapBtn.transform.GetChild(1).GetChild(2).gameObject.SetActive(isPortalMapOpen && isOutsideMapUnlocked);
        startMapBtn.transform.GetChild(1).GetChild(3).gameObject.SetActive(!isPortalMapOpen && isPortalMapUnlocked);
    }

    /// <summary>
    /// Checking map for choice button
    /// </summary>
    Button MapBtnCheck(string choiceID,Button[] mapBtns)
    {
        foreach (Button btn in mapBtns)
            if (btn.gameObject.name == choiceID)
                return btn;

        return null;
    }

    // Updates whether the map is ready to be interacted with
    // Used as an animation event
    public void SetMapReady(int isReady)
    {
        isMapReady = isReady == 1 ? true : false;
    }

    #endregion
}
