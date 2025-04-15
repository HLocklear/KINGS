using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Shooting.cs
 * 
 * This script handles the ball shooting/throwing mechanics.
 * It makes sur4e only one ball can exist in the scene at a time, and
 * manages the ball's physics state when thrown.
 * 
 * Attach this to each player character that can throw a ball.
 */

public class Shooting : MonoBehaviour
{
    public Transform firePoint;           // The position from which the ball is thrown
    public GameObject ballPrefab;         // The ball prefab to instantiate
    public float ballForce = 10f;         // Force applied to the ball when thrown (reduced from 20f to 10f)

    private PlayerMovement playerMovement; // Reference to the associated PlayerMovement component

    // Static tracking of balls to limit how many can exist in the scene at once
    private static List<GameObject> activeBalls = new List<GameObject>();
    private static int maxBalls = 1;      // Maximum number of balls allowed in the scene

    /*
     * Get a reference to the PlayerMovement component on the same GameObject
     */
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    /*
     * Check for input to shoot the ball
     */
    void Update()
    {
        // Don't process input if the game is paused
        if (PauseMenu.GameIsPaused)
            return;

        // Only allow the active player to shoot
        if (playerMovement == null || !playerMovement.isActivePlayer)
            return;

        // Check for left mouse button click to shoot
        if (Input.GetMouseButtonDown(0))
        {
            CreateAndShootNewBall();
        }
    }

    /*
     * Creates a new ball or reuses an existing one, then shoots it
     */
    void CreateAndShootNewBall()
    {
        // Clean up any null references in the list (destroyed balls)
        activeBalls.RemoveAll(ball => ball == null);

        // Either create a new ball or reuse an existing one, depending on how many are active
        if (activeBalls.Count < maxBalls)
        {
            // We're under the max ball limit, create a new one
            GameObject ball = Instantiate(ballPrefab, firePoint.position, firePoint.rotation);
            // Add to our tracking list
            activeBalls.Add(ball);
            // Set up and throw the ball
            SetupAndShootBall(ball);
        }
        else
        {
            // Reuse an existing ball instead of creating a new one
            GameObject existingBall = activeBalls[0];
            // Reposition it at the firepoint for the new throw
            existingBall.transform.position = firePoint.position;
            existingBall.transform.rotation = firePoint.rotation;
            // Set up and throw the ball
            SetupAndShootBall(existingBall);
        }
    }

    /*
     * Sets up a ball's properties and applies force to shoot it
     */
    void SetupAndShootBall(GameObject ball)
    {
        // Make sure the ball is completely detached from any parent objects
        ball.transform.SetParent(null);

        // Force a consistent small scale
        ball.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Make sure the ball has a Rigidbody2D component for physics
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = ball.AddComponent<Rigidbody2D>();
        }

        // Reset all physics properties to ensure clean behavior
        rb.velocity = Vector2.zero;         // Clear any existing velocity
        rb.angularVelocity = 0;             // Stop any rotation
        rb.isKinematic = false;             // Enable physics
        rb.gravityScale = 0;                // No gravity in this 2D top-down game
        rb.drag = 0;                        // No air resistance
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection

        // Reset any physics simulation state to avoid glitches
        rb.Sleep();
        rb.WakeUp();

        // Reset the ball component state
        Ball ballComponent = ball.GetComponent<Ball>();
        if (ballComponent != null)
        {
            ballComponent.ResetState();
        }

        // Get difficulty-based speed multiplier from the DifficultyManager
        float speedMultiplier = 1.0f;  // Default if no DifficultyManager exists
        if (DifficultyManager.Instance != null)
        {
            speedMultiplier = DifficultyManager.Instance.GetBallSpeedMultiplier();
        }

        // Apply force to the ball with difficulty adjustment
        float adjustedForce = ballForce * speedMultiplier;
        rb.AddForce(firePoint.up * adjustedForce, ForceMode2D.Impulse);

        // Update player state to reflect the ball has been thrown
        playerMovement.ResetAfterThrow();
    }
}
