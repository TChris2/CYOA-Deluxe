using UnityEngine;

public class SpawnGlobal : MonoBehaviour
{
    [SerializeField]
    private GameObject GlobalObjects;

    void Awake()
    {
        if (!FindAnyObjectByType<SaveManager>())
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
