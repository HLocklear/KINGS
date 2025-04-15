using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * GameAudio.cs
 * 
 * This script controls the game's audio, including background music and dynamic
 * adjustments based on game state (such as speeding up when time is running low).
 * 
 * The script works by connecting to the GameTimer to determine when to change the music.
 */

public class GameAudio : MonoBehaviour
{
    [Header("---------Audio Source--------")]
    [SerializeField] AudioSource gameAudio;    // The main audio source component for playing game sounds

    [Header("---------Audio Clip----------")]
    public AudioClip background;               // The main background music track

    [Header("---------Timer Settings------")]
    // When this many seconds remain, the music will speed up to create tension
    public float speedUpThreshold = 30f;       // Seconds left when music speeds up
    // How much faster the music will play when speeding up (1.0 is normal speed)
    public float speedUpPitch = 1.3f;          // How fast the music gets (1.0 is normal)

    // References and state tracking
    private GameTimer gameTimer;               // Reference to the GameTimer script for accessing time remaining
    private bool hasSpedUp = false;            // Flag so we only speed up the music once

    /*
     * Initialize the audio system when the game starts
     */
    private void Start()
    {
        // Set up initial background music and start playing
        gameAudio.clip = background;
        gameAudio.Play();

        // Find the GameTimer script in the scene to monitor time remaining
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer == null)
        {
            Debug.LogWarning("GameTimer not found! Music speed-up feature won't work.");
        }
    }

    /*
     * Check every frame if we need to speed up the music based on remaining time
     */
    private void Update()
    {
        // Only check if we have a valid GameTimer reference and haven't already sped up
        if (gameTimer != null && !hasSpedUp)
        {
            // Access the current time from GameTimer
            float timeRemaining = gameTimer.GetCurrentTime();

            // If time is below thirty seconds, speed up the music to create urgency
            if (timeRemaining <= speedUpThreshold)
            {
                SpeedUpMusic();
                hasSpedUp = true;  // Mark as sped up so we don't trigger again
            }
        }
    }

    /*
     * Increases the pitch of the background music to create tension
     * when time is running low
     */
    private void SpeedUpMusic()
    {
        Debug.Log("Music speeding up! 30 seconds remaining!");

        // Changing pitch also changes speed of the audio
        gameAudio.pitch = speedUpPitch;
    }

    /*
     * Resets the music to normal speed - call this when restarting the game
     */
    public void ResetMusic()
    {
        gameAudio.pitch = 1.0f;       // Reset to normal speed
        hasSpedUp = false;            // Allow speed-up to happen again
    }
}
