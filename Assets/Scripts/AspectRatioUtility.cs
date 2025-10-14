using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Updates aspect ratio when changing window size - Script from Max O'Didily
public class AspectRatioUtility : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        // Gets camera
        cam = GetComponent<Camera>();
        // Sets aspect ratio at start
        Adjust();
    }

    void Update()
    {
        // Continually updates to match aspect ratio
        Adjust();
    }

    // Updates to display black bars depending on the user's aspect ratio    
    public void Adjust()
    {
        // Target aspect ratio
        float targetaspect = 16f / 9f;
        // Gets current screen size
        float windowaspect = (float)Screen.width / (float)Screen.height;
        // Gets the scale height
        float scaleheight = windowaspect / targetaspect;

        // If screen is taller than target aspect ratio
        if (scaleheight < 1f)
        {
            Rect rect = cam.rect;

            rect.width = 1f;
            rect.height = scaleheight;
            rect.x = 0f;
            rect.y = (1f - scaleheight) / 2f;

            cam.rect = rect;
        }
        // If screen is wider than target aspect ratio
        else
        {
            float scalewidth = 1f / scaleheight;

            Rect rect = cam.rect;

            rect.width = scalewidth;
            rect.height = 1f;
            rect.x = (1f - scalewidth) / 2f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}
