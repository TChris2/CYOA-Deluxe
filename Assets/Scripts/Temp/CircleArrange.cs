using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CircleArrange : MonoBehaviour
{
    public float radius = 200f;
    public float startAngle = 0f;  // degrees

    #if UNITY_EDITOR
    [ContextMenu("Arrange Children in Circle")]
    public void ArrangeChildren()
    {
        int count = transform.childCount;
        if (count == 0) return;

        Undo.RecordObjects(GetComponentsInChildren<Transform>(), "Arrange in Circle");

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (360f / count) * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Sin(rad) * radius,
                Mathf.Cos(rad) * radius,
                0f
            );

            transform.GetChild(i).localPosition = pos;
        }
    }
    #endif
}