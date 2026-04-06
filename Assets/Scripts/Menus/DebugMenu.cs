using UnityEngine;
using UnityEngine.UI;
using System.IO;

// Functionality for the debug menu
public class DebugMenu : MonoBehaviour
{
    [SerializeField]
    private Button SaveBtn;
    [SerializeField]
    private Button LoadSOBtn;
    [SerializeField]
    private Button LoadJSONBtn;

    void Start()
    {
        // Gets comps
        SaveManager sm = FindAnyObjectByType<SaveManager>();
        GameManager gm = FindAnyObjectByType<GameManager>();

        // Adds functions to the buttons
        // Saves info to json
        SaveBtn.onClick.AddListener(() => sm.SaveData());
        // Resets back to default info
        LoadSOBtn.onClick.AddListener(() => { sm.LoadSOData(); gm.ResetLocalVars(); });
        // Loads save data from memory
        LoadJSONBtn.onClick.AddListener(() => AttemptJSONLoad(sm));
    }

    // Checks to see if filepaths exist before attempting to load json info
    void AttemptJSONLoad(SaveManager sm)
    {
        // Checks to see if the files already exist
        foreach (string path in sm.filePaths)
        {
            if (!File.Exists(path))
            {
                // Debug.Log("ERROR, Not all filepaths exist, aborting loading JSON data");
                return;
            }
        }
        sm.LoadJSONData();
    }
}
