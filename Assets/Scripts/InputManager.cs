using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Input controls for the player
public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;
    public PlayerInput.MenuActions menu;
    InputMenu iMenu;
    GameManager gm;

    void Awake()
    {
        playerInput = new PlayerInput();
        menu = playerInput.Menu;
        iMenu = GetComponent<InputMenu>();
        gm = GetComponent<GameManager>();

        // Assigns functions to each action
        menu.PauseMenu.performed += ctx => iMenu.PauseMenu();
        menu.DebugMenu.performed += ctx => iMenu.DebugMenu();
        menu.Skip.performed += ctx => gm.Skip();
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
