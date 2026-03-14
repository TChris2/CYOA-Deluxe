using UnityEngine;

// Gets GameManager script and uses its SkipVidTime function to skip forward in the vid
public class SkipVidTime : MonoBehaviour
{
    GameManager gm;

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
    }

    public void SkipTime(float time)
    {
        gm.SkipVidTime(time);
    }
}
