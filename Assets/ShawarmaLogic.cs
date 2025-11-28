using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShawarmaLogic : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float fleeSpeed = 4f;
    public float detectionRadius = 5f;
    
    [Header("Wandering")]
    public float wanderChangeInterval = 2f;
    private float wanderTimer = 0f;
    private Vector2 wanderDirection;
    
    private Rigidbody2D rb;
    private Image imageComponent;
    private bool isFleeing = false;
    
    // --- Lives & UI ---
    private int lives = 3;
    private TMP_Text scoreText;
    private SpriteRenderer visualRenderer; // Reference to the child renderer
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Get the UI Image component
        imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            Debug.Log($"Shawarma has Image component with sprite: {imageComponent.sprite?.name ?? "NULL"}");
            
            // Ensure we are inside a Canvas
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.Log("Creating World Space Canvas for Shawarma...");
                
                // Create a new GameObject for the Canvas
                GameObject canvasObj = new GameObject("ShawarmaCanvas");
                canvasObj.transform.position = transform.position;
                
                // Add Canvas component
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                canvas.sortingLayerName = "Default";
                canvas.sortingOrder = 0;
                
                // Add CanvasScaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 100; // Adjust as needed
                
                // Add GraphicRaycaster (needed for UI events usually, but maybe not here)
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Parent Shawarma to this new Canvas
                transform.SetParent(canvasObj.transform, true);
                
                // Standard World Space UI setup:
                canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                
                // Adjust Shawarma RectTransform to be reasonable size in pixels
                RectTransform rect = GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(100, 100); // 1 unit size roughly
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
        else
        {
            Debug.LogWarning("Shawarma has no Image component");
        }
        
        // --- VISUALS SETUP ---
        // Create a separate child object for visuals to avoid RectTransform/UI conflicts
        GameObject visualObj = new GameObject("ShawarmaVisuals");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = Vector3.one;
        
        visualRenderer = visualObj.AddComponent<SpriteRenderer>();
        
        // Assign sprite from Image if possible, or load default
        if (imageComponent != null && imageComponent.sprite != null)
        {
            visualRenderer.sprite = imageComponent.sprite;
        }
        else
        {
            #if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ShawarmaFinal.png");
            if (s != null) visualRenderer.sprite = s;
            #endif
        }
        
        visualRenderer.sortingLayerName = "Default";
        visualRenderer.sortingOrder = 10; // On top of everything
        
        // Force position to 0,0,0 to ensure it's on screen
        transform.position = Vector3.zero;
        
        // Add collider for bullet detection
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = 0.5f; // World units
        
        // Initialize random wander direction
        wanderDirection = Random.insideUnitCircle.normalized;
        
        // --- UI SETUP ---
        // Find ScoreText
        GameObject scoreObj = GameObject.Find("ScoreText");
        if (scoreObj != null)
        {
            scoreText = scoreObj.GetComponent<TMP_Text>();
            UpdateLivesText();
        }
        else
        {
            Debug.LogError("Could not find 'ScoreText' GameObject!");
        }
        
        // Debug info
        Debug.Log($"=== SHAWARMA VISUALS DEBUG ===");
        Debug.Log($"Created visual child object");
        Debug.Log($"Sprite: {visualRenderer.sprite?.name}");
        Debug.Log($"Sorting Order: {visualRenderer.sortingOrder}");
        Debug.Log($"===================");
    }
    
    void Update()
    {
        // Find nearest bullet - use FindObjectsByType instead of tag
        Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        GameObject nearestBullet = null;
        float nearestDistance = detectionRadius;
        
        foreach (Bullet bullet in bullets)
        {
            float distance = Vector2.Distance(transform.position, bullet.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBullet = bullet.gameObject;
            }
        }
        
        // If bullet is nearby, flee from it
        if (nearestBullet != null)
        {
            isFleeing = true;
            Vector2 fleeDirection = (transform.position - nearestBullet.transform.position).normalized;
            rb.linearVelocity = fleeDirection * fleeSpeed;
        }
        else
        {
            // Wander randomly when safe
            isFleeing = false;
            wanderTimer += Time.deltaTime;
            
            if (wanderTimer >= wanderChangeInterval)
            {
                wanderTimer = 0f;
                wanderDirection = Random.insideUnitCircle.normalized;
            }
            
            rb.linearVelocity = wanderDirection * moveSpeed;
        }
        
        // Clamp position to screen boundaries
        ClampPositionToScreen();
    }
    
    void ClampPositionToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Vector3 pos = transform.position;
        Vector3 viewPos = cam.WorldToViewportPoint(pos);
        
        // Clamp to 0-1 viewport space (with small buffer)
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        
        Vector3 newPos = cam.ViewportToWorldPoint(viewPos);
        transform.position = new Vector3(newPos.x, newPos.y, pos.z);
    }
    
    public void OnHit()
    {
        if (lives <= 0) return;
        
        lives--;
        Debug.Log($"Shawarma hit! Lives remaining: {lives}");
        
        UpdateLivesText();
        StartCoroutine(HitEffectRoutine());
        
        if (lives <= 0)
        {
            Die();
        }
    }
    
    void UpdateLivesText()
    {
        if (scoreText != null)
        {
            if (lives > 0)
                scoreText.text = $"Shawarma Lives: {lives}";
            else
                scoreText.text = "Shawarma DEFEATED!";
        }
    }
    
    System.Collections.IEnumerator HitEffectRoutine()
    {
        if (visualRenderer != null)
        {
            visualRenderer.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            visualRenderer.color = Color.white;
        }
    }
    
    void Die()
    {
        Debug.Log("Shawarma died!");
        // Disable movement
        moveSpeed = 0;
        fleeSpeed = 0;
        rb.linearVelocity = Vector2.zero;
        
        // Optional: Destroy(gameObject, 1f);
    }
}
