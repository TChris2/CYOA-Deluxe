using UnityEngine;

public class OpenExplanationMenu : MonoBehaviour
{
    public CanvasGroup menu;

    public void OpenMenu()
    {
        SaveManager sm = FindAnyObjectByType<SaveManager>();
        (int completed, int a)= sm.stats.ChoicesCompleted(sm.choiceDict);
        if (completed < 1)
        {
            menu.alpha = 1;
            menu.interactable = true;
            menu.blocksRaycasts = true;
            menu.GetComponent<Animator>().Play($"Open Menu");
        }
    }
}
