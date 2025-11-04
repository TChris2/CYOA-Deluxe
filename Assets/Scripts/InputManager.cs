using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Gets inputs from the player
public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;
    public PlayerInput.MenuActions menu;
    InputMenu iMenu;
    ButtonManager bm;

    void Awake()
    {
        playerInput = new PlayerInput();
        menu = playerInput.Menu;
        iMenu = GetComponent<InputMenu>();
        bm = GetComponent<ButtonManager>();

        // Assigns functions to each action
        menu.PauseMenu.performed += ctx => iMenu.PauseMenu();
        menu.DebugMenu.performed += ctx => iMenu.DebugMenu();
        menu.Skip.performed += ctx => bm.Skip();
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
