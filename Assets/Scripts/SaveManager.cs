using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    // Store file paths of files being saved across multiple sessions
    [HideInInspector]
    public List<string> filePaths = new List<string>();
    // ChoiceInfo dictionary
    public Dictionary<string, ChoiceInfo> choiceDict = new Dictionary<string, ChoiceInfo>();
    // AchievementInfo dictionary
    public Dictionary<string, AchievementInfo> achieveDict = new Dictionary<string, AchievementInfo>();
    // LetterInfo dictionary
    public Dictionary<LetterID, LetterInfo> letterDict = new Dictionary<LetterID, LetterInfo>();
    // Current player stats
    public Stats stats;

    void Awake()
    {
        // Deletes object if an instance already exists
        if (instance != null && instance != this)
        {
            // Disables scripts with Start functions to prevent them from causing issues
            GetComponentInChildren<MapMenuFunctions>().enabled = false;
            GetComponentInChildren<AchieveMenuFunctions>().enabled = false;
            Destroy(gameObject);
            return;
        }

        // Saves instance
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Adds filepaths to json files
        filePaths.Add(Path.Combine(Application.persistentDataPath, "Choices.json"));
        filePaths.Add(Path.Combine(Application.persistentDataPath, "Achievements.json"));
        filePaths.Add(Path.Combine(Application.persistentDataPath, "Letters.json"));
        filePaths.Add(Path.Combine(Application.persistentDataPath, "Stats.json"));


        // Checks to make sure all filepaths exist
        bool filePathExist = true;

        // Checks to see if the files already exist
        foreach (string path in filePaths)
        {
            if (!File.Exists(path))
            {
                filePathExist = false;
                break;
            }
        }

        // Loads data from scriptable objects if the JSON files do not exist
        if (!filePathExist)
            LoadSOData();
        // Loads data from JSON files if it already exists
        else
            LoadJSONData();

    }

    // Loads scriptable object information
    public void LoadSOData()
    {
        // Debug.Log("Loading SO Data");
        // Clears dictionaries per each load
        choiceDict.Clear();
        achieveDict.Clear();
        letterDict.Clear();
        // Resets stats
        stats.Reset();

        ChoiceInfo[] choiceArr;
        AchievementInfo[] achieveArr;
        LetterInfoList letterList;

        choiceArr = Resources.LoadAll<ChoiceInfo>("Scriptable Objects/Choices");
        AddChoiceDictInfo(choiceArr);
        achieveArr = Resources.LoadAll<AchievementInfo>("Scriptable Objects/Achievements");
        AddAchieveDictInfo(achieveArr);
        letterList = Resources.Load<LetterInfoList>("Scriptable Objects/Letters/LetterInfoList");
        AddLetterDictInfo(letterList);
    }

    // Adds info obtained from choice info arrays into the dictionary
    void AddChoiceDictInfo(ChoiceInfo[] choiceArr)
    {
        foreach (ChoiceInfo choice in choiceArr)
        {
            // Checks for duplicate ids
            if (choiceDict.ContainsKey(choice.choiceID))
            {
                Debug.LogWarning($"Duplicate ChoiceID detected, {choice.choiceID} in {choice.choice}");
                continue;
            }

            ChoiceInfo newChoice = ScriptableObject.CreateInstance<ChoiceInfo>();
            
            // Trims strings to remove empty space
            newChoice.choiceID = choice.choiceID.Trim();
            newChoice.choice = choice.choice.Trim();
            newChoice.vid = choice.vid;
            newChoice.choiceState = choice.choiceState;
            newChoice.vidEndTime = choice.vidEndTime;
            newChoice.hasComplete = choice.hasComplete;
            newChoice.objs = choice.objs;
            newChoice.weaponsUsed = choice.weaponsUsed;
            foreach (string id in newChoice.weaponsUsed)
                id.Trim();
            newChoice.mapName = choice.mapName.Trim();
            newChoice.thumbnail = choice.thumbnail;
            newChoice.nextChoiceIDs = choice.nextChoiceIDs;
            foreach (string id in newChoice.nextChoiceIDs)
                id.Trim();
            newChoice.achieveIDs = choice.achieveIDs;
            foreach (string id in newChoice.achieveIDs)
                id.Trim();
            newChoice.letterIDs = choice.letterIDs;

            // Adds ChoiceInfo to the list
            choiceDict.Add(choice.choiceID, newChoice);
        }
    }

    // Adds info obtained from achievement info arrays into the dictionary
    void AddAchieveDictInfo(AchievementInfo[] achieveArr)
    {
        foreach (AchievementInfo achievement in achieveArr)
        {
            // Checks for duplicate ids
            if (achieveDict.ContainsKey(achievement.achieveID))
            {
                Debug.LogWarning($"Duplicate AchieveID detected, {achievement.achieveID} in {achievement.achievement}");
                continue;
            }

            AchievementInfo newAchievement = ScriptableObject.CreateInstance<AchievementInfo>();
            // Trims strings to remove empty space
            newAchievement.achieveID = achievement.achieveID.Trim();
            newAchievement.achievement = achievement.achievement.Trim();
            newAchievement.achieveState = achievement.achieveState;
            newAchievement.hasUnlocked = achievement.hasUnlocked;
            newAchievement.description = achievement.description.Trim();
            foreach (string id in achievement.choiceIDs)
                id.Trim();
            foreach (string id in achievement.choiceIDs)
                id.Trim();
            newAchievement.icon = achievement.icon;
            newAchievement.updateDisplay = achievement.updateDisplay;

            // Adds AchievementInfo to the list
            achieveDict.Add(achievement.achieveID, newAchievement);
        }
    }

    // Adds info obtained from letter info into the dictionary
    void AddLetterDictInfo(LetterInfoList letterList)
    {
        foreach (LetterInfo letter in letterList.letters)
        {
            // Checks for duplicate ids
            if (letterDict.ContainsKey(letter.letterID))
            {
                Debug.LogWarning($"Duplicate LetterID detected, {letter.letterID} in {letter.letter}");
                continue;
            }

            LetterInfo newLetter = new LetterInfo(letter.letterID, letter.letter, letter.hasObtained);
            // Trims strings to remove empty space
            newLetter.letterID = letter.letterID;
            newLetter.letter = letter.letter.Trim();
            newLetter.hasObtained = letter.hasObtained;

            // Adds AchievementInfo to the list
            letterDict.Add(newLetter.letterID, newLetter);
        }
    }

    // Loads choice info from JSON
    public void LoadJSONData()
    {
        // Debug.Log("Loading Choice JSON Data");
        // Clears dictionaries per each load
        choiceDict.Clear();
        achieveDict.Clear();
        letterDict.Clear();
        
        // Intially loads SO data to get static info
        LoadSOData();
        
        var json = File.ReadAllText(filePaths[0]);
        
        // Loads choice save data
        List<ChoiceSaveData> loadedChoiceInfo = JsonConvert.DeserializeObject<List<ChoiceSaveData>>(json, new JsonSerializerSettings
            { Converters = { new StringEnumConverter() } });

        // Updates ChoiceInfo with current player progress
        foreach (var entry in loadedChoiceInfo)
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

        // Debug.LogWarning("Loading Achievement JSON Data");
        json = File.ReadAllText(filePaths[1]);

        // Loads achievement save data
        List<AchievementSaveData> loadedAchieveInfo = JsonConvert.DeserializeObject<List<AchievementSaveData>>(json,
            new JsonSerializerSettings { Converters = { new StringEnumConverter() } });

        // Updates AchievementInfo with current player progress
        foreach (var entry in loadedAchieveInfo)
        {
            if (achieveDict.TryGetValue(entry.achieveID, out AchievementInfo info))
            {
                info.achieveState = entry.achieveState;
                info.hasUnlocked = entry.hasUnlocked;
            }
            else
            {
                // Debug.LogWarning($"AchieveID {entry.achieveID} not found in ScriptableObjects!");
            }
        }

        // Debug.LogWarning("Loading Letter JSON Data");
        json = File.ReadAllText(filePaths[2]);

        // Loads letter save data
        Dictionary<string, LetterInfo> loadedLetterInfo = JsonConvert.DeserializeObject<Dictionary<string, LetterInfo>>(json,
            new JsonSerializerSettings { Converters = { new StringEnumConverter() } });

        // Updates LetterInfo with current player progress
        foreach (var kvp in loadedLetterInfo)
        {
            if (letterDict.TryGetValue(kvp.Value.letterID, out LetterInfo info))
            {
                info.hasObtained = kvp.Value.hasObtained;
            }
            else
            {
                // Debug.LogWarning($"LetterID {kvp.Value.letterID} not found in ScriptableObjects!");
            }
        }

        // Loads stat save data
        json = File.ReadAllText(filePaths[3]);
        stats = JsonConvert.DeserializeObject<Stats>(json,
            new JsonSerializerSettings { Converters = { new StringEnumConverter() } });
    }

    // Saves info to JSON
    public void SaveData()
    {
        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, Converters = { new StringEnumConverter() } };
        
        // Saves ChoiceInfo
        List<ChoiceSaveData> saveChoiceList = new List<ChoiceSaveData>();

        // Creates list of a simplified version of the ChoiceInfo class
        foreach (var pair in choiceDict)
            saveChoiceList.Add(new ChoiceSaveData(pair.Key, pair.Value.choice, pair.Value.mapName, pair.Value.hasComplete));

        // Debug.Log("Saving Choice Data");
        var json = JsonConvert.SerializeObject(saveChoiceList, settings);
        File.WriteAllText(filePaths[0], json);

        // Saves AchievementInfo
        List<AchievementSaveData> saveAchieveList = new List<AchievementSaveData>();

        // Creates list of a simplified version of the AchievementInfo class
        foreach (var pair in achieveDict)
            saveAchieveList.Add(new AchievementSaveData(pair.Key, pair.Value.achievement, pair.Value.achieveState, pair.Value.hasUnlocked));

        // Debug.Log("Saving Achievement Data");
        json = JsonConvert.SerializeObject(saveAchieveList, settings);
        File.WriteAllText(filePaths[1], json);
        
        // Saves LetterInfo
        // Debug.Log("Saving Letter Data");
        json = JsonConvert.SerializeObject(letterDict, settings);
        File.WriteAllText(filePaths[2], json);

        // Saves Stats
        // Debug.Log("Saving Stats Data");
        json = JsonConvert.SerializeObject(stats, settings);
        File.WriteAllText(filePaths[3], json);
    }

    private void OnDisable()
    {
        if (instance == this)
            SaveData();
    }
}
