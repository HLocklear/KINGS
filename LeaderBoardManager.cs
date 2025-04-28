using Dan.Main;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * LeaderBoardManager.cs
 * 
 * This script manages the game's leaderboard using an online service.
 * It handles displaying player names and scores, uploading new scores, 
 * and formatting times correctly.
 * 
 * Used to show, hide, and refresh the leaderboard UI panel.
 */

public class LeaderBoardManager : MonoBehaviour
{
    [Header("Leaderboard UI References")]
    [SerializeField] private List<TextMeshProUGUI> names;         // List of player name fields
    [SerializeField] private List<TextMeshProUGUI> scores;        // List of player score fields
    [SerializeField] private GameObject leaderboardPanel;         // Panel containing the leaderboard UI
    [SerializeField] private TextMeshProUGUI lastTimeText;         // Text field for last player's time

    // Public key for accessing the leaderboard from https://danqzq.itch.io/leaderboard-creator 
    private string publicLeaderboardKey = "197b12a88dfc38906e57dd1ed2a0b4fff5047f45849ad58ddd20c308b16be8ce";

    /*
     * Initializes the leaderboard by fetching current scores
     * and hides the leaderboard panel initially.
     */
    private void Start()
    {
        GetLeaderboard();

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    /*
     * Displays the leaderboard panel and updates the player's last time.
     */
    public void ShowLeaderboard()
    {
        float raw = PlayerPrefs.GetFloat("LastTime", -1f);

        if (lastTimeText != null)
        {
            if (raw >= 0f)
            {
                int m = Mathf.FloorToInt(raw / 60f);
                int s = Mathf.FloorToInt(raw % 60f);
                lastTimeText.text = $"{m}:{s:00}";
            }
            else
            {
                lastTimeText.text = "--:--";
            }
        }

        GetLeaderboard();

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
    }

    /*
     * Hides the leaderboard panel from view.
     */
    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    /*
     * Fetches the latest leaderboard data from the server 
     * and updates the UI with player names and times.
     */
    public void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, (msg) => {
            for (int i = 0; i < names.Count && i < msg.Length; i++)
            {
                names[i].text = msg[i].Username;

                float timeValue = msg[i].Score / 100f;
                int minutes = Mathf.FloorToInt(timeValue / 60);
                int seconds = Mathf.FloorToInt(timeValue % 60);

                scores[i].text = string.Format("{0}:{1:00}", minutes, seconds);
            }

            // Clear unused leaderboard slots
            for (int i = msg.Length; i < names.Count; i++)
            {
                if (names[i] != null) names[i].text = "---";
                if (scores[i] != null) scores[i].text = "--:--";
            }
        });
    }

    /*
     * Uploads a new leaderboard entry with the given username and score,
     * then refreshes the leaderboard display.
     */
    public void SetLeaderboardEntry(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, score, (msg) => {
            GetLeaderboard();
        });
    }
}