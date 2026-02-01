using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes an image go BRR with RGB hue shifting.
/// Attach to a GameObject with an Image, SpriteRenderer, or both.
/// </summary>
public class RGBHueShift : MonoBehaviour
{
    [Header("Hue Shift Settings")]
    [Tooltip("Speed of the hue shift. Higher = faster rainbow cycling.")]
    [Range(0.01f, 10f)]
    public float speed = 1f;
    
    [Tooltip("Saturation of the color (0 = grayscale, 1 = full color)")]
    [Range(0f, 1f)]
    public float saturation = 1f;
    
    [Tooltip("Brightness/Value of the color")]
    [Range(0f, 1f)]
    public float brightness = 1f;
    
    [Tooltip("Alpha/Opacity of the color")]
    [Range(0f, 1f)]
    public float alpha = 1f;

    // Shared hue value - all instances use the same hue for synchronization
    private static float sharedHue = 0f;
    private static float lastUpdateTime = -1f;

    private Image uiImage;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Try to get Image component (for UI)
        uiImage = GetComponent<Image>();
        
        // Try to get SpriteRenderer component (for 2D sprites)
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (uiImage == null && spriteRenderer == null)
        {
            Debug.LogWarning($"RGBHueShift on '{gameObject.name}': No Image or SpriteRenderer found!");
        }
    }

    private void Update()
    {
        // Only update the shared hue once per frame (first instance to update does it)
        if (lastUpdateTime != Time.time)
        {
            lastUpdateTime = Time.time;
            sharedHue += speed * Time.deltaTime;
            if (sharedHue > 1f)
            {
                sharedHue -= 1f;
            }
        }

        // Convert HSV to RGB using the shared hue
        Color newColor = Color.HSVToRGB(sharedHue, saturation, brightness);
        newColor.a = alpha;

        // Apply to whichever component exists
        if (uiImage != null)
        {
            uiImage.color = newColor;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
    
    /// <summary>
    /// Set the hue shift speed at runtime.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Clamp(newSpeed, 0.01f, 10f);
    }
    
    /// <summary>
    /// Get the current shared hue value (0-1).
    /// </summary>
    public static float GetCurrentHue()
    {
        return sharedHue;
    }
    
    /// <summary>
    /// Reset the shared hue to a specific value.
    /// </summary>
    public static void ResetHue(float hue = 0f)
    {
        sharedHue = hue % 1f;
    }
}
