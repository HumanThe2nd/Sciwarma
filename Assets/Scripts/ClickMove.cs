/*
 * ClickMove Script
 * Author: Dan Shan
 * Created: 2025-11-26
 * Updated: 2025-11-28
*/


using UnityEngine;
using UnityEngine.InputSystem;

public class ClickMove : MonoBehaviour
{
    private Vector3 startPos;
    public float moveDistance = 1f;
    public float moveSpeed = 2f;
    private bool movingUp = false;
    private bool movingBack = false;

    // reference to PlayerStats
    private PlayerStats playerStats;

    void Start()
    {
        startPos = transform.position;

        // Try in parent first, then scene as fallback
        playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
            Debug.LogWarning("PlayerStats not found in scene - ClickMove functionality may be limited");
    }

    void Update()
    {
        // ---- Movement logic ----
        if (movingUp)
        {
            Vector3 targetPos = startPos + Vector3.up * moveDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (transform.position == targetPos)
            {
                movingUp = false;
                movingBack = true;
            }
        }
        else if (movingBack)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);

            if (transform.position == startPos)
                movingBack = false;
        }

        // ---- Click detection ----
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            Collider2D hit = Physics2D.OverlapPoint(mousePos2D);

            if (hit != null && hit.gameObject == gameObject)
            {
                if (!movingUp && !movingBack)
                {
                    // increase score only if playerStats exists
                    if (playerStats != null)
                    {
                        playerStats.score++;
                        Debug.Log("Score: " + playerStats.score);
                    }

                    movingUp = true;
                }
            }
        }
    }
}
