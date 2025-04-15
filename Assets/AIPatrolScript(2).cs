using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * NewBehaviourScript.cs - Enemy Patrol Behavior
 * 
 * This script controls enemy patrol movement along defined waypoints.
 * Enemies can follow either manually placed waypoints or auto-generated paths.
 * 
 * 
 * Attach this to enemy characters that should patrol along a defined path.
 */

public class NewBehaviourScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;                   // How fast the enemy moves
    public float waypointStopDistance = 0.1f;  // How close we need to get to a waypoint before moving to the next
    public bool loopPath = true;               // Whether to loop back to the first waypoint or reverse direction
    public Animator animator;                  // Reference to the animation controller

    [Header("Path Settings")]
    public Transform pathParent;               // Parent object containing waypoint children
    public List<Transform> waypoints = new List<Transform>();  // List of waypoint transforms to follow
    public Color pathColor = Color.green;      // Color for visualizing the path in the editor
    public bool createPathInEditor = false;    // Flag to trigger path creation in editor mode
    public float pathRadius = 5f;              // Radius for auto-generated circular paths

    // Private variables for internal state
    private int currentWaypointIndex = 0;      // Index of the current waypoint we're moving toward
    private bool isMovingForward = true;       // Whether we're moving forward or backward through waypoints
    private Vector2 lastPosition;              // Position from previous frame to calculate movement delta
    private Vector2 movement;                  // Current movement vector (for animation)

    /*
     * Initialize the enemy's movement and pathing system
     */
    void Start()
    {
        // Initialize position tracking for animation
        lastPosition = transform.position;

        // Apply difficulty multiplier to speed if DifficultyManager exists
        if (DifficultyManager.Instance != null)
        {
            speed *= DifficultyManager.Instance.GetEnemySpeedMultiplier();
        }

        // Get waypoints from parent if assigned but no waypoints directly added
        if (pathParent != null && waypoints.Count == 0)
        {
            foreach (Transform child in pathParent)
            {
                waypoints.Add(child);
            }
        }

        // If no waypoints are defined, create a default path
        if (waypoints.Count == 0)
        {
            CreateDefaultPath();
        }
    }

    /*
     * Update enemy movement each frame
     */
    void Update()
    {
        // Skip if no waypoints exist
        if (waypoints.Count == 0) return;

        // Get the current waypoint
        Transform currentWaypoint = waypoints[currentWaypointIndex];

        // Move toward the current waypoint
        transform.position = Vector2.MoveTowards(transform.position, currentWaypoint.position, speed * Time.deltaTime);

        // Calculate movement for animation
        movement = ((Vector2)transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // If reached the waypoint, progress to the next one
        if (Vector2.Distance(transform.position, currentWaypoint.position) <= waypointStopDistance)
        {
            ProgressToNextWaypoint();
        }

        // Update animator parameters if animator exists
        if (animator != null)
        {
            // Update parameters that control which animation to play
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    /*
     * Move to the next waypoint in the sequence based on path settings
     */
    void ProgressToNextWaypoint()
    {
        // Skip if we have only one waypoint (or none)
        if (waypoints.Count <= 1) return;

        if (loopPath)
        {
            // Simple loop around all waypoints
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
        else
        {
            
            if (isMovingForward)
            {
                currentWaypointIndex++;
                // If at the end, reverse direction
                if (currentWaypointIndex >= waypoints.Count - 1)
                {
                    currentWaypointIndex = waypoints.Count - 1;
                    isMovingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                // If back at the start, go forward again
                if (currentWaypointIndex <= 0)
                {
                    currentWaypointIndex = 0;
                    isMovingForward = true;
                }
            }
        }
    }

    /*
     * Creates a default circular patrol path if none is defined
     */
    void CreateDefaultPath()
    {

        // Create parent for waypoints if it doesn't exist
        GameObject pathObj = new GameObject(gameObject.name + "_PatrolPath");
        pathParent = pathObj.transform;

        // Get a reference point - either use current position or find spawner
        Vector3 centerPoint = transform.position;
        SpawnerScript spawner = FindObjectOfType<SpawnerScript>();
        if (spawner != null)
        {
            // Use the spawner position as the center of our patrol path
            centerPoint = spawner.transform.position;
        }

        // Create waypoints in a circle
        int waypointCount = Random.Range(4, 7);  // Random number of waypoints between 4-6

        for (int i = 0; i < waypointCount; i++)
        {
            // Calculate position in a circle
            float angle = i * (360f / waypointCount);
            float x = centerPoint.x + pathRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = centerPoint.y + pathRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

            // Create waypoint object
            GameObject waypointObj = new GameObject("Waypoint_" + i);
            waypointObj.transform.position = new Vector3(x, y, 0);
            waypointObj.transform.parent = pathParent;

            // Add to list
            waypoints.Add(waypointObj.transform);
        }
    }

    /*
     * Creates a custom patrol path from an array of points.
     * Can be called from editor or other scripts.
     */
    public void CreateCustomPath(Vector2[] points)
    {
        // Clear existing waypoints
        waypoints.Clear();

        // Create parent for waypoints if it doesn't exist
        if (pathParent == null)
        {
            GameObject pathObj = new GameObject(gameObject.name + "_PatrolPath");
            pathParent = pathObj.transform;
        }

        // Create waypoints at specified positions
        for (int i = 0; i < points.Length; i++)
        {
            GameObject waypointObj = new GameObject("Waypoint_" + i);
            waypointObj.transform.position = points[i];
            waypointObj.transform.parent = pathParent;

            waypoints.Add(waypointObj.transform);
        }
    }

    /*
     * Visualize the patrol path in the editor
     */
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // Draw the patrol path
        Gizmos.color = pathColor;

        // Draw waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                // Draw waypoint as a sphere
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);

                // Draw line to next waypoint
                if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                else if (i == waypoints.Count - 1 && waypoints[0] != null && loopPath)
                {
                    // Connect last waypoint to first if looping
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }

        // Highlight current waypoint if in play mode
        if (Application.isPlaying && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[currentWaypointIndex].position, 0.3f);
        }
    }

}
