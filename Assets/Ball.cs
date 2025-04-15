using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Ball.cs
 * 
 * This script controls the ball's behavior - the primary game object that players throw
 * to tag enemies. It handles physics, collision detection, state management, and
 * spawning new players when enemies are tagged.
 * 
 * The ball can be in one of three states:
 * - Free: Not held by a player, moving freely with physics
 * - Held: Being carried by a player
 * - Thrown: In flight after being thrown by a player
 */

public class Ball : MonoBehaviour
{
    [Header("Boundary Settings")]
    public bool useBoundaries = true;           // Whether to use world boundaries
    public float minX = -15f;                   // Left boundary of the world
    public float maxX = 18f;                    // Right boundary of the world
    public float minY = -8.5f;                  // Bottom boundary of the world
    public float maxY = 9f;                     // Top boundary of the world
    public float bounceBackStrength = 1.5f;     // How strongly the ball bounces off boundaries

    [Header("Ball Physics")]
    public float bounceStrength = 0.6f;         // How much velocity is retained when bouncing (0-1)
    public float friction = 0.98f;              // Some friction applied each frame to slow the ball (0-1)
    public float stopThreshold = 0.1f;          // Velocity below which the ball stops completely
    public float maxLifetime = 10f;             // Maximum time the ball can be in thrown state

    // Possible states for the ball
    public enum BallState { Free, Held, Thrown }

    [Header("Ball State")]
    public BallState currentState = BallState.Free;  // Current state of the ball

    [Header("Visual Effects")]
    public TrailRenderer trailEffect;           // Trail effect shown when the ball is thrown
    public GameObject hitEffect;                // Optional effect shown when the ball hits something

    // Private variables
    private Rigidbody2D rb;                     // Reference to the ball's rigidbody
    private CircleCollider2D circleCollider;    // Reference to the ball's collider
    private Transform currentHolder;            // Reference to the player currently holding the ball
    private float throwTime;                    // Time when the ball was last thrown
    private int nextPlayerIndex = 3;            // Next player index to activate (start with 3 since 1 and 2 are already active)
    private Vector2 lastFrameVelocity;          // Velocity from the previous frame (for bounce calculations)
    private GameTimer gameTimer;                // Reference to the game timer for tracking enemy elimination

