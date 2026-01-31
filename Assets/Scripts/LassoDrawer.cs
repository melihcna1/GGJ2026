using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LassoDrawer : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera targetCamera;

    [Header("Drawing")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float minPointDistance = 0.1f;

    [Header("Completion Tolerance")]
    [SerializeField] private bool requireCloseToComplete = true;
    [SerializeField] private float closeDistance = 0.5f;
    [SerializeField] private int minPoints = 6;

    [Header("Area")]
    [SerializeField] private LassoDamageArea damageAreaPrefab;
    [SerializeField] private float areaSpawnDelaySeconds = 1f;
    [SerializeField] private float lassoRythm = 1f;
    [SerializeField] private Color fillColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("RAM")]
    [SerializeField] private RamResource ram;
    [SerializeField] private float ramCostPerWorldUnitArea = 1f;

    [Header("Visual Cleanup")]
    [SerializeField] private float clearLineAfterSeconds = 1f;

    private readonly List<Vector2> _points = new List<Vector2>(256);
    private bool _isDrawing;

    private static int ActiveDrawCount;
    public static bool IsAnyDrawing => ActiveDrawCount > 0;

    private static bool HasActiveScreenRect;
    private static Rect ActiveScreenRect;

    public static bool TryGetActiveScreenRect(out Rect rect)
    {
        rect = ActiveScreenRect;
        return HasActiveScreenRect;
    }

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (clearLineAfterSeconds <= 0f)
            clearLineAfterSeconds = areaSpawnDelaySeconds;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            if (lineRenderer.sharedMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    lineRenderer.sharedMaterial = new Material(shader);
            }

            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }

    private void Update()
    {
        if (targetCamera == null || lineRenderer == null)
            return;

        if (IsPointerPressedThisFrame() && !IsPointerOverUI())
            Begin();

        if (_isDrawing && IsPointerHeld())
        {
            if (TryGetPointerWorldPosition(out var worldPos))
                AddPoint(worldPos);
        }

        if (_isDrawing && IsPointerReleasedThisFrame())
        {
            if (TryGetPointerWorldPosition(out var releaseWorldPos))
                AddPoint(releaseWorldPos, force: true);

            End();
        }
    }

    private void Begin()
    {
        if (ram != null && ram.CurrentRam <= 0f)
            return;

        if (_isDrawing)
            return;

        _isDrawing = true;
        ActiveDrawCount++;
        _points.Clear();
        lineRenderer.positionCount = 0;
        lineRenderer.loop = false;

        if (TryGetPointerWorldPosition(out var worldPos))
            AddPoint(worldPos, force: true);
    }

    private void End()
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            ActiveDrawCount = Mathf.Max(0, ActiveDrawCount - 1);
        }

        HasActiveScreenRect = false;

        if (_points.Count < minPoints)
        {
            ClearLine();
            return;
        }

        var closeIndex = GetClosePointIndex(_points, closeDistance);
        if (closeIndex < 0)
            closeIndex = 0;

        if (closeIndex > 0)
        {
            var rotated = new List<Vector2>(_points.Count);
            for (int i = closeIndex; i < _points.Count; i++)
                rotated.Add(_points[i]);
            for (int i = 0; i < closeIndex; i++)
                rotated.Add(_points[i]);
            _points.Clear();
            _points.AddRange(rotated);

            lineRenderer.positionCount = _points.Count;
            for (int i = 0; i < _points.Count; i++)
                lineRenderer.SetPosition(i, new Vector3(_points[i].x, _points[i].y, 0f));
        }

        var first = _points[0];
        _points[_points.Count - 1] = first;

        lineRenderer.loop = true;

        SpawnArea(_points);

        if (clearLineAfterSeconds > 0f)
            StartCoroutine(ClearLineAfterDelay(clearLineAfterSeconds));
    }

    private IEnumerator ClearLineAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearLine();
    }

    private void ClearLine()
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            ActiveDrawCount = Mathf.Max(0, ActiveDrawCount - 1);
        }

        lineRenderer.loop = false;
        lineRenderer.positionCount = 0;
        _points.Clear();

        HasActiveScreenRect = false;
    }

    private void OnDisable()
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            ActiveDrawCount = Mathf.Max(0, ActiveDrawCount - 1);
        }

        HasActiveScreenRect = false;
    }

    private static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (!TryGetPointerScreenPosition(out var screenPos))
            return false;

        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;

        var results = new List<RaycastResult>(16);
        EventSystem.current.RaycastAll(eventData, results);
        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (go == null)
                continue;

            if (go.GetComponentInParent<Selectable>() != null)
                return true;

            if (ExecuteEvents.GetEventHandler<IPointerClickHandler>(go) != null)
                return true;
            if (ExecuteEvents.GetEventHandler<IPointerDownHandler>(go) != null)
                return true;
            if (ExecuteEvents.GetEventHandler<IPointerUpHandler>(go) != null)
                return true;
            if (ExecuteEvents.GetEventHandler<IDragHandler>(go) != null)
                return true;
        }

        return false;
    }

    private void AddPoint(Vector2 worldPos, bool force = false)
    {
        if (!force && _points.Count > 0)
        {
            if (Vector2.Distance(_points[_points.Count - 1], worldPos) < minPointDistance)
                return;
        }

        _points.Add(worldPos);
        lineRenderer.positionCount = _points.Count;
        lineRenderer.SetPosition(_points.Count - 1, new Vector3(worldPos.x, worldPos.y, 0f));

        UpdateActiveScreenRect();
    }

    private void UpdateActiveScreenRect()
    {
        if (!_isDrawing || targetCamera == null || _points.Count == 0)
        {
            HasActiveScreenRect = false;
            return;
        }

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < _points.Count; i++)
        {
            var sp = targetCamera.WorldToScreenPoint(new Vector3(_points[i].x, _points[i].y, 0f));
            minX = Mathf.Min(minX, sp.x);
            minY = Mathf.Min(minY, sp.y);
            maxX = Mathf.Max(maxX, sp.x);
            maxY = Mathf.Max(maxY, sp.y);
        }

        if (float.IsInfinity(minX) || float.IsInfinity(minY))
        {
            HasActiveScreenRect = false;
            return;
        }

        ActiveScreenRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        HasActiveScreenRect = ActiveScreenRect.width > 0.01f || ActiveScreenRect.height > 0.01f;
    }

    private bool TryGetPointerWorldPosition(out Vector2 worldPos)
    {
        worldPos = default;
        if (targetCamera == null)
            return false;

        if (!TryGetPointerScreenPosition(out var screenPos))
            return false;

        var wp = targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos = new Vector2(wp.x, wp.y);
        return true;
    }

    private static bool TryGetPointerScreenPosition(out Vector2 screenPos)
    {
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        screenPos = default;
        return false;
    }

    private static bool IsPointerPressedThisFrame()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        return false;
    }

    private static bool IsPointerHeld()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;

        return false;
    }

    private static bool IsPointerReleasedThisFrame()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        return false;
    }

    private void SpawnArea(List<Vector2> points)
    {
        if (damageAreaPrefab == null)
            return;

        float polygonArea = CalculatePolygonArea(points);
        float cost = Mathf.Max(0f, polygonArea * ramCostPerWorldUnitArea);

        List<Vector2> spawnPoints = points;
        float spawnArea = polygonArea;
        float spawnCost = cost;
        bool wasClamped = false;

        if (ram != null && ramCostPerWorldUnitArea > 0f)
        {
            float availableRam = Mathf.Max(0f, ram.CurrentRam);
            float maxAllowedArea = availableRam / ramCostPerWorldUnitArea;
            if (maxAllowedArea <= 0f)
                return;

            if (polygonArea > maxAllowedArea)
            {
                float scale = Mathf.Sqrt(maxAllowedArea / Mathf.Max(0.0001f, polygonArea));
                spawnPoints = ScaleClosedLoop(points, scale);
                spawnArea = CalculatePolygonArea(spawnPoints);
                spawnCost = Mathf.Max(0f, spawnArea * ramCostPerWorldUnitArea);
                wasClamped = true;
            }

            if (!ram.TrySpend(spawnCost))
                return;
        }

        if (wasClamped && lineRenderer != null)
        {
            lineRenderer.positionCount = spawnPoints.Count;
            for (int i = 0; i < spawnPoints.Count; i++)
                lineRenderer.SetPosition(i, new Vector3(spawnPoints[i].x, spawnPoints[i].y, 0f));
            lineRenderer.loop = true;
        }

        var areaInstance = Instantiate(damageAreaPrefab);

        if (VirusRhythmClock.Instance == null)
            return;

        float delaySeconds = VirusRhythmClock.Instance.GetIntervalSeconds(lassoRythm);

        areaInstance.Initialize(spawnPoints, fillColor, delaySeconds, ram, spawnCost);
    }

    private static float CalculatePolygonArea(IReadOnlyList<Vector2> closedLoop)
    {
        if (closedLoop == null || closedLoop.Count < 4)
            return 0f;

        float sum = 0f;
        for (int i = 0; i < closedLoop.Count - 1; i++)
        {
            var a = closedLoop[i];
            var b = closedLoop[i + 1];
            sum += (a.x * b.y) - (b.x * a.y);
        }

        return Mathf.Abs(sum) * 0.5f;
    }

    private static List<Vector2> ScaleClosedLoop(IReadOnlyList<Vector2> closedLoop, float scale)
    {
        if (closedLoop == null)
            return null;

        int count = closedLoop.Count;
        if (count < 2)
            return new List<Vector2>(closedLoop);

        int uniqueCount = count;
        if (count >= 2 && closedLoop[0] == closedLoop[count - 1])
            uniqueCount = count - 1;

        if (uniqueCount <= 0)
            return new List<Vector2>(closedLoop);

        var center = GetAverageCenter(closedLoop, uniqueCount);

        var scaled = new List<Vector2>(count);
        for (int i = 0; i < uniqueCount; i++)
            scaled.Add(center + (closedLoop[i] - center) * scale);

        if (count >= 2 && closedLoop[0] == closedLoop[count - 1])
            scaled.Add(scaled[0]);
        else
        {
            for (int i = uniqueCount; i < count; i++)
                scaled.Add(center + (closedLoop[i] - center) * scale);
        }

        return scaled;
    }

    private static Vector2 GetAverageCenter(IReadOnlyList<Vector2> points, int count)
    {
        if (points == null || count <= 0)
            return Vector2.zero;

        Vector2 sum = Vector2.zero;
        for (int i = 0; i < count; i++)
            sum += points[i];
        return sum / count;
    }

    private static int GetClosePointIndex(List<Vector2> points, float tolerance)
    {
        if (points == null || points.Count < 2)
            return -1;

        var last = points[points.Count - 1];
        int bestIndex = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var d = Vector2.Distance(points[i], last);
            if (d <= tolerance && d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}
