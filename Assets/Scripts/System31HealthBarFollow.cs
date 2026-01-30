using UnityEngine;

public class System31HealthBarFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera uiCamera;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (uiCamera == null && canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        if (uiCamera == null)
            uiCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || canvas == null || _rect == null)
            return;

        var worldPos = target.position + worldOffset;

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = worldPos;
            transform.rotation = Quaternion.identity;
            return;
        }

        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

        var canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out var localPoint))
            _rect.anchoredPosition = localPoint;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