    /*
     * Initialize ball components
     */
    void Awake()
    {
        // Get component references
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Disable trail if any
        if (trailEffect != null)
        {
            trailEffect.emitting = false;
        }

        // Find the game timer in the scene
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer == null)
        {
            Debug.LogWarning("GameTimer not found in scene. Enemy elimination tracking won't work.");
        }
    }

    /*
     * Handle ball physics update
     */
    void FixedUpdate()
    {
        // Store velocity for bounce calculations
        lastFrameVelocity = rb.velocity;

        // Apply friction when ball is free
        if (currentState == BallState.Free && rb != null)
        {
            // Gradually slow down the ball
            rb.velocity *= friction;

            // Stop the ball completely if it's moving very slowly
            if (rb.velocity.magnitude < stopThreshold)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // Enforce boundaries for the ball
        if (useBoundaries && rb != null && currentState != BallState.Held)
        {
            Vector2 position = transform.position;
            Vector2 velocity = rb.velocity;
            bool hitBoundary = false;

            // Check X boundaries
            if (position.x < minX)
            {
                // Hit left boundary, bounce right
                position.x = minX;
                velocity.x = Mathf.Abs(velocity.x) * bounceBackStrength;
                hitBoundary = true;
            }
            else if (position.x > maxX)
            {
                // Hit right boundary, bounce left
                position.x = maxX;
                velocity.x = -Mathf.Abs(velocity.x) * bounceBackStrength;
                hitBoundary = true;
            }

            // Check Y boundaries
            if (position.y < minY)
            {
                // Hit bottom boundary, bounce up
                position.y = minY;
                velocity.y = Mathf.Abs(velocity.y) * bounceBackStrength;
                hitBoundary = true;
            }
            else if (position.y > maxY)
            {
                // Hit top boundary, bounce down
                position.y = maxY;
                velocity.y = -Mathf.Abs(velocity.y) * bounceBackStrength;
                hitBoundary = true;
            }

            // Apply changes if we hit a boundary
            if (hitBoundary)
            {
                transform.position = position;
                rb.velocity = velocity;
            }
        }

        // Check for ball lifetime - prevent balls from flying forever
        if (currentState == BallState.Thrown && Time.time - throwTime > maxLifetime)
        {
            // Transition to free state if thrown for too long
            currentState = BallState.Free;

            // Disable trail if any
            if (trailEffect != null)
            {
                trailEffect.emitting = false;
            }
        }
    }

    /*
     * Handle collision with walls and other physical objects
     */
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle bounce physics when hitting walls
        if (currentState == BallState.Thrown && collision.gameObject.CompareTag("Wall"))
        {
            Bounce(collision.contacts[0].normal);
        }
    }

    /*
     * Handle trigger collisions (used for enemy tagging)
     */
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy while in thrown state
        if (collision.CompareTag("Enemy") && currentState == BallState.Thrown)
        {
            // Notify the game timer about the enemy hit
            if (gameTimer != null)
            {
                gameTimer.EnemyTaggedAtPosition(collision.transform.position);
            }

            // Store the enemy position
            Vector3 enemyPosition = collision.transform.position;

            // Find spawner script to activate a new player
            SpawnerScript spawner = FindObjectOfType<SpawnerScript>();
            if (spawner != null)
            {
                // Call the method to move the next player to the enemy's position
                spawner.MovePlayerToPosition(nextPlayerIndex, enemyPosition);

                // Increment for next enemy (so we activate players 3, 4, 5, etc.)
                nextPlayerIndex++;

                // Destroy the enemy
                Destroy(collision.gameObject);

                // Change ball state for pickup
                currentState = BallState.Free;

                // Slow down the ball significantly so it can be picked up
                if (rb != null)
                {
                    rb.velocity *= 0.1f; // Reduce to 10% of speed
                }
            }
        }
    }

    /*
     * Calculate bounce direction when hitting a surface
     */
    void Bounce(Vector2 surfaceNormal)
    {
        // Calculate reflection direction
        Vector2 reflectedVelocity = Vector2.Reflect(lastFrameVelocity, surfaceNormal);

        // Apply bounce with reduced strength
        rb.velocity = reflectedVelocity * bounceStrength;
    }

    /*
     * Called when a player picks up the ball
     */
    public void SetHeldBy(Transform holder)
    {
        currentHolder = holder;
        currentState = BallState.Held;

        // Disable physics
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        // Disable trail if any
        if (trailEffect != null)
        {
            trailEffect.emitting = false;
        }
    }

    /*
     * Called when a player throws the ball
     */
    public void Throw(Vector2 direction, float force)
    {
        currentHolder = null;
        currentState = BallState.Thrown;
        throwTime = Time.time;

        // Enable physics and apply force
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }     

        // Enable trail if any
        if (trailEffect != null)
        {
            trailEffect.Clear();
            trailEffect.emitting = true;
        }
    }

    /*
     * Reset the ball's state completely for a new throw.
     * Called by the shooting script to ensure clean physics.
     */
    public void ResetState()
    {
        // Reset to thrown state with a clean slate
        currentState = BallState.Thrown;

        // Ensure the rigidbody is properly reset
        if (rb != null)
        {
            // Clear any existing velocity/forces
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep(); // Ensure the physics engine isn't tracking residual motion
            rb.WakeUp(); // Then wake it up for new forces
        }

        // Reset collision detection
        Collider2D ballCollider = GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            // Temporarily toggle to refresh collision system
            ballCollider.enabled = false;
            ballCollider.enabled = true;
        }

        // Reset the throw time
        throwTime = Time.time;
    }
}


