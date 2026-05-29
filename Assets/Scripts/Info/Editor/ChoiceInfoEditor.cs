using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChoiceInfo))]
public class ChoiceInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChoiceInfo choice = target as ChoiceInfo;
        serializedObject.Update();

        // Choice Info
        EditorGUILayout.PropertyField(serializedObject.FindProperty("choiceID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("choice"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hasComplete"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("firstRunOnly"));
        SerializedProperty choiceState = serializedObject.FindProperty("choiceState");
        EditorGUILayout.PropertyField(choiceState);
    
        // Vid Info
        if (!choice.choiceState.Contains(ChoiceState.Reference))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitles"));
            
            if (choice.choiceState.Contains(ChoiceState.Ending))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vidEndTime"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("objs"));
        }
        
        // Map Menu
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isOnMap"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbnail"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapDisplayChoice"));

        // Stat Tracking
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nextChoices"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("achievements"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("achievementHints"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("letterIDs"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponsUsed"));
        
        serializedObject.ApplyModifiedProperties();
    }
}