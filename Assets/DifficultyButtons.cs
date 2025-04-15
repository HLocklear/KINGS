using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * DifficultyButtons.cs
 * 
 * This script handles the UI buttons for selecting game difficulty settings.
 * It connects to the DifficultyManager singleton to apply the player's choice.
 * 
 * Attach this to a GameObject containing the difficulty selection buttons.
 */

public class DifficultyButtons : MonoBehaviour
{
    // References to the three difficulty selection buttons
    public Button easyButton;      // Reference to the Easy difficulty button
    public Button mediumButton;    // Reference to the Medium difficulty button
    public Button hardButton;      // Reference to the Hard difficulty button

    /*
     * Set up button click handlers and ensure the DifficultyManager exists
     */
    void Start()
    {
        // Make sure DifficultyManager exists in the scene
        // If not, create it so our buttons have something to communicate with
        if (FindObjectOfType<DifficultyManager>() == null)
        {
            // Create a new GameObject to hold the manager
            GameObject diffManagerObj = new GameObject("DifficultyManager");
            // Add the DifficultyManager component to it
            diffManagerObj.AddComponent<DifficultyManager>();
        }

        // Connect button click events to their respective handlers
        if (easyButton != null)
            easyButton.onClick.AddListener(SetEasy);
        if (mediumButton != null)
            mediumButton.onClick.AddListener(SetMedium);
        if (hardButton != null)
            hardButton.onClick.AddListener(SetHard);
    }

    /*
     * Sets the game difficulty to Easy when that button is clicked
     */
    void SetEasy()
    {
        DifficultyManager.Instance.SetEasyDifficulty();
    }

    /*
     * Sets the game difficulty to Medium when that button is clicked
     */
    void SetMedium()
    {
        DifficultyManager.Instance.SetMediumDifficulty();
    }

    /*
     * Sets the game difficulty to Hard when that button is clicked
     */
    void SetHard()
    {
        DifficultyManager.Instance.SetHardDifficulty();
    }
}
