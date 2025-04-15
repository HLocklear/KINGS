using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * CinemachineBounds.cs
 * 
 * This script constrains a Cinemachine virtual camera to stay within defined boundaries.
 * It prevents the camera from showing areas outside the playable game area.
 * 
 * Attach this to the same GameObject as your CinemachineVirtualCamera.
 */

public class CinemachineBounds : MonoBehaviour
    // Manually seeting camerabounds because Cinemachine Polygon Collider was not triggering 
{
    [Header("References")]
    [Tooltip("Reference to the virtual camera")]
    public CinemachineVirtualCamera virtualCamera;    // The virtual camera to constrain

    [Header("Boundary Settings")]
    [Tooltip("The left boundary of the game area")]
    public float leftBoundary = -15f;                 // Left edge of the play area

    [Tooltip("The right boundary of the game area")]
    public float rightBoundary = 18f;                 // Right edge of the play area

    [Tooltip("The bottom boundary of the game area")]
    public float bottomBoundary = -8.5f;              // Bottom edge of the play area

    [Tooltip("The top boundary of the game area")]
    public float topBoundary = 9f;                    // Top edge of the play area

    [Header("Debug Visualization")]
    public bool showBoundary = true;                  // Whether to draw gizmos showing the boundaries
    public Color boundaryColor = Color.green;         // Color of the boundary visualization

    // Private variables
    private Camera mainCamera;                        // Reference to the main camera
    private CinemachineFramingTransposer framingTransposer;  // Reference to the framing component
    private float camHalfHeight;                      // Half the camera's height in world units
    private float camHalfWidth;                       // Half the camera's width in world units
    private Transform virtualCamTransform;            // Transform of the virtual camera

    /*
     * Initialize camera references and calculate dimensions
     */
    void Start()
    {
        // Find the virtual camera if not assigned in inspector
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                {
                    enabled = false;  // Disable this component if no camera is found
                    return;
                }
            }
        }

        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            enabled = false;  // Disable this component if no main camera is found
            return;
        }

        // Get the virtual camera transform 
        // (child object that holds the actual camera position)
        virtualCamTransform = virtualCamera.transform;

        // Get the framing transposer component that controls camera positioning
        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // Calculate initial camera dimensions
        UpdateCameraDimensions();
    }

    /*
     * Late update ensures this runs after the camera position is updated by Cinemachine
     */
    void LateUpdate()
    {
        if (virtualCamera == null || mainCamera == null) return;

        // Update camera dimensions (needed if orthographic size changes)
        UpdateCameraDimensions();

        // Get current position of the virtual camera
        Vector3 pos = virtualCamTransform.position;

        // Calculate the clamped position - keep the camera inside boundaries
        // We subtract half the camera's width/height from the boundaries so
        // the edges of the camera view stay within the boundaries
        float clampedX = Mathf.Clamp(pos.x, leftBoundary + camHalfWidth, rightBoundary - camHalfWidth);
        float clampedY = Mathf.Clamp(pos.y, bottomBoundary + camHalfHeight, topBoundary - camHalfHeight);

        // Check if position needs adjustment
        if (clampedX != pos.x || clampedY != pos.y)
        {
            // Force the virtual camera position
            virtualCamTransform.position = new Vector3(clampedX, clampedY, pos.z);

            // If we have a framing transposer, adjust its screen position
            // This helps prevent the camera from fighting against our position constraints
            if (framingTransposer != null)
            {
                // Calculate adjustment ratio
                Vector2 screenAdjustment = new Vector2(
                    (clampedX - pos.x) / (2 * camHalfWidth),
                    (clampedY - pos.y) / (2 * camHalfHeight)
                );

                // Apply small adjustments to counter-act the camera trying to return to center
                framingTransposer.m_TrackedObjectOffset += new Vector3(screenAdjustment.x, screenAdjustment.y, 0) * 0.1f;
            }
        }
    }

    /*
     * Updates the camera's dimensions based on its current settings
     */
    void UpdateCameraDimensions()
    {
        if (mainCamera == null) return;

        // Get current orthographic size (or calculate from FOV if perspective)
        float orthoSize = virtualCamera.m_Lens.OrthographicSize;

        // Calculate half-height and half-width
        camHalfHeight = orthoSize;
        camHalfWidth = camHalfHeight * mainCamera.aspect;
    }

    /*
     * Draw visible boundaries in the Scene view to help with level design
     */
    void OnDrawGizmos()
    {
        if (!showBoundary) return;

        // Only update camera dimensions in editor mode if we don't have them yet
        if (Application.isPlaying == false || mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;

            if (virtualCamera == null)
            {
                virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
                if (virtualCamera == null) return;
            }

            float orthoSize = virtualCamera.m_Lens.OrthographicSize;
            camHalfHeight = orthoSize;
            camHalfWidth = camHalfHeight * mainCamera.aspect;
        }

        // Draw the outer boundary (the absolute limits of the game world)
        Gizmos.color = boundaryColor;
        Gizmos.DrawLine(new Vector3(leftBoundary, bottomBoundary, 0), new Vector3(rightBoundary, bottomBoundary, 0));
        Gizmos.DrawLine(new Vector3(rightBoundary, bottomBoundary, 0), new Vector3(rightBoundary, topBoundary, 0));
        Gizmos.DrawLine(new Vector3(rightBoundary, topBoundary, 0), new Vector3(leftBoundary, topBoundary, 0));
        Gizmos.DrawLine(new Vector3(leftBoundary, topBoundary, 0), new Vector3(leftBoundary, bottomBoundary, 0));

        // Draw the inner boundary (the limits of where the camera can actually move)
        // This is inset by half the camera width/height
        Gizmos.color = new Color(boundaryColor.r, boundaryColor.g, boundaryColor.b, 0.5f);
        float innerLeft = leftBoundary + camHalfWidth;
        float innerRight = rightBoundary - camHalfWidth;
        float innerBottom = bottomBoundary + camHalfHeight;
        float innerTop = topBoundary - camHalfHeight;

        Gizmos.DrawLine(new Vector3(innerLeft, innerBottom, 0), new Vector3(innerRight, innerBottom, 0));
        Gizmos.DrawLine(new Vector3(innerRight, innerBottom, 0), new Vector3(innerRight, innerTop, 0));
        Gizmos.DrawLine(new Vector3(innerRight, innerTop, 0), new Vector3(innerLeft, innerTop, 0));
        Gizmos.DrawLine(new Vector3(innerLeft, innerTop, 0), new Vector3(innerLeft, innerBottom, 0));
    }
}
