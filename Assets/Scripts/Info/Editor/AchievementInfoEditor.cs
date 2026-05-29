using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AchievementInfo))]
public class AchievementInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AchievementInfo achievement = target as AchievementInfo;
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "popupTime", "customPopupTime");

        if (achievement.isDisplayed)
        {
            SerializedProperty popupTime = serializedObject.FindProperty("popupTime");
            EditorGUILayout.PropertyField(popupTime);

            if (achievement.popupTime == PopupTime.Custom)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customPopupTime"));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}