using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string choiceFilePath;
    public Dictionary<string, ChoiceInfo> choiceDict = new Dictionary<string, ChoiceInfo>();

    void Awake()
    {
        choiceFilePath = Path.Combine(Application.persistentDataPath, "ChoiceState.json");

        // Loads data from scriptable objects if the JSON file does not exist
        if (!File.Exists(choiceFilePath))
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
        ChoiceInfo[] choiceArr;

        choiceArr = Resources.LoadAll<ChoiceInfo>("Scriptable Objects/Choices");
        AddChoiceDictInfo(choiceArr);
    }

    // Adds info obtained from choice info arrays into the dictionary
    void AddChoiceDictInfo(ChoiceInfo[] choiceArr)
    {
        foreach (ChoiceInfo choice in choiceArr)
        {
            if (choiceDict.ContainsKey(choice.choiceID))
            {
                Debug.LogWarning($"Duplicate ChoiceID detected, {choice.choiceID} in {choice.choice}");
                continue;
            }

            ChoiceInfo newChoice = ScriptableObject.CreateInstance<ChoiceInfo>();

            newChoice.choiceID = choice.choiceID.Trim();
            newChoice.choice = choice.choice.Trim();
            newChoice.vid = choice.vid;
            newChoice.choiceState = choice.choiceState;
            newChoice.vidEndTime = choice.vidEndTime;
            newChoice.hasComplete = choice.hasComplete;
            newChoice.objs = choice.objs;
            newChoice.mapName = choice.mapName.Trim();
            newChoice.thumbnail = choice.thumbnail;
            newChoice.nextChoiceIDs = choice.nextChoiceIDs;
            foreach (string id in newChoice.nextChoiceIDs)
                id.Trim();
            newChoice.achieveIDs = choice.achieveIDs;
            foreach (string id in newChoice.achieveIDs)
                id.Trim();

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
        var json = File.ReadAllText(choiceFilePath);
        
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
            saveList.Add(new ChoiceSaveData(pair.Key, pair.Value.choice, pair.Value.mapName, pair.Value.hasComplete));

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() }
        };

        // Debug.Log("Saving Data");
        var json = JsonConvert.SerializeObject(saveList, settings);

        // Debug.Log(json);

        File.WriteAllText(choiceFilePath, json);
    }

    private void OnDisable()
    {
        SaveData();
    }
}
