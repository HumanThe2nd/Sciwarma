using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        Debug.Log($"Bullet created with direction: {direction}");
        
        // Destroy bullet after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void FixedUpdate()
    {
        if (rb != null && direction != Vector2.zero)
        {
            // Use velocity for movement
            rb.linearVelocity = direction * speed;
        }
    }
    
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        Debug.Log($"Bullet direction set to: {direction}");
        
        // Set velocity immediately if rb exists
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ONLY destroy if we hit the shawarma, ignore everything else including player
        ShawarmaLogic shawarma = collision.gameObject.GetComponent<ShawarmaLogic>();
        if (shawarma != null)
        {
            shawarma.OnHit();
            Destroy(gameObject);
            Debug.Log("Bullet hit shawarma!");
        }
    }
    
    /// <summary>
    /// Creates a circular bullet sprite programmatically
    /// </summary>
    public static Sprite CreateCircleBulletSprite(int size = 16, Color color = default)
    {
        if (color == default)
        {
            color = new Color(1f, 0.8f, 0f, 1f); // Yellow-orange
        }
        
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        // Create a circle using distance from center
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                // Anti-aliasing for smoother edges
                if (distance < radius - 1f)
                {
                    pixels[y * size + x] = color;
                }
                else if (distance < radius)
                {
                    float alpha = radius - distance;
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear; // Smooth rendering for circle
        
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );
        
        return sprite;
    }
}
