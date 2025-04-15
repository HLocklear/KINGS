using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/**
 * CharacterSwap.cs
 * 
 * This script manages switching between different playable characters.
 * It tracks which characters are currently active in the game and handles
 * switching control between them with camera follow.
 * 
 * This system allows the player to switch between unlocked characters
 * using hotkeys (Q for previous, F for next).
 */

public class CharacterSwap : MonoBehaviour
{
    // Current active player that the player is controlling
    public Transform currentPlayer;

    // All possible characters in the scene (including inactive ones)
    public List<Transform> allPlayers = new List<Transform>();

    // Currently activated/available players that the player can control
    [HideInInspector] public List<Transform> activePlayers = new List<Transform>();

    // Camera reference for following the active player
    public CinemachineVirtualCamera vcam;

    // Reference to pause menu to prevent character swapping when paused
    private PauseMenu pauseMenu;

    /*
     * Initialize the character swap system
     */
    void Start()
    {
        // Find the pause menu
        pauseMenu = FindObjectOfType<PauseMenu>();

        // Initialize with the first two players (initially available)
        for (int i = 0; i < Mathf.Min(2, allPlayers.Count); i++)
        {
            if (allPlayers[i] != null)
            {
                activePlayers.Add(allPlayers[i]);
                allPlayers[i].gameObject.SetActive(true);
            }
        }

        // Set initial active player
        if (activePlayers.Count > 0)
        {
            currentPlayer = activePlayers[0];
        }

        // Disable all movement components initially
        DisableAllPlayers();

        // Enable movement for the current player
        UpdateActivePlayer();
    }

    /*
     * Check for input to switch between characters
     */
    void Update()
    {
        // Don't allow character switching when the game is paused
        if (PauseMenu.GameIsPaused)
            return;

        // Don't check for input if we have 0 or 1 active players
        if (activePlayers.Count <= 1) return;

        // Switch to previous character with Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchToPrevious();
        }

        // Switch to next character with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchToNext();
        }
    }

    /*
     * Switch to the previous player in the active list (circular)
     */
    public void SwitchToPrevious()
    {
        if (activePlayers.Count <= 1) return;

        // Get the index of the current player
        int currentIndex = activePlayers.IndexOf(currentPlayer);

        // Calculate the previous index (wrapping to the end if at the start)
        int prevIndex = (currentIndex - 1 + activePlayers.Count) % activePlayers.Count;

        // Set the new current player
        currentPlayer = activePlayers[prevIndex];
        UpdateActivePlayer();
    }

    /*
     * Switch to the next player in the active list (circular)
     */
    public void SwitchToNext()
    {
        if (activePlayers.Count <= 1) return;

        // Get the index of the current player
        int currentIndex = activePlayers.IndexOf(currentPlayer);

        // Calculate the next index (wrapping to the beginning if at the end)
        int nextIndex = (currentIndex + 1) % activePlayers.Count;

        // Set the new current player
        currentPlayer = activePlayers[nextIndex];
        UpdateActivePlayer();
    }

    /*
     * Register a new player as active/available for control.
     * Called by SpawnerScript when a new player is activated.
     */
    public void RegisterPlayer(Transform player)
    {
        if (player != null && !activePlayers.Contains(player))
        {
            // Add to active list and make sure object is active
            activePlayers.Add(player);
            player.gameObject.SetActive(true);
        }
    }

    /*
     * Switch to a specific player by reference.
     */
    public void SwitchToPlayer(Transform player)
    {
        if (player != null && activePlayers.Contains(player))
        {
            currentPlayer = player;
            UpdateActivePlayer();
        }
    }

    /*
     * Disable control for all players to prepare for switching
     */
    private void DisableAllPlayers()
    {
        foreach (Transform player in allPlayers)
        {
            if (player != null)
            {
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    // Use the new method if available
                    if (movement.GetType().GetMethod("SetActiveStatus") != null)
                    {
                        movement.SetActiveStatus(false);
                    }
                    else
                    {
                        // Fall back to the old disable method
                        movement.enabled = false;
                    }
                }
            }
        }
    }

    /*
     * Update the active player by enabling their movement, updating camera, etc.
     */
    private void UpdateActivePlayer()
    {
        // Disable all players first
        DisableAllPlayers();

        // Enable movement for current player
        if (currentPlayer != null)
        {
            PlayerMovement movement = currentPlayer.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                // Use the new method if available
                if (movement.GetType().GetMethod("SetActiveStatus") != null)
                {
                    movement.SetActiveStatus(true);
                }
                else
                {
                    // Fall back to the old enable method
                    movement.enabled = true;
                }
            }

            // Update camera to follow the new active player
            if (vcam != null)
            {
                vcam.LookAt = currentPlayer;
                vcam.Follow = currentPlayer;
            }
        }
    }

  
}
