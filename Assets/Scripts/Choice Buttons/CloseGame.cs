using System.Collections;
using UnityEngine;

// Closes game, used in Doctor_1_2_1 - The Spire
public class CloseGame : MonoBehaviour
{   
    [SerializeField]
    private float closeGameTime;
    GameManager gm;

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
        StartCoroutine(CloseGameDelay());
    }

    // Closes the game after a specific amt of time passes
    private IEnumerator CloseGameDelay()
    {
        while (gm.videoPlayer.time < closeGameTime)
            yield return null;

        yield return null;
        Debug.Log("CloseGame: Closing game");

        Application.Quit();
    }
}
