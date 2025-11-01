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
    [SerializeField]
    private TMP_Text choiceLabel;
    [SerializeField]
    private Image choiceCheckmark;
    [SerializeField]
    private Image choiceLabelBG;
    [SerializeField]
    private Image choiceThumbnail;
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
        // bm.isSkipping = true;

        // Loads choice use button manager if the player is already in the main scene   
        if (SceneManager.GetActiveScene().name == "Main Game Video")
        {
            // Closes map and pause screen menus
            if (inMapMenu)
                CloseMapMenu();

            iMenu.Resume();
            
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
            if (System.Enum.TryParse(btn.gameObject.name, out ChoiceID choiceID))
            {
                btn.onClick.AddListener(() => LoadChoiceMap(btn.gameObject.name, true));

                // Adds EventTrigger for pointer enter
                var trigger = btn.gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) =>
                {
                    if (sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))
                    {
                        DisplayChoiceInfo(choice, btn.GetComponent<Image>().color);
                    }
                });
                trigger.triggers.Add(entry);
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
            if (System.Enum.TryParse(btn.gameObject.name, out ChoiceID choiceID))
            {
                // Debug.Log($"{btn.gameObject.name} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                // Updates button to display how many choices the player has completed
                if (sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))
                {
                    TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
                    Image checkmark = btn.transform.Find("Checkmark").GetComponent<Image>();

                    if (choice.hasComplete || completeOverride)
                    {
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
                        // text.text = "";
                    }
                }
            }
        }
    }

    // Checks if the player has completed all the choices for a choice
    (bool, int) CheckChoiceCompletion(ChoiceInfo choice)
    {
        // Debug.Log($"Current Choice {choice.choiceID}");

        int completedChoices = 0;

        foreach (ChoiceID id in choice.nextChoiceIDs)
        {
            if (sm.choiceDict.TryGetValue(id, out ChoiceInfo nextChoice))
            {
                // Debug.Log($"Next Choice {nextChoice.choiceID} {nextChoice.hasComplete}");
                if (nextChoice.hasComplete)
                    completedChoices += 1;
            }
        }

        return (completedChoices == choice.nextChoiceIDs.Length, completedChoices);
    }

    // Displays the info on the sidebar of what choice the player is currently highlighting
    void DisplayChoiceInfo(ChoiceInfo choice, Color color)
    {
        // Debug.Log($"Displaying choice {choice.choiceID}");
        choiceLabel.text = choice.choice;

        var (isComplete, completedChoices) = CheckChoiceCompletion(choice);
        choiceCheckmark.enabled = isComplete;

        choiceLabelBG.color = color;

        choiceThumbnail.sprite = choice.thumbnail;

        string style = isComplete ? "Complete" : "Normal";
        choicesCompletedLabel.text = $"Choices Completed: <style=\"{style}\">{completedChoices}/{choice.nextChoiceIDs.Length}</style>";
    }

    // Opens Map Menu
    public void OpenMapMenu(CanvasGroup menu)
    {
        // Stores previous menu and disables it
        prevMenu = menu;
        prevMenu.interactable = false;

        // Gets necessary components from the current scene if the script does not already have it
        if (!bm || !iMenu)
        {
            bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();
            iMenu = bm.GetComponent<InputMenu>();
        }

        // Updates map menu buttons based on player progression
        UpdateMapBtns();

        string choiceID;

        // Delete the previous wyaIcon
        if (wyaIconStorage)
            Destroy(wyaIconStorage);

        // If the player is in the game it centers the choice on the current on the player is on
        if (SceneManager.GetActiveScene().name == "Main Game Video")
            choiceID = bm.currentChoice.choiceID.ToString();
        // If in main menu centers it on the start choice
        else
            choiceID = ChoiceID.Start_.ToString();

        // Converts option which are essentially the same vid to the one displayed on the map
        switch (choiceID)
        {
            case "Retry_":
                choiceID = "Start_";
                break;
        }

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

        // If at the end of a path
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

            sm.choiceDict.TryGetValue(System.Enum.TryParse(prevChoice, out ChoiceID id) ? id : default, out mapChoice);
        }

        Color btnColor = targetBtn.GetComponent<Image>().color;
        
        // Spawns wya icon at current choice
        wyaIconStorage = Instantiate(wyaIcon, new Vector2(targetBtn.transform.position.x, targetBtn.transform.position.y + 57.5f),
            Quaternion.identity, targetBtn.transform);

        DisplayChoiceInfo(mapChoice, btnColor);

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
