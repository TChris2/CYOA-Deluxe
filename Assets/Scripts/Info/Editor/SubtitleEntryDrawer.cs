using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubtitleEntry))]
public class SubtitleEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var textProp = property.FindPropertyRelative("text");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        float width = EditorGUIUtility.currentViewWidth - 40;

        // Guard against mixed values when multiple SOs selected
        string displayText = textProp.hasMultipleDifferentValues ? "" : textProp.stringValue;

        float textHeight = EditorStyles.textArea.CalcHeight(
            new GUIContent(displayText),
            width
        );

        return (lineHeight + spacing) * 2 + textHeight + 5;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var startProp = property.FindPropertyRelative("displayStartTime");
        var endProp = property.FindPropertyRelative("displayEndTime");
        var textProp = property.FindPropertyRelative("text");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect rect = position;
        rect.height = lineHeight;

        // These already handle multi-select correctly via PropertyField
        EditorGUI.PropertyField(rect, startProp);
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, endProp);
        rect.y += lineHeight + spacing;

        // Guard against mixed values when multiple SOs are selected
        string displayText = textProp.hasMultipleDifferentValues ? "" : textProp.stringValue;

        float textHeight = EditorStyles.textArea.CalcHeight(
            new GUIContent(displayText),
            rect.width
        );

        rect.height = textHeight;

        // Only write back when the user actually edits, preventing the
        // first SO's value from being broadcast to all selected SOs on repaint
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUI.TextArea(rect, displayText, EditorStyles.textArea);
        if (EditorGUI.EndChangeCheck())
        {
            textProp.stringValue = newText;
        }

        EditorGUI.EndProperty();
    }
}