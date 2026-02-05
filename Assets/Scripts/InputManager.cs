using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Input controls for the player
public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public PlayerInput playerInput;
    public PlayerInput.GameActions gameActions;
    public PlayerInput.TitleScreenActions titleScreenActions;
    // Scripts
    InputMenu iMenu;
    // Title Screen Scripts
    IntroManager intro;

    void Awake()
    {
        // Stops script if not the current instance to prevent multiple listeners from being unintentionally added
        if (instance != null && instance != this)
        {
            return;
        }

        instance = this;

        // Checks which scene has been currently loaded
        SceneManager.sceneLoaded += SceneCheck;

        playerInput = new PlayerInput();
        gameActions = playerInput.Game;
        titleScreenActions = playerInput.TitleScreen;

        // Get components located in Global Objects
        iMenu = GetComponent<InputMenu>();

        // Title Screen actions
        titleScreenActions.CloseMenu.performed += ctx => iMenu.CloseMenu();
        titleScreenActions.Debug.performed += ctx => { iMenu.completeOverride = !iMenu.completeOverride; iMenu.DebugMenu(); };
        titleScreenActions.Skip.performed += IntroSkip;


        // Main Game actions
        gameActions.PauseMenu.performed += ctx => iMenu.PauseMenu();
        gameActions.DebugMenu.performed += ctx => { iMenu.completeOverride = !iMenu.completeOverride; iMenu.DebugMenu(); };
        gameActions.Skip.performed += Skip;
    }

    // Skip function for the intro
    void IntroSkip(InputAction.CallbackContext ctx)
    {
        intro.Skip();
    }

    // Skip function for the main game
    void Skip(InputAction.CallbackContext ctx)
    {
        iMenu.gm.Skip();
    }

    // Checks what scene the player is currently in when a scene is loaded
    void SceneCheck(Scene scene, LoadSceneMode mode)
    {
        // Clears menu list per scene change
        iMenu.openMenus.Clear();

        // Title Screen
        if (scene.name == "Title Screen")
        {
            // Debug.Log("Enabling Title Screen Action Map");

            // Disables previous action map
            gameActions.Disable();

            // Gets local scripts
            intro = FindAnyObjectByType<IntroManager>();
            iMenu.openMenus.Add(GameObject.Find("Title Screen Menu").GetComponent<CanvasGroup>());
            iMenu.dMenu = GameObject.Find("Debug Menu").GetComponent<CanvasGroup>();

            // Enables or disables debug based on whether debug was already enabled
            iMenu.DebugMenu();

            // Enables current action map for the scene
            titleScreenActions.Enable();
        }
        // Main Game
        else
        {
            // Debug.Log("Enabling Main Game Action Map");

            // Disables previous action map
            titleScreenActions.Disable();

            // Gets local scripts
            iMenu.gm = FindAnyObjectByType<GameManager>();
            iMenu.pMenuF = FindAnyObjectByType<PauseMenuFunctions>();
            iMenu.dMenu = GameObject.Find("Debug Menu").GetComponent<CanvasGroup>();
            iMenu.pMenu = GameObject.Find("Pause Menu").GetComponent<CanvasGroup>();
            iMenu.pMenuAni = iMenu.pMenu.GetComponent<Animator>();

            // Enables or disables debug based on whether debug was already enabled
            iMenu.DebugMenu();

            // Enables current action map for the scene
            gameActions.Enable();
        }
    }

    private void OnEnable()
    {
        if (instance == this)
            playerInput.Enable();
    }

    private void OnDisable()
    {
        if (instance == this)
            playerInput.Disable();
    }
}
