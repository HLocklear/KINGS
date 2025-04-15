using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * DifficultyManager.cs
 * 
 * This script implements a singleton pattern to manage game difficulty settings.
 * It persists between scenes and provides multipliers for enemy and ball speeds
 * based on the selected difficulty.
 * 
 * Other game systems can query this manager to adjust their parameters.
 */

public class DifficultyManager : MonoBehaviour
{// Possible difficulty levels
    public enum Difficulty { Easy, Medium, Hard }

    // allows global access via DifficultyManager.Instance
    public static DifficultyManager Instance { get; private set; }

    // Current difficulty setting
    public Difficulty currentDifficulty = Difficulty.Easy;  // Default to Easy

    // Speed multipliers for each difficulty
    [Header("Enemy Speed Multipliers")]
    public float easyEnemySpeedMultiplier = 0.8f;    // Slower enemies on Easy
    public float mediumEnemySpeedMultiplier = 1.0f;  // Normal speed on Medium
    public float hardEnemySpeedMultiplier = 1.5f;    // Faster enemies on Hard

    [Header("Ball Speed Multipliers")]
    public float easyBallSpeedMultiplier = 0.8f;     // Slower ball on Easy
    public float mediumBallSpeedMultiplier = 1.0f;   // Normal ball speed on Medium
    public float hardBallSpeedMultiplier = 1.3f;     // Faster ball on Hard

    /*
     * So only one DifficultyManager exists across all scenes
     */
    void Awake()
    {
       
        if (Instance == null)
        {
            // If this is the first DifficultyManager, make it the only one
            // Prevent it from being destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a DifficultyManager already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    /*
     * Sets the game difficulty to Easy
     * - Slower enemies
     * - Slower ball speed
     */
    public void SetEasyDifficulty()
    {
        currentDifficulty = Difficulty.Easy;
    }

    /*
     * Sets the game difficulty to Medium
     * - Normal enemy speed
     * - Normal ball speed
     */
    public void SetMediumDifficulty()
    {
        currentDifficulty = Difficulty.Medium;
    }

    /*
     * Sets the game difficulty to Hard
     * - Faster enemies
     * - Faster ball speed
     */
    public void SetHardDifficulty()
    {
        currentDifficulty = Difficulty.Hard;      
    }

    /*
     * Returns the appropriate enemy speed multiplier based on current difficulty.
     * Used by enemy movement scripts to adjust their speeds.
     */
    public float GetEnemySpeedMultiplier()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easyEnemySpeedMultiplier;
            case Difficulty.Medium:
                return mediumEnemySpeedMultiplier;
            case Difficulty.Hard:
                return hardEnemySpeedMultiplier;
            default:
                return 1.0f;  // Fallback to normal speed if something goes wrong
        }
    }

    /*
     * Returns the appropriate ball speed multiplier based on current difficulty.
     * Used by ball throwing scripts to adjust throwing force.
     */
    public float GetBallSpeedMultiplier()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easyBallSpeedMultiplier;
            case Difficulty.Medium:
                return mediumBallSpeedMultiplier;
            case Difficulty.Hard:
                return hardBallSpeedMultiplier;
            default:
                return 1.0f;  // Fallback to normal speed if something goes wrong
        }
    }
}
