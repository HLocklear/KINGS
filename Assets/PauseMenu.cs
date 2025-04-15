using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * PauseMenu.cs
 * 
 * This script manages the pause menu functionality, allowing the player to:
 * - Pause/resume the game
 * - Return to the main menu
 * - Quit the application
 * 
 * It uses Time.timeScale to pause gameplay and provides a static GameIsPaused
 * flag that other scripts can check to prevent interaction during pause.
 */

public class PauseMenu : MonoBehaviour
{
    // Static flag that other scripts can check to see if the game is paused
    public static bool GameIsPaused = false;

    // The pause menu UI panel containing all pause-related buttons
    public GameObject pauseMenuUI;

    // References to specific buttons in the pause menu
    public Button resumeButton;    // Button to resume the game
    public Button menuButton;      // Button to go back to main menu
    public Button quitButton;      // Button to exit the game

    /*
     * Set up button click handlers when the game starts
     */
    void Start()
    {
        // Find buttons if not assigned in inspector
        if (resumeButton == null)
            resumeButton = pauseMenuUI.transform.Find("ResumeButton").GetComponent<Button>();
        if (menuButton == null)
            menuButton = pauseMenuUI.transform.Find("MenuButton").GetComponent<Button>();
        if (quitButton == null)
            quitButton = pauseMenuUI.transform.Find("QuitButton").GetComponent<Button>();

        // Add listeners programmatically
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => Resume());
        if (menuButton != null)
            menuButton.onClick.AddListener(() => LoadMenu());
        if (quitButton != null)
            quitButton.onClick.AddListener(() => QuitGame());

    }

    /*
     * Listen for the Escape key to toggle pause state
     */
    void Update()
    {
        // Check for Escape key press to toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /*
     * Resumes the game by hiding the pause menu and setting time scale back to normal
     */
    public void Resume()
    {
        pauseMenuUI.SetActive(false);   // Hide the pause menu
        Time.timeScale = 1f;            // Set time back to normal speed
        GameIsPaused = false;           // Update the pause flag
    }

    /*
     * Pauses the game by showing the pause menu and setting time scale to zero
     */
    public void Pause()
    {
        pauseMenuUI.SetActive(true);    // Show the pause menu
        Time.timeScale = 0f;            // Freeze time to pause gameplay
        GameIsPaused = true;            // Update the pause flag
    }

    /*
     * Returns to the main menu scene
     */
    public void LoadMenu()
    {
        // Load the scene named "Menu"
        SceneManager.LoadScene("Menu");

        // Make sure time scale is reset when loading the menu
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    /*
     * Exits the application completely
     * (Note: This doesn't work in the Unity Editor, only in builds)
     */
    public void QuitGame()
    {
        Application.Quit();
    }
}
