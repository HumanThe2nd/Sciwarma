using UnityEngine;

[System.Serializable]
public class Character
{
    public string characterName;
    private const int TILE_SIZE = 16;
    
    // Store textures instead of pre-sliced sprites
    [System.NonSerialized]
    private Texture2D idleTexture;
    [System.NonSerialized]
    private Texture2D idleAnimTexture;
    [System.NonSerialized]
    private Texture2D phoneTexture;
    [System.NonSerialized]
    private Texture2D runTexture;
    
    public Character(string name)
    {
        characterName = name;
        LoadTextures();
    }
    
    public void LoadTextures()
    {
        string path = $"Assets/Scenes/CharacterAnims/{characterName}";
        
        #if UNITY_EDITOR
        idleTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path + "_idle_16x16.png");
        idleAnimTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path + "_idle_anim_16x16.png");
        phoneTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path + "_phone_16x16.png");
        runTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path + "_run_16x16.png");
        
        Debug.Log($"Loaded {characterName} textures:");
        Debug.Log($"  idle: {(idleTexture != null ? "✓" : "✗")}");
        Debug.Log($"  idle_anim: {(idleAnimTexture != null ? "✓" : "✗")}");
        Debug.Log($"  phone: {(phoneTexture != null ? "✓" : "✗")}");
        Debug.Log($"  run: {(runTexture != null ? "✓" : "✗")}");
        #endif
    }
    
    private Texture2D GetTexture(string animation)
    {
        switch (animation)
        {
            case "idle": return idleTexture;
            case "idle_anim": return idleAnimTexture;
            case "phone": return phoneTexture;
            case "run": return runTexture;
            default: return idleAnimTexture;
        }
    }
    
    public int GetFramesPerDirection(string animation)
    {
        switch (animation)
        {
            case "idle": return 1;  // 1 frame per direction (4 total)
            case "idle_anim": return 6;  // 6 frames per direction (24 total)
            case "phone": return 8;  // 8 frames, no directions
            case "run": return 6;  // 6 frames per direction (24 total)
            default: return 6;
        }
    }
    
    public bool HasDirections(string animation)
    {
        return animation != "phone";
    }
    
    /// <summary>
    /// Extract a 16x16 sprite from the texture at the specified position
    /// Matches Python implementation: surface.blit(sheet, (0, 0), (x, y, TILE_SIZE, TILE_SIZE))
    /// </summary>
    public Sprite GetSprite(string animation, string direction, int frame, bool isHat)
    {
        Texture2D texture = GetTexture(animation);
        if (texture == null) return null;
        
        int framesPerDirection = GetFramesPerDirection(animation);
        bool hasDirections = HasDirections(animation);
        
        // Calculate X position (in pixels)
        int x;
        if (hasDirections)
        {
            // Direction offsets (in frames)
            int directionOffset = 0;
            switch (direction)
            {
                case "right": directionOffset = 0; break;
                case "up": directionOffset = framesPerDirection; break;
                case "left": directionOffset = framesPerDirection * 2; break;
                case "down": directionOffset = framesPerDirection * 3; break;
            }
            x = (directionOffset + frame) * TILE_SIZE;
        }
        else
        {
            // No directions, just frames going left to right
            x = frame * TILE_SIZE;
        }
        
        // Calculate Y position: row 0 for hats (y=0), row 1 for bodies (y=16)
        // NOTE: Unity's texture coordinates start from BOTTOM-left, so we need to flip
        int y = isHat ? TILE_SIZE : 0;  // Flipped because Unity uses bottom-left origin
        
        // Extract the 16x16 region
        Color[] pixels = texture.GetPixels(x, y, TILE_SIZE, TILE_SIZE);
        
        // Create a new texture for this sprite
        Texture2D spriteTexture = new Texture2D(TILE_SIZE, TILE_SIZE);
        spriteTexture.SetPixels(pixels);
        spriteTexture.Apply();
        spriteTexture.filterMode = FilterMode.Point; // Pixel-perfect rendering
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(
            spriteTexture,
            new Rect(0, 0, TILE_SIZE, TILE_SIZE),
            new Vector2(0.5f, 0.5f),  // Pivot at center
            TILE_SIZE  // Pixels per unit
        );
        
        return sprite;
    }
}
