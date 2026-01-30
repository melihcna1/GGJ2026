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
        PositionFullyOnScreen(popupRect, targetCanvas.GetComponent<RectTransform>());
        return true;
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
