using UnityEngine;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image bodyImage;

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

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
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
