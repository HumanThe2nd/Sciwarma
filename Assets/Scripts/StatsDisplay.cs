/*
 * StatsDisplay Script
 * Author: Dan Shan
 * Created: 2025-11-28
*/
using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    public PlayerStats player;

    public TMP_Text scoreText;

    void Update()
    {
        if (player != null && scoreText != null)
        {
            scoreText.text = "Cooking Score: " + player.score;
        }
    }
}
