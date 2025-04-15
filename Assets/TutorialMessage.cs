using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/**
 * TutorialMessage.cs
 * 
 * This script handles the tutorial message window and its confirmation button.
 * When the player clicks the button, it closes the tutorial window and
 * loads the next scene (typically the actual game).
 * 
 * Attach this to the OK/Continue button within your tutorial window.
 */

public class TutorialMessage : MonoBehaviour
{
    // Reference to the entire tutorial window GameObject that should be closed
    public GameObject tutorialWindow;

    /*
     * Set up button click handler and try to find the tutorial window
     * if it wasn't assigned in the Inspector
     */
    void Start()
    {
        // Get the Button component and add a click listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
           
        }
      

        // If tutorial window isn't explicitly set in the inspector,
        // try to find it automatically by looking at the parent's parent
        if (tutorialWindow == null)
        {
            // Try to get the parent of parent (usually the Tutorial window)
            Transform parent = transform.parent;
            if (parent != null && parent.parent != null)
            {
                tutorialWindow = parent.parent.gameObject;
               
            }
        }
    }

    /*
     * Called when the button is clicked - closes the tutorial and starts the game
     */
    public void OnButtonClick()
    {
      
        // Hide the tutorial window
        if (tutorialWindow != null)
        {
            tutorialWindow.SetActive(false);
        }
      

        // Load the next scene in the build index (typically the game scene)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
     
    }
}
