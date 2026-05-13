using UnityEngine;

/// <summary>
/// Handles the exterior void cover to block additional elements not normally blocked the rect masks
/// </summary>
public class VoidCover : MonoBehaviour
{
    CanvasGroup voidCover;

    void Awake()
    {
        voidCover = GetComponent<CanvasGroup>();
    }
    
    /// <summary>
    /// Enables or disable exterior void cover
    /// </summary>
    public void EnableVoidCover(bool isEnable)
    {
        voidCover.alpha = isEnable ? 1 : 0;
    }
}
