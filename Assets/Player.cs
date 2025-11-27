using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    
    [Header("Character Selection")]
    public string characterName = "Adam";
    
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
        
        UpdateAnimation();
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
}