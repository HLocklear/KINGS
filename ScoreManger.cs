using TMPro;
using UnityEngine;
using UnityEngine.Events;
/**
* ScoreManager.cs
* 
* This script handles showing the player's final time when they win,
* collecting their name input, and submitting the final score to the leaderboard.
* 
* Also manages visibility of the score input UI panel.
*/

public class ScoreManager : MonoBehaviour
{
    [Header("Score Submission UI References")]
    [SerializeField] private TextMeshProUGUI scoreDisplay;    // Text showing the final time
    [SerializeField] private TMP_InputField inputName;        // Input field for player name
    [SerializeField] private GameObject inputPanel;           // Panel containing the input fields

    public UnityEvent<string, int> submitScoreEvent;          // Event triggered when submitting the score

    private float finalTime = 0f;                             // The player's final time (score). Still not displaying time correctly, keeps showing 1:29 in the leaderbaord 

    /*
     * Initializes by hiding the input panel when the game starts.
     */
    private void Start()
    {
        if (inputPanel != null)
            inputPanel.SetActive(false);
    }

    /*
     * Displays the input panel and shows the final time after winning.
     */
    public void ShowInputPanel(float remainingTime)
    {
        finalTime = remainingTime;

        if (scoreDisplay != null)
        {
            int minutes = Mathf.FloorToInt(finalTime / 60);
            int seconds = Mathf.FloorToInt(finalTime % 60);
            scoreDisplay.text = string.Format("Your time: {0}:{1:00}", minutes, seconds);
        }

        if (inputPanel != null)
            inputPanel.SetActive(true);
    }

    /*
     * Submits the player's name and score to the leaderboard. Submit name does not work immeadiatately after game over 
     * Converts the time into an integer format (multiplied by 100).
     */
    public void SubmitScore()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            return; // Don't submit if no name entered
        }

        int score = Mathf.RoundToInt(finalTime * 100);

        submitScoreEvent.Invoke(inputName.text, score);

        if (inputPanel != null)
            inputPanel.SetActive(false);
    }
}


