using UnityEngine;

// Spawns GlobalObjects if it has not been detected in Main Game
public class SpawnGlobal : MonoBehaviour
{
    [SerializeField]
    private GameObject GlobalObjects;

    void Awake()
    {
        if (!SaveManager.instance)
        {
            // Debug.Log("SaveManager not found, spawning Global Objects");
            Instantiate(GlobalObjects);
        }
        else
        {
            // Debug.Log("SaveManager found, not spawning Global Objects");
        }
    }
}
