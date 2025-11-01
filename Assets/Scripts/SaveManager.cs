using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

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
        // Clears dictionary per each load
        choiceDict.Clear();

        // Debug.LogWarning("Loading Scriptable Object Data");
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
            
            ChoiceInfo newChoice = new ChoiceInfo(choice.choiceID, choice.choice, choice.vid, choice.thumbnail, choice.choiceState, choice.vidEndTime, choice.hasComplete,
                choice.nextChoiceIDs, choice.objs);
            choiceDict.Add(choice.choiceID, newChoice);
        }
    }

    // Loads choice info from JSON
    public void LoadJSONData()
    {
        // Clears dictionary per each load
        choiceDict.Clear();
        
        LoadSOData();
        
        // Debug.LogWarning("Loading JSON Data");
        var json = File.ReadAllText(filePath);
        
        // Loads choice save data
        List<ChoiceSaveData> loadedInfo = JsonConvert.DeserializeObject<List<ChoiceSaveData>>(json, new JsonSerializerSettings
            { Converters = { new StringEnumConverter() } });
        
        // Updates ChoiceInfo with current player progress
        foreach (var entry in loadedInfo)
        {
            if (choiceDict.TryGetValue(entry.choiceID, out ChoiceInfo info))
            {
                info.hasComplete = entry.hasComplete;
            }
            else
            {
                // Debug.LogWarning($"ChoiceID {entry.choiceID} not found in ScriptableObjects!");
            }
        }
    }

    // Save any object to JSON
    public void SaveData()
    {
        List<ChoiceSaveData> saveList = new List<ChoiceSaveData>();

        // Creates list of a simplified version of the ChoiceInfo class
        foreach (var pair in choiceDict)
            saveList.Add(new ChoiceSaveData(pair.Key, pair.Value.choice, pair.Value.hasComplete));

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() }
        };

        // Debug.Log("Saving Data");
        var json = JsonConvert.SerializeObject(saveList, settings);

        // Debug.Log(json);

        File.WriteAllText(filePath, json);
    }

    private void OnDisable()
    {
        SaveData();
    }
}
