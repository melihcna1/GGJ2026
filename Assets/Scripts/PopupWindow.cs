using UnityEngine;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image bodyImage;

    public static int ActivePopupCount { get; private set; }

    private void Awake()
    {
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
    }

    private void OnEnable()
    {
        ActivePopupCount++;
    }

    private void OnDisable()
    {
        ActivePopupCount = Mathf.Max(0, ActivePopupCount - 1);
    }

    public void SetSprite(Sprite sprite)
    {
        if (bodyImage == null)
            return;

        bodyImage.sprite = sprite;
        bodyImage.enabled = sprite != null;
        bodyImage.preserveAspect = true;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
