using System.Collections;
using UnityEngine;

// Removes choice if the player has already completed the choice
public class RemoveChoice : MonoBehaviour
{
    public void DeleteChoice()
    {
        // Debug.Log($"Deleting choice {gameObject.name}");
        Destroy(gameObject);
    }
}
