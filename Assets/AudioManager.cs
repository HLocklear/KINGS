using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //Make sure this is here for all UI elements

/**
 * AudioManager.cs
 * 
 * This script manages global audio settings for the game, particularly the
 * background music and its volume control.
 * 
 * Place this on a GameObject that should persist between scenes.
 */

public class AudioManager : MonoBehaviour
{
    [Header("---------Audio Source--------")]
    [SerializeField] AudioSource musicSource;      // The main music audio source component

    [Header("---------Audio Clip----------")]
    public AudioClip background;                   // The main background music track

    [Header("---------Volume Control--------")]
    public Slider volumeSlider;                    // Reference to the UI slider for volume control
    private float defaultVolume = 0.75f;           // Default volume if no saved preference exists

    /*
     * Sets up audio and volume controls when the game starts
     */
    private void Start()
    {
        // Set up the background music and start playback
        musicSource.clip = background;
        musicSource.Play();

        // Configure the volume slider if it exists
        if (volumeSlider != null)
        {
            // Try to load previously saved volume preference, or use default value
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);

            // Apply the volume setting to the audio source
            musicSource.volume = savedVolume;

            // Set the UI slider to match the current volume
            volumeSlider.value = savedVolume;

            // Set up a listener to detect when the player changes the volume slider
            volumeSlider.onValueChanged.AddListener(delegate { AdjustVolume(); });
        }

      
    }

    /*
     * Adjusts the volume based on the slider's current value.
     * Called automatically when the player moves the volume slider.
     * Also saves the setting for future game sessions.
     */
    public void AdjustVolume()
    {
        // Safety check - ensure slider exists
        if (volumeSlider == null) return;

        // Get the current slider value
        float volumeValue = volumeSlider.value;

        // Apply volume setting to the music
        musicSource.volume = volumeValue;

        // Save this preference for future game sessions
        PlayerPrefs.SetFloat("MusicVolume", volumeValue);

        Debug.Log("Volume changed to " + volumeValue);
    }
}
