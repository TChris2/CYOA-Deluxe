using ScrutableObjects.UnityEditor;
using UnityEditor;
using UnityEngine;

// Enables direct access to the properties of the ScriptableObject references in the editor
[CustomPropertyDrawer(typeof(AchievementInfo), true)]
public class AchievementInfoScrutEditor : ScrutableObjectDrawer
{

}
