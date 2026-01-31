using UnityEngine;
using UnityEngine.UI;

public class RamUIBar : MonoBehaviour
{
    [SerializeField] private RamResource ram;
    [SerializeField] private Image fillImage;

    private float fullWidth;

    private void Awake()
    {
        CacheFullWidth();
    }

    private void OnValidate()
    {
        CacheFullWidth();
    }

    private void CacheFullWidth()
    {
        if (fillImage == null)
            return;

        var rt = fillImage.rectTransform;
        if (rt == null)
            return;

        fullWidth = rt.rect.width;
    }

    private void Update()
    {
        if (ram == null || fillImage == null)
            return;

        var rt = fillImage.rectTransform;
        if (rt == null)
            return;

        if (fullWidth <= 0f)
            fullWidth = rt.rect.width;

        var pivot = rt.pivot;
        if (!Mathf.Approximately(pivot.x, 0f))
        {
            var anchored = rt.anchoredPosition;
            rt.pivot = new Vector2(0f, pivot.y);
            rt.anchoredPosition = anchored;
        }

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth * Mathf.Clamp01(ram.Normalized));
    }
}
