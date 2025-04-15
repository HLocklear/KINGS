using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/**
 * GameTimer.cs
 * 
 * This script manages the game's timer and win/lose conditions.
 * It counts down from a set time and tracks the number of surviving enemies.
 * The game is won if all enemies are eliminated before time runs out,
 * or lost if time runs out before all enemies are eliminated.
 * 
 * 
 */

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 90f;      // Starting time in seconds (1:30)
    private float currentTime;         // Current remaining time

    [Header("UI References")]
    public TextMeshProUGUI timerText;             // Text element to display time
    public TextMeshProUGUI enemiesRemainingText;  // Text element to display remaining enemies
    public GameObject winPanel;                   // Panel to show when player wins
    public GameObject losePanel;                  // Panel to show when player loses

    [Header("Game State")]
    private bool gameOver = false;     // Flag to track if the game has ended
    private int totalEnemies;          // Total number of enemies at the start
    private int remainingEnemies;      // Current number of remaining enemies

    /*
     * Initialize the game state, timer, and UI elements
     */
    void Start()
    {
        // Initialize timer to the starting time
        currentTime = startTime;
        UpdateTimerDisplay();

        // Count how many enemies are in the scene initially
        CountRemainingEnemies();

        // Make sure the win/lose panels are hidden at the start
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        // Make sure we're not in a paused state (in case this was set in a previous game)
        Time.timeScale = 1.0f;
    }

    /*
     * Update the timer and check win/lose conditions every frame
     */
    void Update()
    {
        // Don't update anything if the game is already over
        if (gameOver) return;

        // Update timer countdown
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame(false);  // Time's up player loses
        }

        // Update the timer display with current time
        UpdateTimerDisplay();

        // recount enemies to handle any that might have been
        // destroyed without notifying this script
        if (Time.frameCount % 30 == 0)  // Check roughly twice per second at 60fps
        {
            CountRemainingEnemies();
        }
    }

    /*
     * Updates the UI text showing the current time
     */
    void UpdateTimerDisplay()
    {
        if (timerText)
        {
            // Convert time to minutes and seconds format
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    /*
     * Updates the UI text showing how many enemies remain
     */
    void UpdateEnemiesDisplay()
    {
        if (enemiesRemainingText)
        {
            enemiesRemainingText.text = "Survivors Remaining: " + remainingEnemies;
        }
    }

    /*
     * Counts how many enemies currently exist in the scene.
     * Also checks the win condition (all enemies eliminated).
     */
    void CountRemainingEnemies()
    {
        // Find all GameObjects tagged as "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        remainingEnemies = enemies.Length;

        // Keep track of the maximum number of enemies we've seen
        // This helps so we don't trigger a win if there were never any enemies
        totalEnemies = Mathf.Max(totalEnemies, remainingEnemies);

        // Update the display with current count
        UpdateEnemiesDisplay();

        // Check if all enemies are eliminated - player wins if so
        if (remainingEnemies == 0 && totalEnemies > 0)
        {
            EndGame(true);  // Player wins
        }
    }

    /*
     * Called when a ball hits an enemy.
     * Decrements enemy count and checks win condition.
     * Can be called directly from your Ball script.
     */
    public void EnemyTagged()
    {
        // Reduce enemy count
        remainingEnemies--;
        UpdateEnemiesDisplay();

        // Check if all enemies are tagged
        if (remainingEnemies <= 0 && totalEnemies > 0)
        {
            EndGame(true);  // Player wins
        }
    }

    /*
     * Like EnemyTagged, but also receives the position where
     * the enemy was tagged. This could be used for visual effects.
     */
    public void EnemyTaggedAtPosition(Vector3 position)
    {
        // Update the enemy count
        EnemyTagged();

        // This could be used to spawn effects at this position
        // Example: Instantiate(tagEffect, position, Quaternion.identity);
    }

    /*
     * Handles the end of the game - either victory or defeat
     */
    void EndGame(bool playerWon)
    {
        // Prevent this from being called multiple times
        if (gameOver) return;

        // Set the game over flag
        gameOver = true;

        if (playerWon)
        {
            if (winPanel) winPanel.SetActive(true);
        }
        else
        {
            if (losePanel) losePanel.SetActive(true);
        }

        // Pause the game when showing results
        Time.timeScale = 0f;
    }

    /*
     * Button handler for "Play Again" buttons in win/lose panels
     */
    public void RestartGame()
    {
        // Reset time scale before loading scene
        Time.timeScale = 1.0f;

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /*
     * Button handler for "Main Menu" buttons in win/lose panels
     */
    public void ReturnToMainMenu()
    {
        // Reset time scale before loading scene
        Time.timeScale = 1.0f;

        // Load the first scene (assumed to be the menu)
        SceneManager.LoadScene(0);
    }

    /*
     * Helper method for debugging and testing - adds time to the clock
     */
    public void AddTime(float seconds)
    {
        currentTime += seconds;
        UpdateTimerDisplay();
    }

    /*
     * Public accessor for the current time remaining.
     * Used by other scripts that need to know the time (like GameAudio).
     */
    public float GetCurrentTime()
    {
        return currentTime;
    }

}

