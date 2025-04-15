using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

/**
 * AIMovement.cs
 * 
 * This script controls enemy AI movement behaviors. It provides two main movement patterns:
 * 1. Flee from the player (default behavior)
 * 2. Follow waypoints in a patrol pattern
 * 
 * It also enforces boundaries to keep enemies within the playable area.
 * 
 * Attach this to enemy characters that need to move intelligently.
 */

public class AIMovement : MonoBehaviour
{
    public GameObject enemy;                     // Reference to the player or object to flee from
    public float speed;                          // Movement speed
    public Animator animator;                    // Reference to the animation controller

    [Header("Boundary Settings")]
    public bool usePlayAreaBounds = true;        // Whether to enforce boundaries
    public Transform playAreaCenter;             // Center of the play area
    public float maxDistanceFromCenter = 10f;    // Maximum distance from center the AI can travel

    [Header("Waypoint System")]
    public bool useWaypoints = false;            // Whether to use waypoint movement instead of fleeing
    public List<Transform> waypoints;            // Waypoints to follow if using waypoint system
    public float waypointStopDistance = 0.5f;    // How close to get to a waypoint before moving to the next
    private int currentWaypointIndex = 0;        // Current waypoint index being pursued

    // Private variables for tracking movement
    private Vector2 movement;                    // Current movement vector
    private Vector2 lastPosition;                // Position from previous frame to calculate movement
    private Vector2 targetPosition;              // Target position to move towards

    /*
     * Initialize AI movement
     */
    void Start()
    {
        // Store initial position for movement calculations
        lastPosition = transform.position;

        // If no center is defined, create one at the spawner's position
        if (playAreaCenter == null)
        {
            GameObject centerObj = new GameObject("PlayAreaCenter");
            centerObj.transform.position = FindObjectOfType<SpawnerScript>().transform.position;
            playAreaCenter = centerObj.transform;
        }

        // If using waypoints but none defined, create a circular patrol path
        if (useWaypoints && (waypoints == null || waypoints.Count == 0))
        {
            CreateCircularPatrol();
        }
    }

    /*
     * Update enemy movement each frame
     */
    void Update()
    {
        // Determine movement direction based on AI behavior mode
        Vector2 moveDirection;

        if (useWaypoints && waypoints.Count > 0)
        {
            // Use waypoint system - follow predefined path
            moveDirection = FollowWaypoints();
        }
        else
        {
            // Calculate the direction for fleeing: enemy position minus character position
            // This creates a vector pointing away from the player
            moveDirection = (Vector2)transform.position - (Vector2)enemy.transform.position;
            moveDirection.Normalize();
        }

        // Get the potential new position
        Vector2 potentialNewPos = (Vector2)transform.position + (moveDirection * speed * Time.deltaTime);

        // If using bounds, check if the new position is within the allowed area
        if (usePlayAreaBounds)
        {
            float distanceFromCenter = Vector2.Distance(potentialNewPos, playAreaCenter.position);

            if (distanceFromCenter > maxDistanceFromCenter)
            {
                // If outside bounds, adjust direction to move back toward center
                Vector2 directionToCenter = ((Vector2)playAreaCenter.position - (Vector2)transform.position).normalized;

                // Blend between fleeing and returning to bounds
                // The further outside the boundary, the more we prioritize returning
                //Determines where a value lies between two points 
                float blendFactor = Mathf.InverseLerp(maxDistanceFromCenter, maxDistanceFromCenter + 2f, distanceFromCenter);
                moveDirection = Vector2.Lerp(moveDirection, directionToCenter, blendFactor);
            }
        }

        // Apply the actual movement
        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + moveDirection,speed * Time.deltaTime);

        // Calculate the movement vector based on the change in position
        movement = ((Vector2)transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Update animator parameters to show correct animations
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    /*
     * Calculate direction to follow waypoints in sequence
     */
    Vector2 FollowWaypoints()
    {
        if (waypoints.Count == 0) return Vector2.zero;

        // Get current waypoint
        Transform currentWaypoint = waypoints[currentWaypointIndex];

        // Calculate direction to waypoint
        Vector2 directionToWaypoint = ((Vector2)currentWaypoint.position - (Vector2)transform.position).normalized;

        // Check if we've reached the waypoint
        float distanceToWaypoint = Vector2.Distance(transform.position, currentWaypoint.position);
        if (distanceToWaypoint < waypointStopDistance)
        {
            // Move to next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        return directionToWaypoint;
    }

    /*
     * Create a default circular patrol path around the play area center
     */
    void CreateCircularPatrol()
    {
        waypoints = new List<Transform>();

        // Create 6 waypoints in a circle around the center
        int waypointCount = 6;
        float radius = maxDistanceFromCenter * 0.7f; // 70% of max distance

        for (int i = 0; i < waypointCount; i++)
        {
            // Calculate position in a circle
            float angle = i * (360f / waypointCount);
            float x = playAreaCenter.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = playAreaCenter.position.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            // Create waypoint object
            GameObject waypointObj = new GameObject("Waypoint_" + i);
            waypointObj.transform.position = new Vector3(x, y, 0);
            waypointObj.transform.parent = transform;

            waypoints.Add(waypointObj.transform);
        }
    }

    /*
     * Draw debug visualization when the object is selected in the editor
     */
    void OnDrawGizmosSelected()
    {
        if (playAreaCenter != null && usePlayAreaBounds)
        {
            // Draw the play area boundary
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playAreaCenter.position, maxDistanceFromCenter);
        }

        if (useWaypoints && waypoints != null && waypoints.Count > 0)
        {
            // Draw waypoint paths
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                    // Draw lines between waypoints
                    if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                    else if (i == waypoints.Count - 1 && waypoints[0] != null)
                    {
                        // Connect last waypoint to first
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }
    }
}

