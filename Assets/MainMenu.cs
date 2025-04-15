using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * MainMenu.cs
 * 
 * This script manages the main menu interface, handling all button clicks and
 * transitions between different menu screens (main, options, tutorial).
 * 
 * Attach this to a GameObject in your main menu scene.
 */

public class MainMenu : MonoBehaviour
{
    // References to different menu panels that can be shown/hidden
    public GameObject mainMenuObject;    // The main menu panel with play/options/quit buttons
    public GameObject optionsMenuObject; // The options panel with settings controls
    public GameObject tutorialObject;    // The tutorial/instructions panel

    /*
     * Called when the Play button is clicked.
     * Either shows the tutorial first or loads the game directly.
     */
    public void PlayGame()
    {
        // Check if we have a tutorial to show before starting the game
        if (tutorialObject != null)
        {
            // Hide main menu and show tutorial
            mainMenuObject.SetActive(false);
            tutorialObject.SetActive(true);
            
        }
        else
        {
            // No tutorial to show, load game directly
            LoadGameScene();
        }
    }

    /*
     * Loads the game scene directly.
     * Called either from PlayGame or from the tutorial's OK button.
     */
    public void LoadGameScene()
    {
        // Load the next scene in the build index (assumed to be the game level)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /*
     * Shows the options menu and hides the main menu.
     * Called when the Options button is clicked.
     */
    public void ShowOptions()
    {
        mainMenuObject.SetActive(false);
        optionsMenuObject.SetActive(true);
    }

    /*
     * Returns to the main menu from the options screen.
     * Called when the Back button is clicked in the options menu.
     */
    public void BackToMainMenu()
    {
        optionsMenuObject.SetActive(false);
        mainMenuObject.SetActive(true);
    }

    /*
     * Quits the application when the Quit button is clicked.
     * Note: This only works in a built game, not in the Unity Editor.
     */
    public void QuitGame()
    {
        Application.Quit();
    }
}
