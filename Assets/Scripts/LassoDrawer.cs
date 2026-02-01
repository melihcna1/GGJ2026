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

    [Header("Cursor")]
    [SerializeField] private Texture2D lassoCursor;
    [SerializeField] private Vector2 lassoCursorHotspot;
    [SerializeField] private CursorMode lassoCursorMode = CursorMode.Auto;
    [SerializeField] private bool restoreCursorOnEnd = true;
    [SerializeField, Range(0.1f, 5f)] private float cursorScale = 1f; // NEW: Control scale

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

    private static bool IsLassoCursorApplied;

    private static bool HasActiveScreenRect;
    private static Rect ActiveScreenRect;

    // We store the original cursor in case we need to revert or re-scale later
    private Texture2D _runtimeCursorTexture;
    private Vector2 _runtimeHotspot;

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

        // Apply scaling to cursor texture
        ApplyCursorScaling();

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

    private void ApplyCursorScaling()
    {
        if (lassoCursor == null) return;

        // If scale is effectively 1, just use the original
        if (Mathf.Abs(cursorScale - 1f) < 0.001f)
        {
            _runtimeCursorTexture = lassoCursor;
            _runtimeHotspot = lassoCursorHotspot;
            return;
        }

        // Calculate new dimensions
        int newWidth = Mathf.RoundToInt(lassoCursor.width * cursorScale);
        int newHeight = Mathf.RoundToInt(lassoCursor.height * cursorScale);

        // Resize using RenderTexture (works even if Texture is not Read/Write enabled)
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(lassoCursor, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D scaledTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        scaledTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        scaledTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        _runtimeCursorTexture = scaledTex;
        _runtimeHotspot = lassoCursorHotspot * cursorScale;
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
        ApplyLassoCursorIfNeeded();
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
            RestoreCursorIfNeeded();
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
            RestoreCursorIfNeeded();
        }

        lineRenderer.loop = false;
        lineRenderer.positionCount = 0;
        _points.Clear();

        HasActiveScreenRect = false;
    }

    private void ApplyLassoCursorIfNeeded()
    {
        if (_runtimeCursorTexture == null)
            return;

        if (IsLassoCursorApplied)
            return;

        // Use the runtime (potentially scaled) texture and hotspot
        Cursor.SetCursor(_runtimeCursorTexture, _runtimeHotspot, lassoCursorMode);
        IsLassoCursorApplied = true;
    }

    private void RestoreCursorIfNeeded()
    {
        if (!restoreCursorOnEnd)
            return;

        if (ActiveDrawCount > 0)
            return;

        if (!IsLassoCursorApplied)
            return;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        IsLassoCursorApplied = false;
    }

    private void OnDisable()
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            ActiveDrawCount = Mathf.Max(0, ActiveDrawCount - 1);
            RestoreCursorIfNeeded();
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

        var sanitizedPoints = SanitizeClosedLoop(points);
        if (sanitizedPoints == null || sanitizedPoints.Count < 4)
            return;

        float polygonArea = CalculateEffectiveArea(sanitizedPoints);
        float cost = Mathf.Max(0f, polygonArea * ramCostPerWorldUnitArea);

        List<Vector2> spawnPoints = sanitizedPoints;
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
                spawnPoints = ScaleClosedLoop(spawnPoints, scale);
                spawnArea = CalculateEffectiveArea(spawnPoints);
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

        if (VirusRhythmClock.Instance == null)
            return;

        float delaySeconds = VirusRhythmClock.Instance.GetIntervalSeconds(lassoRythm);

        var areaInstance = Instantiate(damageAreaPrefab);
        areaInstance.Initialize(spawnPoints, fillColor, delaySeconds, ram, spawnCost);
    }

    private static List<Vector2> SanitizeClosedLoop(IReadOnlyList<Vector2> closedLoop)
    {
        if (closedLoop == null || closedLoop.Count < 4)
            return null;

        int count = closedLoop.Count;
        int uniqueCount = count;
        if (closedLoop[0] == closedLoop[count - 1])
            uniqueCount = count - 1;

        if (uniqueCount < 3)
            return null;

        var unique = new List<Vector2>(uniqueCount);
        for (int i = 0; i < uniqueCount; i++)
            unique.Add(closedLoop[i]);

        if (unique.Count < 3)
            return null;

        var loop = new List<Vector2>(unique.Count + 1);
        loop.AddRange(unique);
        loop.Add(loop[0]);
        return loop;
    }

    private static float CalculateEffectiveArea(IReadOnlyList<Vector2> closedLoop)
    {
        if (closedLoop == null || closedLoop.Count < 4)
            return 0f;

        int count = closedLoop.Count;
        int uniqueCount = count;
        if (closedLoop[0] == closedLoop[count - 1])
            uniqueCount = count - 1;

        if (uniqueCount < 3)
            return 0f;

        var unique = new List<Vector2>(uniqueCount);
        for (int i = 0; i < uniqueCount; i++)
            unique.Add(closedLoop[i]);

        if (!IsSelfIntersecting(unique))
            return CalculatePolygonArea(closedLoop);

        if (!TryFindFirstIntersection(unique, out var iEdge, out var jEdge, out var intersection))
            return CalculatePolygonArea(closedLoop);

        var a = BuildLoopFromCut(unique, iEdge, jEdge, intersection);
        var b = BuildLoopFromCut(unique, jEdge, iEdge, intersection);

        float areaA = Mathf.Abs(SignedArea(a));
        float areaB = Mathf.Abs(SignedArea(b));
        return areaA + areaB;
    }

    private static bool TryFindFirstIntersection(IReadOnlyList<Vector2> polygon, out int iEdge, out int jEdge, out Vector2 point)
    {
        iEdge = -1;
        jEdge = -1;
        point = default;

        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            var a1 = polygon[i];
            var a2 = polygon[(i + 1) % n];

            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1)
                    continue;

                var b1 = polygon[j];
                var b2 = polygon[(j + 1) % n];

                if (TrySegmentIntersectionPoint(a1, a2, b1, b2, out var ip))
                {
                    iEdge = i;
                    jEdge = j;
                    point = ip;
                    return true;
                }
            }
        }

        return false;
    }

    private static Vector2[] BuildLoopFromCut(IReadOnlyList<Vector2> polygon, int startEdge, int endEdge, Vector2 intersection)
    {
        int n = polygon.Count;

        var loop = new List<Vector2>(n + 2);
        loop.Add(intersection);

        int v = startEdge + 1;
        while (true)
        {
            if (v >= n)
                v -= n;

            if (v == endEdge + 1)
                break;

            loop.Add(polygon[v]);
            v++;
        }

        loop.Add(intersection);

        if (loop.Count >= 2 && loop[0] == loop[loop.Count - 1])
            loop.RemoveAt(loop.Count - 1);

        return loop.ToArray();
    }

    private static float SignedArea(Vector2[] points)
    {
        if (points == null || points.Length < 3)
            return 0f;

        float a = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            var q = points[(i + 1) % points.Length];
            a += p.x * q.y - q.x * p.y;
        }
        return a * 0.5f;
    }

    private static bool TrySegmentIntersectionPoint(Vector2 p, Vector2 p2, Vector2 q, Vector2 q2, out Vector2 intersection)
    {
        intersection = default;

        var r = p2 - p;
        var s = q2 - q;
        float rxs = Cross(r, s);
        float qpxr = Cross(q - p, r);

        if (Mathf.Approximately(rxs, 0f) && Mathf.Approximately(qpxr, 0f))
            return false;

        if (Mathf.Approximately(rxs, 0f) && !Mathf.Approximately(qpxr, 0f))
            return false;

        float t = Cross(q - p, s) / rxs;
        float u = Cross(q - p, r) / rxs;

        if (t > 0f && t < 1f && u > 0f && u < 1f)
        {
            intersection = p + t * r;
            return true;
        }

        return false;
    }

    private static bool IsSelfIntersecting(IReadOnlyList<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 4)
            return false;

        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            var a1 = polygon[i];
            var a2 = polygon[(i + 1) % n];

            for (int j = i + 1; j < n; j++)
            {
                if (j == i)
                    continue;

                int i2 = (i + 1) % n;
                int j2 = (j + 1) % n;

                if (i == j || i2 == j || j2 == i)
                    continue;

                if (i == 0 && j2 == 0)
                    continue;

                var b1 = polygon[j];
                var b2 = polygon[j2];

                if (SegmentsIntersect(a1, a2, b1, b2))
                    return true;
            }
        }

        return false;
    }

    private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float o1 = Cross(p2 - p1, q1 - p1);
        float o2 = Cross(p2 - p1, q2 - p1);
        float o3 = Cross(q2 - q1, p1 - q1);
        float o4 = Cross(q2 - q1, p2 - q1);

        if ((o1 > 0f && o2 < 0f || o1 < 0f && o2 > 0f) && (o3 > 0f && o4 < 0f || o3 < 0f && o4 > 0f))
            return true;

        if (Mathf.Approximately(o1, 0f) && OnSegment(p1, p2, q1))
            return true;
        if (Mathf.Approximately(o2, 0f) && OnSegment(p1, p2, q2))
            return true;
        if (Mathf.Approximately(o3, 0f) && OnSegment(q1, q2, p1))
            return true;
        if (Mathf.Approximately(o4, 0f) && OnSegment(q1, q2, p2))
            return true;

        return false;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static bool OnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        return p.x >= Mathf.Min(a.x, b.x) - 0.0001f &&
               p.x <= Mathf.Max(a.x, b.x) + 0.0001f &&
               p.y >= Mathf.Min(a.y, b.y) - 0.0001f &&
               p.y <= Mathf.Max(a.y, b.y) + 0.0001f;
    }

    private static List<Vector2> BuildConvexHull(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return null;

        var pts = new List<Vector2>(points);
        pts.Sort((a, b) =>
        {
            int cx = a.x.CompareTo(b.x);
            return cx != 0 ? cx : a.y.CompareTo(b.y);
        });

        var hull = new List<Vector2>(pts.Count);

        for (int i = 0; i < pts.Count; i++)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 1] - hull[hull.Count - 2], pts[i] - hull[hull.Count - 1]) <= 0f)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(pts[i]);
        }

        int lowerCount = hull.Count;
        for (int i = pts.Count - 2; i >= 0; i--)
        {
            while (hull.Count > lowerCount && Cross(hull[hull.Count - 1] - hull[hull.Count - 2], pts[i] - hull[hull.Count - 1]) <= 0f)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(pts[i]);
        }

        if (hull.Count > 1)
            hull.RemoveAt(hull.Count - 1);

        return hull;
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