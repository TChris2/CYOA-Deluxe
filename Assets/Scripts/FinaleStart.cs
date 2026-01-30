using UnityEngine;
using System.Collections;

// Script attached to Letter Circle Object as a means of reaching the GameManager script as a animation event
public class FinaleStart : MonoBehaviour
{
    public GameManager gm;
    void Reset()
    {
        gm = FindAnyObjectByType<GameManager>();
    }

    public IEnumerator StartFinale()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(gm.StartFinale());
    }
}
