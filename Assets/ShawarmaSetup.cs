using UnityEngine;

/// <summary>
/// Helper script to automatically set up the shawarma GameObject in the scene.
/// Attach this to your shawarma GameObject to configure it properly.
/// </summary>
public class ShawarmaSetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Setting up Shawarma GameObject...");
        
        // Ensure we have a SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            Debug.Log("Added SpriteRenderer to Shawarma");
        }
        
        // Try to load the shawarma sprite
        if (sr.sprite == null)
        {
            Sprite shawarmaSprite = Resources.Load<Sprite>("ShawarmaFinal");
            if (shawarmaSprite == null)
            {
                // Try loading from Assets folder directly
                #if UNITY_EDITOR
                shawarmaSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ShawarmaFinal.png");
                #endif
            }
            
            if (shawarmaSprite != null)
            {
                sr.sprite = shawarmaSprite;
                Debug.Log("Loaded shawarma sprite!");
            }
            else
            {
                Debug.LogWarning("Could not find ShawarmaFinal sprite. Please assign it manually in the Inspector.");
            }
        }
        
        // Set sorting order to be visible
        sr.sortingOrder = 10;
        Debug.Log($"Set sorting order to {sr.sortingOrder}");
        
        // Ensure we have the ShawarmaLogic script
        ShawarmaLogic logic = GetComponent<ShawarmaLogic>();
        if (logic == null)
        {
            logic = gameObject.AddComponent<ShawarmaLogic>();
            Debug.Log("Added ShawarmaLogic script");
        }
        
        // Set a reasonable scale if it's too small
        if (transform.localScale.magnitude < 0.1f)
        {
            transform.localScale = Vector3.one;
            Debug.Log("Reset scale to 1,1,1");
        }
        
        Debug.Log($"Shawarma setup complete! Position: {transform.position}, Scale: {transform.localScale}");
    }
}
