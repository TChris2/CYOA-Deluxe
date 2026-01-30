using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Functionality for the pause menu
public class DebugMenuFunctions : MonoBehaviour
{
    [SerializeField]
    private Button SaveBtn;
    [SerializeField]
    private Button LoadSOBtn;
    [SerializeField]
    private Button LoadJSON;
    

    void Start()
    {
        // Gets comps
        SaveManager sm = FindAnyObjectByType<SaveManager>();

        // Adds functions to the buttons
        SaveBtn.onClick.AddListener(() => sm.SaveData());
        LoadSOBtn.onClick.AddListener(() => sm.LoadSOData());
        LoadJSON.onClick.AddListener(() => sm.LoadJSONData());
    }
}
