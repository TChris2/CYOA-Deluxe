using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

/// <summary>
/// Manages save data
/// </summary>
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
    [HideInInspector]
    public AchievementManager am;

    void Awake()
    {
        // Deletes object if an instance already exists
        if (instance != null && instance != this)
        {
            // Disables scripts with Start functions to prevent them from causing issues
            GetComponentInChildren<MapMenu>().enabled = false;
            GetComponentInChildren<AchievementMenu>().enabled = false;
            Destroy(gameObject);
            return;
        }

        // Saves instance
        instance = this;
        DontDestroyOnLoad(gameObject);
        am = GetComponent<AchievementManager>();
        
        // Adds filepaths to json files
        filePaths.Add(Path.Combine(Application.persistentDataPath, "SaveData.json"));

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

    /// <summary>
    /// Loads scriptable object information
    /// </summary>
    public void LoadSOData()
    {
        // Debug.Log("SaveManager: Loading SO Data");
        // Clears dictionaries per each load
        choiceDict.Clear();
        achieveDict.Clear();
        letterDict.Clear();
        // Resets stats
        stats.Reset();

        ChoiceInfo[] choiceArr;
        AchievementInfo[] achieveArr;
        LetterInfoList letterList;

        choiceArr = Resources.LoadAll<ChoiceInfo>("Choices");
        AddChoiceDictInfo(choiceArr);
        achieveArr = Resources.LoadAll<AchievementInfo>("Achievements");
        AddAchieveDictInfo(achieveArr);
        letterList = Resources.Load<LetterInfoList>("Letters/LetterInfoList");
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
                Debug.LogWarning($"SaveManager: Duplicate ChoiceID detected, {choice.choiceID} in {choice.choice}");
                continue;
            }

            ChoiceInfo newChoice = ScriptableObject.CreateInstance<ChoiceInfo>();
            newChoice.AddInfo(choice);

            // Adds ChoiceInfo to the list
            choiceDict.Add(newChoice.choiceID, newChoice);
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
                Debug.LogWarning($"SaveManager: Duplicate AchieveID detected, {achievement.achieveID} in {achievement.achievement}");
                continue;
            }

            AchievementInfo newAchievement = ScriptableObject.CreateInstance<AchievementInfo>();
            newAchievement.AddInfo(achievement);

            // Adds AchievementInfo to the list
            achieveDict.Add(newAchievement.achieveID, newAchievement);
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
                Debug.LogWarning($"SaveManager: Duplicate LetterID detected, {letter.letterID} in {letter.letter}");
                continue;
            }

            LetterInfo newLetter = new LetterInfo(letter.letterID, letter.letter, letter.hasObtained);

            // Adds AchievementInfo to the list
            letterDict.Add(newLetter.letterID, newLetter);
        }
    }

    /// <summary>
    /// Loads choice info from JSON
    /// </summary>
    public void LoadJSONData()
    {
        // Debug.Log("SaveManager: Loading Choice JSON Data");
        // Clears dictionaries per each load
        choiceDict.Clear();
        achieveDict.Clear();
        letterDict.Clear();
        
        // Intially loads SO data to get static info
        LoadSOData();
        
        var json = File.ReadAllText(filePaths[0]);
        
        // Loads save data
        GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json, new JsonSerializerSettings
            { Converters = { new StringEnumConverter() } });
        
        // Updates ChoiceInfo with current player progress
        foreach (ChoiceSaveData entry in saveData.choices)
        {
            if (choiceDict.TryGetValue(entry.choiceID, out ChoiceInfo info))
            {
                info.hasComplete = entry.hasComplete;
            }
            else
            {
                // Debug.LogWarning($"SaveManager: ChoiceID {entry.choiceID} not found in ScriptableObjects!");
            }
        }

        // Updates AchievementInfo with current player progress
        foreach (AchievementSaveData entry in saveData.achievements)
        {
            if (achieveDict.TryGetValue(entry.achieveID, out AchievementInfo info))
            {
                info.achieveState = entry.achieveState;
                info.hasUnlocked = entry.hasUnlocked;
            }
            else
            {
                // Debug.LogWarning($"SaveManager: AchieveID {entry.achieveID} not found in ScriptableObjects!");
            }
        }

        // Updates LetterInfo with current player progress
        foreach (LetterInfo entry in saveData.letters)
        {
            if (letterDict.TryGetValue(entry.letterID, out LetterInfo info))
            {
                info.hasObtained = entry.hasObtained;
            }
            else
            {
                // Debug.LogWarning($"SaveManager: LetterID {kvp.Value.letterID} not found in ScriptableObjects!");
            }
        }

        // Loads stat save data
        stats = saveData.stats;
    }

    /// <summary>
    /// Saves info to JSON
    /// </summary>
    public void SaveData()
    {
        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, Converters = { new StringEnumConverter() } };
        
        GameSaveData saveData = new GameSaveData();

        // Choice Info
        foreach (var pair in choiceDict)
        saveData.choices.Add(new ChoiceSaveData(pair.Key, pair.Value.hasComplete));
        // Achievement Info
        foreach (var pair in achieveDict)
            saveData.achievements.Add(new AchievementSaveData(pair.Key, pair.Value.achieveState, pair.Value.hasUnlocked));
        // Letter Info
        foreach (var pair in letterDict)
            saveData.letters.Add(new LetterInfo(pair.Key, pair.Value.letter, pair.Value.hasObtained));
        // Stats
        saveData.stats = stats;

        // Saves all info to a single file
        var json = JsonConvert.SerializeObject(saveData, settings);
        File.WriteAllText(filePaths[0], json);
    }

    private void OnDisable()
    {
        if (instance == this)
            SaveData();
    }
}

/// <summary>
/// Stores all the data that needs to be saved
/// </summary>
public class GameSaveData
{
    public List<ChoiceSaveData> choices = new List<ChoiceSaveData>();
    public List<AchievementSaveData> achievements = new List<AchievementSaveData>();
    public List<LetterInfo> letters = new List<LetterInfo>();
    public Stats stats;
}
