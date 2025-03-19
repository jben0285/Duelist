using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ScoreboardManager : MonoBehaviour
{
    public TextMeshProUGUI scoreboardText;

    void Update()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        string scoreboard = "";

        foreach (PlayerController player in players)
        {
            scoreboard += $"Player {player.OwnerClientId} - Deaths: {player.deathCount.Value}\n";
        }

        scoreboardText.text = scoreboard;
    }
}
