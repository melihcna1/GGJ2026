using UnityEngine;

public class System31Relocator : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float systemMoveTimerMin = 1f;
    [SerializeField] private float systemMoveTimerMax = 3f;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Placement")]
    [SerializeField] private bool keepInsideViewUsingSpriteBounds = true;
    [SerializeField] private float extraMarginWorld = 0f;

    private float _timer;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        ResetTimer();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Relocate();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        float min = Mathf.Max(0f, systemMoveTimerMin);
        float max = Mathf.Max(min, systemMoveTimerMax);
        _timer = Random.Range(min, max);
        if (_timer <= 0f)
            _timer = 0.01f;
    }

    private void Relocate()
    {
        if (targetCamera == null)
            return;

        Vector3 newPos;

        if (targetCamera.orthographic)
            newPos = PickRandomPointInOrthoCamera();
        else
            newPos = PickRandomPointInPerspectiveCamera();

        transform.position = newPos;
    }

    private Vector3 PickRandomPointInOrthoCamera()
    {
        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        Vector3 camPos = targetCamera.transform.position;

        Vector2 extents = Vector2.zero;
        if (keepInsideViewUsingSpriteBounds && _sr != null)
        {
            var b = _sr.bounds;
            extents = new Vector2(b.extents.x, b.extents.y);
        }

        float minX = camPos.x - camWidth * 0.5f + extents.x + extraMarginWorld;
        float maxX = camPos.x + camWidth * 0.5f - extents.x - extraMarginWorld;
        float minY = camPos.y - camHeight * 0.5f + extents.y + extraMarginWorld;
        float maxY = camPos.y + camHeight * 0.5f - extents.y - extraMarginWorld;

        float x = Random.Range(Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        float y = Random.Range(Mathf.Min(minY, maxY), Mathf.Max(minY, maxY));

        return new Vector3(x, y, transform.position.z);
    }

    private Vector3 PickRandomPointInPerspectiveCamera()
    {
        float zDist = Mathf.Max(0.01f, Mathf.Abs(transform.position.z - targetCamera.transform.position.z));

        Vector3 bl = targetCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDist));
        Vector3 tr = targetCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDist));

        Vector2 extents = Vector2.zero;
        if (keepInsideViewUsingSpriteBounds && _sr != null)
        {
            var b = _sr.bounds;
            extents = new Vector2(b.extents.x, b.extents.y);
        }

        float minX = bl.x + extents.x + extraMarginWorld;
        float maxX = tr.x - extents.x - extraMarginWorld;
        float minY = bl.y + extents.y + extraMarginWorld;
        float maxY = tr.y - extents.y - extraMarginWorld;

        float x = Random.Range(Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        float y = Random.Range(Mathf.Min(minY, maxY), Mathf.Max(minY, maxY));

        return new Vector3(x, y, transform.position.z);
    }
}
