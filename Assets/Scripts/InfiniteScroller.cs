using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Perfect seamless scrolling using UV offset on a RawImage.
/// 
/// SETUP:
/// 1. Delete all your individual banana images
/// 2. Create a RawImage (UI > Raw Image)
/// 3. Assign your banana texture to the RawImage
/// 4. Set UV Rect W and H to tile the texture (e.g., W=5, H=5 for 5x5 tiles)
/// 5. Add this script to the RawImage
/// 6. Press Play
/// </summary>
public class InfiniteScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("How fast to scroll the texture")]
    public Vector2 scrollSpeed = new Vector2(-0.1f, -0.1f);

    private RawImage rawImage;
    private Rect uvRect;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        
        if (rawImage == null)
        {
            Debug.LogError("InfiniteScroller: Attach this to a RawImage component!");
            enabled = false;
            return;
        }

        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        // Scroll the UV offset
        uvRect.x += scrollSpeed.x * Time.deltaTime;
        uvRect.y += scrollSpeed.y * Time.deltaTime;
        
        // Keep values in reasonable range to avoid floating point issues over time
        uvRect.x = uvRect.x % 1f;
        uvRect.y = uvRect.y % 1f;
        
        rawImage.uvRect = uvRect;
    }
}
