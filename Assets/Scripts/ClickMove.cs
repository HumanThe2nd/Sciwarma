/*
 * ClickMove Script
 * Author: Dan Shan
 * Created: 2025-11-26
 */

using UnityEngine;
using UnityEngine.InputSystem; // new Input System

public class ClickMove : MonoBehaviour
{
    private Vector3 startPos;
    public float moveDistance = 1f;
    public float moveSpeed = 2f;
    private bool movingUp = false;
    private bool movingBack = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Movement logic
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

        // New Input System click detection
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Clicked detected");
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            Collider2D hit = Physics2D.OverlapPoint(mousePos2D);
            if (hit != null && hit.gameObject == gameObject)
            {
                movingUp = true;
            }
        }
    }
}
