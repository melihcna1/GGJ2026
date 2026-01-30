using System.Collections;
using UnityEngine;

public class PopupSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private PopupWindow popupTemplate;

    [Header("Timing")]
    [SerializeField] private float minSpawnDelaySeconds = 1.5f;
    [SerializeField] private float maxSpawnDelaySeconds = 5f;

    [Header("Avoidance")]
    [SerializeField] private float avoidActiveLassoMarginPixels = 40f;
    [SerializeField] private int maxPlacementAttempts = 20;

    [Header("Content")]
    [SerializeField] private Sprite[] possibleSprites;

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            var delay = Random.Range(minSpawnDelaySeconds, maxSpawnDelaySeconds);
            yield return new WaitForSeconds(delay);

            TrySpawnPopup();
        }
    }

    public bool TrySpawnPopup()
    {
        if (popupTemplate == null)
            return false;

        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null)
            return false;

        var popup = Instantiate(popupTemplate, targetCanvas.transform, false);
        popup.gameObject.SetActive(true);

        if (possibleSprites != null && possibleSprites.Length > 0)
            popup.SetSprite(possibleSprites[Random.Range(0, possibleSprites.Length)]);

        var popupRect = popup.GetComponent<RectTransform>();
        EnsureCenteredAnchors(popupRect);
        if (!TryPlacePopupAvoidingActiveLasso(popupRect, targetCanvas.GetComponent<RectTransform>()))
        {
            Destroy(popup.gameObject);
            return false;
        }
        return true;
    }

    private bool TryPlacePopupAvoidingActiveLasso(RectTransform popupRect, RectTransform canvasRect)
    {
        if (popupRect == null || canvasRect == null)
            return false;

        Canvas.ForceUpdateCanvases();

        Rect? avoidRect = null;
        if (LassoDrawer.TryGetActiveScreenRect(out var lassoRect))
        {
            float m = Mathf.Max(0f, avoidActiveLassoMarginPixels);
            avoidRect = Rect.MinMaxRect(lassoRect.xMin - m, lassoRect.yMin - m, lassoRect.xMax + m, lassoRect.yMax + m);
        }

        var canvasSize = canvasRect.rect.size;
        var popupSize = popupRect.rect.size;

        var halfW = Mathf.Max(0f, (canvasSize.x - popupSize.x) * 0.5f);
        var halfH = Mathf.Max(0f, (canvasSize.y - popupSize.y) * 0.5f);

        int attempts = Mathf.Max(1, maxPlacementAttempts);
        for (int i = 0; i < attempts; i++)
        {
            var x = Random.Range(-halfW, halfW);
            var y = Random.Range(-halfH, halfH);
            popupRect.anchoredPosition = new Vector2(x, y);

            if (avoidRect == null)
                return true;

            var popupScreenRect = GetScreenRect(popupRect);
            if (!popupScreenRect.Overlaps(avoidRect.Value))
                return true;
        }

        return false;
    }

    private static Rect GetScreenRect(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = min;

        for (int i = 1; i < 4; i++)
        {
            var sp = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static void EnsureCenteredAnchors(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void PositionFullyOnScreen(RectTransform popupRect, RectTransform canvasRect)
    {
        if (popupRect == null || canvasRect == null)
            return;

        // Ensure layout is up-to-date so rect sizes are valid.
        Canvas.ForceUpdateCanvases();

        var canvasSize = canvasRect.rect.size;
        var popupSize = popupRect.rect.size;

        // If the popup is bigger than the canvas, clamp size assumptions.
        var halfW = Mathf.Max(0f, (canvasSize.x - popupSize.x) * 0.5f);
        var halfH = Mathf.Max(0f, (canvasSize.y - popupSize.y) * 0.5f);

        var x = Random.Range(-halfW, halfW);
        var y = Random.Range(-halfH, halfH);

        popupRect.anchoredPosition = new Vector2(x, y);
    }
}
