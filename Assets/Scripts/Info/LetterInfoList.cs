using System.Collections.Generic;
using UnityEngine;

// Stores letter info
[CreateAssetMenu(fileName = "LetterInfoList", menuName = "Scriptable Objects/LetterInfoList")]
public class LetterInfoList : ScriptableObject
{
    [Tooltip("List of letter information")]
    public List<LetterInfo> letters;

    // Trims strings of empty space    
    void OnValidate()
    {
        foreach (LetterInfo letter in letters)
            letter.letter.Trim();
    }
}

// Store letter information on whether the player collected the letter or not
[System.Serializable]
public class LetterInfo
{
    [Tooltip("ID for the letter")]
    public LetterID letterID;
    [Tooltip("Letter")]
    public string letter;
    [Tooltip("Whether the player has obtained the letter or not")]
    public bool hasObtained;

    // Adds info to new instance of LetterInfo
    public LetterInfo(LetterID letterID, string letter, bool hasObtained)
    {
        this.letterID = letterID;
        this.letter = letter;
        this.hasObtained = hasObtained;
    }
}

// Letter Ids
public enum LetterID
{
    l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11 
}