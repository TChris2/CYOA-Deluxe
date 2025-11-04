using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// Functionality for the map menu
public class MapMenuFunctions : MonoBehaviour
{
    // Override to access everything the map menu without already completing it
    public bool completeOverride;
    // Stores the default scale of the map menu
    [SerializeField]
    private Transform mapContent;
    [SerializeField]
    private GameObject wyaIcon;
    // Keeps track of instantiate icons 
    private GameObject wyaIconStorage;
    // Canvas groups
    [HideInInspector]
    public CanvasGroup mapMenu;
    // Stores the canvas group of the previous menu
    [HideInInspector]
    public CanvasGroup prevMenu;
    // Scripts
    SaveManager sm;
    ButtonManager bm;
    InputMenu iMenu;
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
        // Gets comps
        mapMenu = GetComponent<CanvasGroup>();
        sm = GameObject.Find("SaveManager").GetComponent<SaveManager>();

        // Adds LoadChoiceMap function to each map button
        AddMapBtnFunctions();
    }

    // Load choice from the map
    public void LoadChoiceMap(string id, bool inMapMenu)
    {
        // Allows the player to immediately skip to the start of the that choice's choices 
        // bm.isSkipping = true;

        // Loads choice use button manager if the player is already in the main scene   
        if (SceneManager.GetActiveScene().name == "Main Game Video")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetComponents();
            
            // Closes the map menu
            if (inMapMenu)
                CloseMapMenu();

            // Closes the pause menu and resumes the game
            iMenu.Resume();
            
            // Loads choice with selected id
            bm.LoadChoice(id);
        }
        // If the player is in the main menu
        else
        {
            // Saves the chosen id to be loaded at start by button manager
            PlayerPrefs.SetString("Current ChoiceID", id);
            // Loads scene
            SceneManager.LoadScene("Main Game Video");
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
                        // Displays the choice's information
                        DisplayChoiceInfo(choice, btn.GetComponent<Image>().color);
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
        Button[] mapBtns = gameObject.GetComponentsInChildren<Button>();

        foreach (Button btn in mapBtns)
        {
            // Only checks objects with potential of being an id
            if (btn.gameObject.name.Contains("_"))
            {
                if (sm.choiceDict.TryGetValue(btn.gameObject.name, out ChoiceInfo choice))
                {
                    // Gets the checkmark image from the button
                    Image checkmark = btn.transform.Find("Checkmark").GetComponent<Image>();

                    // Checks if the player has completed the choice or has the debug menu enabled
                    if (choice.hasComplete || completeOverride)
                    {
                        // Enables the button
                        btn.interactable = true;

                        // If the player has completed all the available choices the choice will be displayed as complete
                        var (isComplete, completedChoices) = CheckChoiceCompletion(choice);

                        checkmark.enabled = isComplete;
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
        // Debug.Log($"Current Choice {choice.choiceID}");

        int completedChoices = 0;

        // Goes through each nextChoiceID store in the choice
        foreach (string id in choice.nextChoiceIDs)
        {
            if (sm.choiceDict.TryGetValue(id, out ChoiceInfo nextChoice))
            {
                // Debug.Log($"Next Choice {nextChoice.choiceID} {nextChoice.hasComplete}");
                if (nextChoice.hasComplete)
                    completedChoices += 1;
            }
            else
            {
                Debug.Log($"ID - {id} - not found in the system when checking in CheckChoiceCompletion()");
            }
        }

        // If the player has completed all the nextChoiceIDs it returns true, alongside the total of completed choices
        return (completedChoices == choice.nextChoiceIDs.Count, completedChoices);
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
        string style = isComplete ? "Complete" : "Normal";
        choicesCompletedLabel.text = $"Choices Completed: <style=\"{style}\">{completedChoices}/{choice.nextChoiceIDs.Count}</style>";
    }

    // Gets necessary components from the current scene if the script does not already have it
    void GetComponents()
    {
        if (!bm || !iMenu)
        {
            bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();
            iMenu = bm.GetComponent<InputMenu>();
        }
    }

    // Opens Map Menu
    public void OpenMapMenu(CanvasGroup menu)
    {
        // Stores previous menu and disables it
        prevMenu = menu;
        prevMenu.interactable = false;

        // Gets necessary components from the current scene if the script does not already have it
        GetComponents();

        // Updates map menu buttons based on player progression
        UpdateMapBtns();

        string choiceID;

        // If the player is in the game it centers the choice on the current on the player is on
        if (SceneManager.GetActiveScene().name == "Main Game Video")
            choiceID = bm.currentChoice.choiceID.ToString();
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

        // Delete the previous wyaIcon
        if (wyaIconStorage)
            Destroy(wyaIconStorage);

        // Gets position of the new wyaIcon
        Button[] mapBtns = mapContent.GetComponentsInChildren<Button>(true);
        Button targetBtn = null;

        foreach (Button btn in mapBtns)
        {
            if (btn.gameObject.name == choiceID)
            {
                targetBtn = btn;
                break;
            }
        }

        ChoiceInfo mapChoice = bm.currentChoice;

        // If at the end of a path it insteads searches for the previous choice
        if (targetBtn == null)
        {
            string[] parts = choiceID.Split('_');
            string prevChoice = string.Join("_", parts, 0, parts.Length - 1);

            if (parts.Length == 2)
                prevChoice = $"{prevChoice}_";
        
            foreach (Button btn in mapBtns)
            {
                if (btn.gameObject.name == prevChoice)
                {
                    targetBtn = btn;
                    break;
                }
            }

            sm.choiceDict.TryGetValue(prevChoice, out mapChoice);
            if (!mapChoice)
            {
                Debug.Log($"ID - {prevChoice} - not found in the system when checking in OpenMapMenu()");
            }
        }

        // Spawns wya icon at current choice
        wyaIconStorage = Instantiate(wyaIcon, new Vector2(targetBtn.transform.position.x, targetBtn.transform.position.y + 57.5f),
            Quaternion.identity, targetBtn.transform);

        // Displays the current choice
        DisplayChoiceInfo(mapChoice, targetBtn.GetComponent<Image>().color);

        // Opens map menu
        MenuOpenClose(mapMenu, true);
    }

    // Closes map menu
    public void CloseMapMenu()
    {
        prevMenu.interactable = true;
        MenuOpenClose(mapMenu, false);
    }

    // Opens or closes selected menus
    public void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        menu.interactable = isOpen;
        menu.alpha = isOpen ? 1 : 0;
        menu.blocksRaycasts = isOpen;
    }
}
