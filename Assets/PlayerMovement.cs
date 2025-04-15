using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * PlayerMovement.cs
 * 
 * This script controls player character movement, ball handling, and aiming.
 * It manages:
 * - Movement based on keyboard input (WASD)
 * - Aiming based on mouse position
 * - Ball pickup, holding, and release
 * - Visual effects to indicate when a player is holding a ball
 * 
 * Attach this to all player character prefabs.
 */
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;        // How fast the player moves
    public bool canMove = true;         // Whether the player can move (false when throwing)
    public bool isActivePlayer = false; // Whether this is the currently controlled player

    [Header("Visual Effects")]
    public GameObject glowEffect;       // Visual effect to show when holding a ball
    public Color glowColor = Color.red; // Color to use if no glow effect is assigned

    [Header("Ball Handling")]
    public bool holdingBall = false;    // Whether the player is currently holding a ball
    public Transform ballHoldPoint;     // Position where the ball should be held

    [Header("References")]
    public Rigidbody2D rb;              // Rigidbody for rotation (aim direction)
    public Rigidbody2D player;          // Rigidbody for movement
    public Rigidbody2D triAim;          // Optional triangle aiming indicator
    public Animator animator;           // Reference to the animation controller
    public Camera cam;                  // Reference to the main camera for mouse position

    // Private variables
    private Vector2 movement;           // Current movement input
    private Vector2 mousePos;           // Current mouse position in world space
    public GameObject heldBall;         // Reference to the ball being held (if any)

    /*
     * Validation to ensure inspector changes don't create inconsistent state
     */
    private void OnValidate()
    {
        // This ensures inspector changes don't create inconsistent state
        if (holdingBall && heldBall == null)
        {
            holdingBall = false;
        }
    }

    /*
     * Late update handles ball positioning and state validation
     */
    void LateUpdate()
    {
        // This catches and fixes any inconsistent state every frame
        if (holdingBall && heldBall == null)
        {
            holdingBall = false;
        }

        // This makes sure the ball stays with the player if they're holding one
        if (holdingBall && heldBall != null)
        {
            // Position the ball at the hold point
            if (ballHoldPoint != null)
            {
                heldBall.transform.position = ballHoldPoint.position;
            }
            else
            {
                // Default position if no hold point is specified
                heldBall.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            }
        }
    }

    /*
     * Process player input and update animation
     */
    void Update()
    {
        // Don't process input when the game is paused
        if (PauseMenu.GameIsPaused)
            return;

        // Safety check to fix inconsistent state
        if (holdingBall && heldBall == null)
        {
            holdingBall = false;
        }

        // Only process input if this is the active player
        if (!isActivePlayer) return;

        // Get movement input
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Update animator parameters
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            // Only show movement animation if we can move and aren't holding a ball
            animator.SetFloat("Speed", canMove && !holdingBall ? movement.sqrMagnitude : 0);
        }

        // Track mouse position for aiming
        if (cam != null)
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // Ball pickup when E key is pressed
        if (Input.GetKeyDown(KeyCode.E) && !holdingBall)
        {
            TryPickupBall();
        }

        // Final safety check for inconsistent state
        if (holdingBall && heldBall == null)
        {
            holdingBall = false; // Fix the state
        }
    }

    /*
     * Handle physics-based movement and aiming
     */
    void FixedUpdate()
    {
        // Don't update when the game is paused
        if (PauseMenu.GameIsPaused)
            return;

        // Only move if we can move, and either we're not holding a ball or not the active player
        if (canMove && ((!holdingBall) || !isActivePlayer))
        {
            // Apply movement using physics
            player.MovePosition(player.position + movement * moveSpeed * Time.fixedDeltaTime);

            // Update aim objects
            Vector2 lookDir = mousePos - rb.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
            rb.position = player.position;

            // Update triangle aim indicator if it exists
            if (triAim != null)
            {
                triAim.rotation = rb.rotation;
                triAim.position = player.position;
            }
        }
        else if (isActivePlayer)
        {
            // Stop movement if restrictions apply, but still update aim
            player.velocity = Vector2.zero;

            if (rb != null)
            {
                // Update rotation to face mouse even when not moving
                Vector2 lookDir = mousePos - rb.position;
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
                rb.position = player.position;

                // Update triangle aim indicator
                if (triAim != null)
                {
                    triAim.rotation = rb.rotation;
                    triAim.position = player.position;
                }
            }
        }
    }

    /*
     * Called after a ball is thrown to reset player state
     */
    public void ResetAfterThrow()
    {
        holdingBall = false;
        canMove = true;

        // Log only for the active player
        if (isActivePlayer)
        {
            Debug.Log("Player movement reset after throw");
        }
    }

    /*
     * Sets whether this player is the active player (being controlled)
     * Called by CharacterSwap when switching players
     */
    public void SetActiveStatus(bool active)
    {
        isActivePlayer = active;

        // If not active, stop movement
        if (!active && player != null)
        {
            player.velocity = Vector2.zero;
        }
    }

    /*
     * Try to pick up a nearby ball
     */
    void TryPickupBall()
    {
        // Don't try to pick up if already holding
        if (holdingBall) return;

        // Check for nearby balls in a 1-unit radius
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, 1.0f);

        foreach (Collider2D obj in nearbyObjects)
        {
            if (obj.CompareTag("Ball"))
            {
                // Make sure the ball is not being held by another player
                Ball ballComponent = obj.GetComponent<Ball>();
                if (ballComponent != null && ballComponent.currentState == Ball.BallState.Free)
                {
                    // Make sure ball is not moving too fast
                    Rigidbody2D ballRb = obj.GetComponent<Rigidbody2D>();
                    if (ballRb != null && ballRb.velocity.magnitude < 0.5f)
                    {
                        PickupBall(obj.gameObject);
                        break;
                    }
                }
            }
        }
    }

    /*
     * Pick up the ball and update player state
     */
    public void PickupBall(GameObject ball)
    {
        // Safety checks
        if (holdingBall || ball == null)
        {
            return;
        }

        // Update player state
        heldBall = ball;
        holdingBall = true;
        UpdateGlowEffect();

        // Show glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }
        else
        {
            // If no glow effect is assigned, change the player's color
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = glowColor;
            }
        }

        // Configure ball physics
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            ballRb.velocity = Vector2.zero;
            ballRb.isKinematic = true;
        }

        // Position the ball properly
        if (ballHoldPoint != null)
        {
            ball.transform.SetParent(transform);
            ball.transform.position = ballHoldPoint.position;
        }
        else
        {
            ball.transform.SetParent(transform);
            ball.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Update the ball's state
        Ball ballController = ball.GetComponent<Ball>();
        if (ballController != null)
        {
            ballController.SetHeldBy(transform);
        }
    }

    /*
     * Release the ball (called from shooting script)
     */
    public GameObject ReleaseBall()
    {
        // Safety checks
        if (!holdingBall)
        {
            return null;
        }

        if (heldBall == null)
        {
            // Fix the inconsistent state
            holdingBall = false;
            return null;
        }

        // Hide glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
        else
        {
            // Reset color if changed
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }

        // Get the ball reference before clearing
        GameObject ball = heldBall;

        // Detach from player
        ball.transform.SetParent(null);

        // Enable physics
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            ballRb.isKinematic = false;
        }

        // Reset player state
        heldBall = null;
        holdingBall = false;

        UpdateGlowEffect();
        return ball;
    }

    /*
     * Force a player to hold a specific ball.
     * Used during initialization or when restoring game state.
     */
    public void ForceSetBall(GameObject ball)
    {
        // Clear any existing state first
        if (heldBall != null && heldBall != ball)
        {
            Destroy(heldBall);
        }

        // Set new ball
        if (ball != null)
        {
            heldBall = ball;
            holdingBall = true;

            // Make sure the ball is properly parented and positioned
            ball.transform.SetParent(transform);

            if (ballHoldPoint != null)
            {
                ball.transform.position = ballHoldPoint.position;
            }
            else
            {
                ball.transform.localPosition = new Vector3(0, 0.5f, 0);
            }

            // Configure ball physics
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.velocity = Vector2.zero;
                ballRb.isKinematic = true;
            }

            // Notify the ball component
            Ball ballComponent = ball.GetComponent<Ball>();
            if (ballComponent != null)
            {
                ballComponent.SetHeldBy(transform);
            }
        }
        else
        {
            heldBall = null;
            holdingBall = false;
        }

        UpdateGlowEffect();
    }

    /*
     * Creates and sets up a new ball for this player
     */
    public void SetupWithNewBall(GameObject ballPrefab)
    {
        // Clear any existing state
        holdingBall = false;
        heldBall = null;

        // Create a new ball as a child of this player
        if (ballPrefab != null)
        {
            // Choose spawn position
            Vector3 spawnPos = transform.position;
            if (ballHoldPoint != null)
            {
                spawnPos = ballHoldPoint.position;
            }

            // Create the ball
            GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            // Properly setup the ball
            heldBall = ball;
            holdingBall = true;

            // Configure the ball
            ball.transform.SetParent(transform);

            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.isKinematic = true;
            }

            Ball ballComponent = ball.GetComponent<Ball>();
            if (ballComponent != null)
            {
                ballComponent.SetHeldBy(transform);
            }
        }
    }

    /*
     * Updates the glow effect based on ball holding state
     */
    public void UpdateGlowEffect()
    {
        // Show or hide glow based on ball holding state
        if (holdingBall)
        {
            // Show glow effect
            if (glowEffect != null)
            {
                glowEffect.SetActive(true);
            }
            else
            {
                // If no glow effect is assigned, change the player's color
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = glowColor;
                }
            }
        }
        else
        {
            // Hide glow effect
            if (glowEffect != null)
            {
                glowEffect.SetActive(false);
            }
            else
            {
                // Reset color
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.white;
                }
            }
        }
    }

    /*
     * Checks if the player has a valid ball reference
     * return True if the player has a valid ball, false otherwise
     */
    public bool HasValidBall()
    {
        // Check if the heldBall reference is valid and properly parented
        return heldBall != null && heldBall.transform.parent == transform;
    }

    /*
     * Sets the holdingBall flag directly
     */
    public void SetHoldingBall(bool holding)
    {
        holdingBall = holding;
    }
}
