using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Messes with the time during testing
public class TimeControl : MonoBehaviour
{
    public float time;
    // Update is called once per frame
    void Update()
    {
        Time.timeScale = time;
    }
}
