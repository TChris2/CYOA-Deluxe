using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    
    // Open stats menu
    public void OpenStatsMenu()
    {
        if (!statsMenu.interactable)
            sm.stats.DisplayStatsShort(statsText, sm);

        iMenu.SmallMenuOpenClose(statsMenu, !statsMenu.interactable);
    }
}
