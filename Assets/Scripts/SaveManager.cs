using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Video;

public class SaveManager : MonoBehaviour
{
    private string filePath;
    public Dictionary<ChoiceID, ChoiceInfo> choiceDict = new Dictionary<ChoiceID, ChoiceInfo>();

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "ChoiceState.json");

        // Loads data from scriptable objects if the JSON file does not exist
        if (!File.Exists(filePath))
            LoadSOData();
        // Loads data from JSON file if it already exists
        else
            LoadJSONData();

    }

    // Loads choice info from scriptable object
    public void LoadSOData()
    {
        choiceDict.Clear();

        Debug.LogWarning("Loading Scriptable Object Data");
        ChoiceInfoSO[] choiceArr;

        choiceArr = Resources.LoadAll<ChoiceInfoSO>("Scriptable Objects/Choices");
        AddChoiceDictInfo(choiceArr);
    }

    // Adds info obtained from choice info arrays into the dictionary
    void AddChoiceDictInfo(ChoiceInfoSO[] choiceArr)
    {
        foreach (ChoiceInfoSO choice in choiceArr)
        {
            if (choiceDict.ContainsKey(choice.choiceID))
            {
                Debug.LogWarning($"Duplicate ChoiceID detected, {choice.choiceID} in {choice.choice}");
                continue;
            }
            
            ChoiceInfo newChoice = new ChoiceInfo(choice.choiceID, choice.choice, choice.ExportVid(), choice.choiceState, choice.vidEndTime, choice.hasComplete,
                choice.nextChoiceIDs, choice.objs);
            choiceDict.Add(choice.choiceID, newChoice);
        }
    }

    // Loads choice info from JSON
    public void LoadJSONData()
    {
        choiceDict.Clear();
        
        Debug.LogWarning("Loading JSON Data");
        var json = File.ReadAllText(filePath);

        choiceDict = JsonConvert.DeserializeObject<Dictionary<ChoiceID, ChoiceInfo>>(json, new JsonSerializerSettings
            { Converters = { new StringEnumConverter() }});
    }

    // Save any object to JSON
    public void SaveData()
    {
        // Debug.Log("Saving Data");
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() }
        };

        var json = JsonConvert.SerializeObject(choiceDict, settings);

        // Debug.Log(json);

        File.WriteAllText(filePath, json);
    }

    private void OnDisable()
    {
        SaveData();
    }
}
