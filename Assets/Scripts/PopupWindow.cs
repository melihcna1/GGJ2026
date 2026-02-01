using UnityEngine;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image bodyImage;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private Vector2 paddingPixels;

    public static int ActivePopupCount { get; private set; }

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();

        if (closeButton == null)
        {
            var closeTransform = transform.Find("CloseButton");
            if (closeTransform != null)
                closeButton = closeTransform.GetComponent<Button>();
        }

        if (bodyImage == null)
        {
            var bodyTransform = transform.Find("BodyImage");
            if (bodyTransform != null)
                bodyImage = bodyTransform.GetComponent<Image>();
        }

        if (bodyImage != null)
            bodyImage.raycastTarget = false;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (bodyImage != null)
            bodyImage.transform.SetAsFirstSibling();

        if (closeButton != null)
            closeButton.transform.SetAsLastSibling();
    }

    private void OnEnable()
    {
        ActivePopupCount++;
    }

    private void OnDisable()
    {
        ActivePopupCount = Mathf.Max(0, ActivePopupCount - 1);
    }

    private void LateUpdate()
    {
        if (_canvasGroup != null)
        {
            bool canInteract = !GameOverUI.IsActive;
            _canvasGroup.interactable = canInteract;
            _canvasGroup.blocksRaycasts = canInteract;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (bodyImage == null)
            return;

        bodyImage.sprite = sprite;
        bodyImage.enabled = sprite != null;
        bodyImage.preserveAspect = true;

        if (sprite == null)
            return;

        bodyImage.SetNativeSize();

        var canvas = bodyImage.canvas;
        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        scaleFactor = Mathf.Max(0.0001f, scaleFactor);

        var bodyRect = bodyImage.rectTransform;
        EnsureCenteredAnchors(bodyRect);
        bodyRect.anchoredPosition = Vector2.zero;
        var bodySize = bodyRect.sizeDelta;
        bodySize = RoundSizeToScreenPixels(bodySize, scaleFactor);
        bodyRect.sizeDelta = bodySize;

        if (popupRect != null)
        {
            var size = bodySize + paddingPixels;
            size = RoundSizeToScreenPixels(size, scaleFactor);
            popupRect.sizeDelta = size;
        }
    }

    private static void EnsureCenteredAnchors(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static Vector2 RoundSizeToScreenPixels(Vector2 sizeDelta, float scaleFactor)
    {
        var w = Mathf.Round(sizeDelta.x * scaleFactor) / scaleFactor;
        var h = Mathf.Round(sizeDelta.y * scaleFactor) / scaleFactor;
        return new Vector2(w, h);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
