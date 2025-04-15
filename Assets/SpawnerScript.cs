using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SpawnerScript.cs
 * 
 * This script manages player characters and their initial setup.
 * It handles:
 * - Activating/deactivating different player characters
 * - Ensuring the active player has a ball
 * - Spawning new players at positions where enemies are eliminated
 * 
 * This is a central manager for player characters in the game.
 */

public class SpawnerScript : MonoBehaviour
{
    // Player prefab references - up to 8 different player characters
    public GameObject ballPrefab;        // The ball prefab
    public GameObject playerPrefab;      // Player 1 prefab
    public GameObject playerPrefab2;     // Player 2 prefab
    public GameObject playerPrefab3;     // Player 3 prefab
    public GameObject playerPrefab4;     // Player 4 prefab
    public GameObject playerPrefab5;     // Player 5 prefab
    public GameObject playerPrefab6;     // Player 6 prefab
    public GameObject playerPrefab7;     // Player 7 prefab
    public GameObject playerPrefab8;     // Player 8 prefab

    // Track which players are currently active in the game
    [HideInInspector] public List<GameObject> activatedPlayers = new List<GameObject>();

    /*
     * Set up the initial game state with active players and their balls
     */
    void Start()
    {
        // Clear the active players list
        activatedPlayers.Clear();

        // Initially, only players 1 and 2 are active
        if (playerPrefab)
        {
            playerPrefab.SetActive(true);
            activatedPlayers.Add(playerPrefab);
        }
        if (playerPrefab2)
        {
            playerPrefab2.SetActive(true);
            activatedPlayers.Add(playerPrefab2);
        }

        // Deactivate all other players until they're unlocked by tagging enemies
        if (playerPrefab3) playerPrefab3.SetActive(false);
        if (playerPrefab4) playerPrefab4.SetActive(false);
        if (playerPrefab5) playerPrefab5.SetActive(false);
        if (playerPrefab6) playerPrefab6.SetActive(false);
        if (playerPrefab7) playerPrefab7.SetActive(false);
        if (playerPrefab8) playerPrefab8.SetActive(false);

        // Fix all active players' ball state 
        foreach (GameObject player in activatedPlayers)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                // Reset any inconsistent state
                pm.holdingBall = false;
            }
        }

        // Set up just the first player with a ball
        if (activatedPlayers.Count > 0 && ballPrefab != null)
        {
            PlayerMovement firstPlayer = activatedPlayers[0].GetComponent<PlayerMovement>();
            if (firstPlayer != null)
            {
                // Create the ball at the proper hold point
                Vector3 spawnPos = firstPlayer.transform.position;
                if (firstPlayer.ballHoldPoint != null)
                {
                    spawnPos = firstPlayer.ballHoldPoint.position;
                }

                // Create the ball
                GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
                ball.transform.SetParent(firstPlayer.transform);

                // Make sure it has physics and is set to kinematic (held state)
                Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    ballRb.isKinematic = true;
                }

                // Set the ball state
                Ball ballComponent = ball.GetComponent<Ball>();
                if (ballComponent != null)
                {
                    ballComponent.SetHeldBy(firstPlayer.transform);
                }

                // Set the player state properly
                firstPlayer.ForceSetBall(ball);
                firstPlayer.SetActiveStatus(true);

            }
        }

        // Final safety check - ensure the active player has a ball
        StartCoroutine(EnsurePlayerHasBall());
    }

    /*
     * Activates a previously inactive player at a specific position.
     * Called when an enemy is eliminated to spawn a new player at that position.
     */
    public void MovePlayerToPosition(int playerNumber, Vector3 position)
    {
        // Get the player GameObject based on its number
        GameObject playerToMove = GetPlayerByNumber(playerNumber);

        if (playerToMove != null)
        {

            // Move player to position and activate
            playerToMove.transform.position = position;
            playerToMove.SetActive(true);

            // Add to activated list if not already there
            if (!activatedPlayers.Contains(playerToMove))
            {
                activatedPlayers.Add(playerToMove);
            }

            // Make sure player can move and isn't holding a ball (initial state)
            PlayerMovement pm = playerToMove.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.canMove = true;      // Make sure this is set to true
                pm.holdingBall = false;  // Make sure this is false so they can move
            }

            // Update character swap system to include this new player
            CharacterSwap charSwap = FindObjectOfType<CharacterSwap>();
            if (charSwap != null)
            {
                // Register the player in the character swap system
                charSwap.RegisterPlayer(playerToMove.transform);
                // Switch to this player immediately
                charSwap.SwitchToPlayer(playerToMove.transform);
            }
        }
    }

    /*
     * Helper method to get player GameObject by number (1-8).
     */
    private GameObject GetPlayerByNumber(int number)
    {
        switch (number)
        {
            case 1: return playerPrefab;
            case 2: return playerPrefab2;
            case 3: return playerPrefab3;
            case 4: return playerPrefab4;
            case 5: return playerPrefab5;
            case 6: return playerPrefab6;
            case 7: return playerPrefab7;
            case 8: return playerPrefab8;
            default: return null;
        }
    }

    /*
     * This is a safety mechanism that runs after initialization to
     * fix any inconsistent state where the active player has no ball.
     */
    private IEnumerator EnsurePlayerHasBall()
    {
        // Wait for one frame to make sure everything is initialized
        yield return null;

        // Find which player is currently active
        PlayerMovement activePlayer = null;
        foreach (GameObject player in activatedPlayers)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null && pm.isActivePlayer)
            {
                activePlayer = pm;
                break;
            }
        }

        // If active player found but doesn't have a ball, give them one
        if (activePlayer != null && !activePlayer.holdingBall && ballPrefab != null)
        {
            // Create a new ball
            GameObject ball = Instantiate(ballPrefab, activePlayer.transform.position, Quaternion.identity);

            // Set up the ball properly
            ball.transform.SetParent(activePlayer.transform);
            if (activePlayer.ballHoldPoint != null)
            {
                ball.transform.position = activePlayer.ballHoldPoint.position;
            }

            // Configure ball physics
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.isKinematic = true;
            }

            // Set up ball component
            Ball ballComponent = ball.GetComponent<Ball>();
            if (ballComponent != null)
            {
                ballComponent.SetHeldBy(activePlayer.transform);
            }

            // Set player state
            activePlayer.ForceSetBall(ball);
        }
    }
}
