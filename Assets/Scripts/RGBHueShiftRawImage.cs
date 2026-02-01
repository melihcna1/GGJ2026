using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes a RawImage go BRR with RGB hue shifting.
/// Shares the same hue as RGBHueShift so all images stay synchronized.
/// </summary>
public class RGBHueShiftRawImage : MonoBehaviour
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

    private RawImage rawImage;
    private float currentHue;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        
        if (rawImage == null)
        {
            Debug.LogWarning($"RGBHueShiftRawImage on '{gameObject.name}': No RawImage found!");
        }
        
        // Start at a random hue so each RawImage looks different/async
        currentHue = Random.Range(0f, 1f);
    }

    private void Update()
    {
        // Each instance updates its own hue independently
        currentHue += speed * Time.deltaTime;
        if (currentHue > 1f)
        {
            currentHue -= 1f;
        }

        // Convert HSV to RGB
        Color newColor = Color.HSVToRGB(currentHue, saturation, brightness);
        newColor.a = alpha;

        if (rawImage != null)
        {
            rawImage.color = newColor;
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
    /// Get this instance's current hue value (0-1).
    /// </summary>
    public float GetCurrentHue()
    {
        return currentHue;
    }
    
    /// <summary>
    /// Set this instance's hue to a specific value.
    /// </summary>
    public void SetHue(float hue)
    {
        currentHue = hue % 1f;
    }
    
    /// <summary>
    /// Randomize this instance's hue.
    /// </summary>
    public void RandomizeHue()
    {
        currentHue = Random.Range(0f, 1f);
    }
}
