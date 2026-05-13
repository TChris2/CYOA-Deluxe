using UnityEngine;
using System.Collections;

// Script attached to Letter Circle Object as a means of reaching the GameManager script as a animation event
public class FinaleStart : MonoBehaviour
{
    public FinaleManager fm;
    void Reset()
    {
        fm = FindAnyObjectByType<FinaleManager>();
    }

    public IEnumerator StartFinale()
    {
        yield return new WaitForSeconds(2.5f);
        fm.StartFinale();
    }
}
