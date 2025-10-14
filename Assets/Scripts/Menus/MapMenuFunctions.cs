using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Functionality for the map menu
public class MapMenuFunctions : MonoBehaviour
{
    // Content of the map menu
    [SerializeField]
    private RectTransform mapContent;
    // Slider for controlling the zoom of the map menu
    [SerializeField]
    private Slider zoomSlider;
    // Stores the default scale of the map menu
    Vector3 defaultScale;
    // Override to access everything the map menu without already completing it
    public bool completeOverride;
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


    void Start()
    {
        // Gets comps
        mapMenu = GetComponent<CanvasGroup>();
        sm = GameObject.Find("SaveManager").GetComponent<SaveManager>();

        // Stores default scale
        defaultScale = mapContent.localScale;

        // Adds LoadChoiceMap function to each map button
        AddMapBtnFunctions();
    }

    // Resets zoom of map menu to default
    public void ResetZoom()
    {
        mapContent.localScale = defaultScale;
        zoomSlider.value = defaultScale.x;
    }

    // Controls zoom of map menu
    public void SetZoom()
    {
        mapContent.localScale = new Vector3(zoomSlider.value, zoomSlider.value, 1f);
    }

    // Load choice from the map
    public void LoadChoiceMap(string id, bool inMapMenu)
    {
        // bm.isSkipping = true;

        // Loads choice use button manager if the player is already in the main scene   
        if (SceneManager.GetActiveScene().name == "Main Game Video")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetSceneComps();

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
        Button[] mapBtns = mapContent.GetComponentsInChildren<Button>();

        // Adds function to each button
        foreach (Button btn in mapBtns)
        {
            if (System.Enum.TryParse(btn.gameObject.name, out ChoiceID choiceID))
            {
                btn.onClick.AddListener(() => LoadChoiceMap(btn.gameObject.name, true));
            }
        }
    }

    // Gets necessary components from the current scene if the script does not already have it
    void GetSceneComps()
    {
        if (!bm || !iMenu)
        {
            bm = GameObject.Find("Local UI").GetComponent<ButtonManager>();
            iMenu = bm.GetComponent<InputMenu>();
        }
    }

    // Updates map menu buttons to show which choices the player has completed
    void UpdateMapBtns()
    {
        // Gets all the choice buttons on the map
        Button[] mapBtns = mapContent.GetComponentsInChildren<Button>();


        foreach (Button btn in mapBtns)
        {
            if (System.Enum.TryParse(btn.gameObject.name, out ChoiceID choiceID))
            {
                // Debug.Log($"{btn.gameObject.name} {(sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))}");
                // Updates button to display how many choices the player has completed
                if (sm.choiceDict.TryGetValue(choiceID, out ChoiceInfo choice))
                {
                    TMP_Text text = btn.GetComponentInChildren<TMP_Text>();

                    if (choice.hasComplete || completeOverride)
                    {
                        int completedChoices = 0;

                        // Debug.Log($"Current Choice {choice.choiceID}");

                        foreach (ChoiceID id in choice.nextChoiceIDs)
                        {
                            if (sm.choiceDict.TryGetValue(id, out ChoiceInfo nextChoice))
                            {
                                // Debug.Log($"Next Choice {nextChoice.choiceID} {nextChoice.hasComplete}");
                                if (nextChoice.hasComplete)
                                    completedChoices += 1;
                            }
                        }

                        // If the player has completed all the available choices the text will be displayed in complete style
                        string style = completedChoices == choice.nextChoiceIDs.Length ? "Complete" : "Normal";

                        btn.interactable = true;
                        text.text = $"<style=\"{style}\">{completedChoices}/{choice.nextChoiceIDs.Length}</style>";
                    }
                    // If the player has not gotten to that choice yet the button is disabled
                    else
                    {
                        btn.interactable = false;
                        text.text = "";
                    }
                }
            }
        }
    }

    // Centers on the choice the player is currently at
    void CenterMapItem()
    {
        string choiceID;

        // If the player is in the game it centers the choice on the current on the player is on
        if (SceneManager.GetActiveScene().name == "Main Game Video")
        {
            // Gets necessary components from the current scene if the script does not already have it
            GetSceneComps();

            choiceID = bm.currentChoice.choiceID.ToString();

            if (choiceID == ChoiceID.Retry_.ToString())
                choiceID = ChoiceID.Start_.ToString();
        }
        // If in main menu centers it on the start choice
        else
            choiceID = ChoiceID.Start_.ToString();

        // Debug.Log($"In {SceneManager.GetActiveScene().name} searching for {choiceID}");

        Transform mapItem = mapContent.Find(choiceID);
        // If the player is at the end of a path
        if (!mapItem)
        {
            string[] parts = choiceID.Split('_');

            string prevChoice = string.Join("_", parts, 0, parts.Length - 1);;

            if (parts.Length == 2)
                prevChoice = $"{prevChoice}_";

            // Debug.Log($"{choiceID} not found, searching for prev choice {prevChoice}");

            // Searches instead for the previous choice
            mapItem = mapContent.Find(prevChoice);
        }

        ScrollRect scrollRect = mapContent.GetComponentInParent<ScrollRect>();
        if (!scrollRect) return;

        RectTransform viewport = scrollRect.viewport;
        RectTransform itemRect = mapItem.GetComponent<RectTransform>();

        // Position of item relative to content
        Vector3 itemLocalPos = mapContent.InverseTransformPoint(itemRect.position);
        Vector3 viewportLocalPos = mapContent.InverseTransformPoint(viewport.position);

        // Difference between item center and viewport center
        Vector3 offset = viewportLocalPos - itemLocalPos;

        // Move content by this offset
        Vector3 newPos = mapContent.localPosition + offset;

        // Optional: clamp so content doesn’t scroll too far
        Vector2 minPos = viewport.rect.size * 0.5f - mapContent.rect.size * 0.5f;
        Vector2 maxPos = -minPos;
        newPos.x = Mathf.Clamp(newPos.x, minPos.x, maxPos.x);
        newPos.y = Mathf.Clamp(newPos.y, minPos.y, maxPos.y);

        mapContent.localPosition = newPos;
    }

    // Opens Map Menu
    public void OpenMapMenu(CanvasGroup menu)
    {
        // Stores previous menu and disables it
        prevMenu = menu;
        prevMenu.interactable = false;

        // Updates map menu buttons based on player progression
        UpdateMapBtns();
        // Resets zoom slider
        zoomSlider.value = 1;

        // Centers on the choice the player is currently at
        CenterMapItem();

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
