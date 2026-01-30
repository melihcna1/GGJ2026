using UnityEngine;
using UnityEngine.UI;

public class GoodVirusProgressUI : MonoBehaviour
{
    [SerializeField] private Text progressText;
    [SerializeField] private bool createIfMissing = true;

    private void Awake()
    {
        if (progressText == null && createIfMissing)
            progressText = FindOrCreateText();
    }

    private void OnEnable()
    {
        EnsureManagerExists();

        GoodVirusProgress.Instance.ProgressChanged += OnProgressChanged;
        GoodVirusProgress.Instance.ProgressCompleted += OnProgressCompleted;

        OnProgressChanged(GoodVirusProgress.Instance.CurrentCount, GoodVirusProgress.Instance.TargetCount);
    }

    private void OnDisable()
    {
        if (GoodVirusProgress.Instance == null)
            return;

        GoodVirusProgress.Instance.ProgressChanged -= OnProgressChanged;
        GoodVirusProgress.Instance.ProgressCompleted -= OnProgressCompleted;
    }

    private void EnsureManagerExists()
    {
        if (GoodVirusProgress.Instance != null)
            return;

        var mgr = new GameObject("GoodVirusProgress");
        mgr.AddComponent<GoodVirusProgress>();
    }

    private void OnProgressChanged(int current, int target)
    {
        if (progressText == null)
            return;

        progressText.text = current + "/" + target;
    }

    private void OnProgressCompleted()
    {
        if (progressText != null)
            progressText.text = "10/10 - YOU WIN";

        Time.timeScale = 0f;
    }

    private Text FindOrCreateText()
    {
        var existing = FindFirstObjectByType<Text>();
        if (existing != null)
            return existing;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("GoodVirusProgressCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        var textGo = new GameObject("GoodVirusProgressText");
        textGo.transform.SetParent(canvas.transform, false);

        var t = textGo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 28;
        t.alignment = TextAnchor.UpperLeft;
        t.color = Color.white;
        t.text = "0/10";

        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(16f, -16f);
        rt.sizeDelta = new Vector2(260f, 60f);

        return t;
    }
}
