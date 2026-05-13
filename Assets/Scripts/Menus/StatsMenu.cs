using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Functionality for the small Stats menu
/// </summary>
public class StatsMenu : MonoBehaviour
{
    private CanvasGroup statsMenu;
    [SerializeField]
    private List<TMP_Text> statsText;
    // Scripts
    InputMenu iMenu;
    SaveManager sm;
    
    void Start()
    {
        statsMenu = GetComponent<CanvasGroup>();
        StartCoroutine(GetComponents());
    }

    IEnumerator GetComponents()
    {
        yield return null;

        sm = FindAnyObjectByType<SaveManager>();
        iMenu = FindAnyObjectByType<InputMenu>();
    }
    
    /// <summary>
    /// Opens Stats Menu
    /// </summary>
    public void OpenStatsMenu()
    {
        if (!statsMenu.interactable)
            sm.stats.DisplayStatsShort(statsText, sm);

        iMenu.SmallMenuOpenClose(statsMenu, !statsMenu.interactable);
    }
}
