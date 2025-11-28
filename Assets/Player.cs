using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Character Selection")]
    public string characterName = "Adam";

    [Header("Shooting")]
    public float bulletSpeed = 10f;
    public float fireRate = 0.3f; // Time between shots
    private float nextFireTime = 0f;

    [Header("References - MUST ASSIGN")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer hatRenderer;

    private Rigidbody2D rb;
    private Vector2 movement;
    private string currentDirection = "down";
    private string currentAnimation = "idle_anim";
    private int currentFrame = 0;
    private float frameTimer = 0f;
    public float frameRate = 8f;

    private Character character;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        character = new Character(characterName);
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found! Make sure your camera is tagged as 'MainCamera'");
        }
        else
        {
            Debug.Log($"Camera found: {mainCamera.name}");
        }

        SetSprite("idle_anim", "down", 0);
    }

    void SetSprite(string animation, string direction, int frame)
    {
        if (character == null) return;

        // Get hat and body sprites by extracting 16x16 tiles from the texture
        Sprite hatSprite = character.GetSprite(animation, direction, frame, true);
        Sprite bodySprite = character.GetSprite(animation, direction, frame, false);

        if (hatSprite != null && hatRenderer != null)
        {
            hatRenderer.sprite = hatSprite;
        }

        if (bodySprite != null && bodyRenderer != null)
        {
            bodyRenderer.sprite = bodySprite;
        }
    }

    private bool manualAnimationOverride = false;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        movement = Vector2.zero;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y = 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y = -1;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x = -1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x = 1;

        bool isMoving = movement.magnitude > 0;

        // Test different animations with number keys - these override automatic animations
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            currentAnimation = "idle";
            currentFrame = 0;
            manualAnimationOverride = true;
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            currentAnimation = "idle_anim";
            currentFrame = 0;
            manualAnimationOverride = true;
        }
        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            currentAnimation = "phone";
            currentFrame = 0;
            manualAnimationOverride = true;
        }
        if (keyboard.digit4Key.wasPressedThisFrame)
        {
            currentAnimation = "run";
            currentFrame = 0;
            manualAnimationOverride = true;
        }

        // Automatic animation based on movement (only if not manually overridden)
        if (!manualAnimationOverride)
        {
            if (isMoving)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                    currentDirection = movement.x > 0 ? "right" : "left";
                else
                    currentDirection = movement.y > 0 ? "up" : "down";
                currentAnimation = "run";
            }
            else
            {
                currentAnimation = "idle_anim";
            }
        }
        else
        {
            // If moving while in manual mode, exit manual mode and return to automatic
            if (isMoving)
            {
                manualAnimationOverride = false;
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                    currentDirection = movement.x > 0 ? "right" : "left";
                else
                    currentDirection = movement.y > 0 ? "up" : "down";
                currentAnimation = "run";
            }
        }

        // Test different directions with IJKL keys
        if (keyboard.iKey.wasPressedThisFrame) currentDirection = "up";
        if (keyboard.kKey.wasPressedThisFrame) currentDirection = "down";
        if (keyboard.jKey.wasPressedThisFrame) currentDirection = "left";
        if (keyboard.lKey.wasPressedThisFrame) currentDirection = "right";

        // Switch characters with Q/E keys
        if (keyboard.qKey.wasPressedThisFrame)
        {
            SwitchCharacter(-1);
        }
        if (keyboard.eKey.wasPressedThisFrame)
        {
            SwitchCharacter(1);
        }

        // Shooting with mouse (supports both new and old input systems)
        bool mousePressed = false;
        
        // Try new Input System first
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            mousePressed = mouse.leftButton.isPressed;
            if (mousePressed && Time.frameCount % 30 == 0) // Log every 30 frames to avoid spam
            {
                Debug.Log("Mouse detected via new Input System");
            }
        }
        else
        {
            // Fallback to legacy Input system
            mousePressed = Input.GetMouseButton(0);
            if (mousePressed && Time.frameCount % 30 == 0)
            {
                Debug.Log("Mouse detected via legacy Input System");
            }
        }
        
        if (mousePressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        UpdateAnimation();
        ClampPositionToScreen();
    }
    
    void ClampPositionToScreen()
    {
        if (mainCamera == null) return;
        
        Vector3 pos = transform.position;
        Vector3 viewPos = mainCamera.WorldToViewportPoint(pos);
        
        // Clamp to 0-1 viewport space (with small buffer)
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        
        Vector3 newPos = mainCamera.ViewportToWorldPoint(viewPos);
        transform.position = new Vector3(newPos.x, newPos.y, pos.z);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimation()
    {
        if (character == null) return;

        int framesPerDirection = character.GetFramesPerDirection(currentAnimation);

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % framesPerDirection;
        }

        SetSprite(currentAnimation, currentDirection, currentFrame);
    }

    void SwitchCharacter(int direction)
    {
        string[] characters = { "Adam", "Alex", "Amelia", "Bob" };
        int currentIndex = System.Array.IndexOf(characters, characterName);

        if (currentIndex == -1) currentIndex = 0;

        currentIndex = (currentIndex + direction + characters.Length) % characters.Length;
        characterName = characters[currentIndex];

        character = new Character(characterName);
        currentFrame = 0;

        Debug.Log($"Switched to {characterName}");
    }

    void Shoot()
    {
        if (mainCamera == null) return;

        // Get mouse position in world space (support both input systems)
        Vector3 mousePos;
        
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            // New Input System
            mousePos = mouse.position.ReadValue();
        }
        else
        {
            // Legacy Input System
            mousePos = Input.mousePosition;
        }
        
        // For 2D orthographic camera, use camera's z position
        mousePos.z = mainCamera.nearClipPlane;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        // Get spawn position from body renderer (actual character position)
        Vector3 spawnPos = bodyRenderer != null ? bodyRenderer.transform.position : transform.position;

        // Calculate direction from spawn position to mouse
        Vector2 direction = (worldPos - spawnPos).normalized;
        
        // Offset spawn position slightly in the direction of shooting to avoid hitting player
        spawnPos += (Vector3)(direction * 0.5f);
        
        Debug.Log($"Mouse screen: {mousePos}, world: {worldPos}, spawn: {spawnPos}, direction: {direction}");

        // Create bullet GameObject
        GameObject bulletObj = new GameObject("Bullet");
        bulletObj.transform.position = spawnPos;
        bulletObj.layer = LayerMask.NameToLayer("Default");
        
        Debug.Log($"Created bullet at {bulletObj.transform.position} (player at {transform.position})");

        // Add SpriteRenderer with circular sprite
        SpriteRenderer bulletRenderer = bulletObj.AddComponent<SpriteRenderer>();
        bulletRenderer.sprite = Bullet.CreateCircleBulletSprite(16, new Color(1f, 0.8f, 0f, 1f));
        bulletRenderer.sortingOrder = 10; // Render above most things

        // Add CircleCollider2D for collision detection
        CircleCollider2D collider = bulletObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.isTrigger = true;

        // Add Rigidbody2D
        Rigidbody2D bulletRb = bulletObj.AddComponent<Rigidbody2D>();
        bulletRb.gravityScale = 0;
        bulletRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add Bullet script and set direction
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.speed = bulletSpeed;
        bullet.SetDirection(direction);
    }
}
