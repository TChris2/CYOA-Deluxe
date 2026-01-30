using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Input controls for the player
public class IntroInputManager : MonoBehaviour
{
    public PlayerInput playerInput;
    public PlayerInput.MenuActions menu;
    Intro intro;

    void Awake()
    {
        playerInput = new PlayerInput();
        menu = playerInput.Menu;
        intro = GetComponent<Intro>();

        menu.Skip.performed += ctx => intro.Skip();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }
}
