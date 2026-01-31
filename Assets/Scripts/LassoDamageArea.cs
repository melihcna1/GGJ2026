using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class LassoDamageArea : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damageAmount = 3;

    [Header("Behavior")]
    [SerializeField] private bool destroyAfterDamage = true;

    private PolygonCollider2D _collider;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private RamResource _ram;
    private float _spentRam;

    private readonly List<Collider2D> _overlaps = new List<Collider2D>(64);

    private void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _collider.isTrigger = true;

        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
            _meshFilter = gameObject.AddComponent<MeshFilter>();

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (_meshRenderer.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _meshRenderer.sharedMaterial = new Material(shader);
        }
        if (_meshRenderer.sharedMaterial != null)
            _meshRenderer.material = new Material(_meshRenderer.sharedMaterial);
    }

    private int GetDamageToApply()
    {
        return Mathf.Max(0, damageAmount);
    }

    public void Initialize(IReadOnlyList<Vector2> closedLoopPoints, Color fillColor, float delaySeconds)
    {
        Initialize(closedLoopPoints, fillColor, delaySeconds, null, 0f);
    }

    public void Initialize(IReadOnlyList<Vector2> closedLoopPoints, Color fillColor, float delaySeconds, RamResource ram, float spentRam)
    {
        _ram = ram;
        _spentRam = Mathf.Max(0f, spentRam);

        if (closedLoopPoints == null || closedLoopPoints.Count < 4)
        {
            if (destroyAfterDamage)
                Destroy(gameObject);
            return;
        }

        var loop = new Vector2[closedLoopPoints.Count - 1];
        for (int i = 0; i < loop.Length; i++)
            loop[i] = closedLoopPoints[i];

        var loops = SplitSelfIntersectingLoop(loop);
        if (loops == null || loops.Count == 0)
        {
            if (destroyAfterDamage)
                Destroy(gameObject);
            return;
        }

        _collider.pathCount = loops.Count;
        for (int i = 0; i < loops.Count; i++)
            _collider.SetPath(i, loops[i]);

        BuildMesh(loops, fillColor);

        StartCoroutine(DamageAfterDelay(delaySeconds));
    }

    private IEnumerator DamageAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        _overlaps.Clear();
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        _collider.Overlap(filter, _overlaps);

        int damage = GetDamageToApply();

        for (int i = 0; i < _overlaps.Count; i++)
        {
            var col = _overlaps[i];
            if (col == null)
                continue;

            var zipBomb = col.GetComponentInParent<ZipBomb>();
            if (zipBomb != null)
            {
                zipBomb.Explode();
                continue;
            }

            var enemyHealth = col.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                continue;
            }

            var goodVirus = col.GetComponentInParent<GoodVirus>();
            if (goodVirus != null)
            {
                goodVirus.TakeDamage(damage);
                continue;
            }

            var enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        if (destroyAfterDamage)
        {
            Destroy(gameObject);
        }
    }

    private void BuildMesh(List<Vector2[]> polygons, Color color)
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = color;

        if (polygons == null || polygons.Count == 0)
            return;

        var allVerts = new List<Vector3>(256);
        var allUV = new List<Vector2>(256);
        var allTris = new List<int>(512);

        for (int p = 0; p < polygons.Count; p++)
        {
            var poly = polygons[p];
            if (poly == null || poly.Length < 3)
                continue;

            int vertOffset = allVerts.Count;
            for (int i = 0; i < poly.Length; i++)
            {
                allVerts.Add(new Vector3(poly[i].x, poly[i].y, 0f));
                allUV.Add(poly[i]);
            }

            var tris = Triangulator.Triangulate(poly);
            if (tris == null || tris.Length < 3)
                continue;

            for (int i = 0; i < tris.Length; i++)
                allTris.Add(vertOffset + tris[i]);
        }

        if (allVerts.Count < 3 || allTris.Count < 3)
            return;

        var mesh = new Mesh();
        mesh.SetVertices(allVerts);
        mesh.SetTriangles(allTris, 0);
        mesh.SetUVs(0, allUV);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        _meshFilter.sharedMesh = mesh;
    }

    private static List<Vector2[]> SplitSelfIntersectingLoop(Vector2[] loop)
    {
        if (loop == null || loop.Length < 3)
            return null;

        var pts = new List<Vector2>(loop.Length + 1);
        pts.AddRange(loop);
        pts.Add(loop[0]);

        var cut = TryFindFirstIntersection(pts);
        if (!cut.hasIntersection)
        {
            return new List<Vector2[]> { loop };
        }

        var aLoop = BuildLoopFromCut(pts, cut.i, cut.j, cut.point);
        var bLoop = BuildLoopFromCut(pts, cut.j, cut.i, cut.point);

        var result = new List<Vector2[]>(2);
        if (IsValidSimplePolygon(aLoop))
            result.Add(aLoop);
        if (IsValidSimplePolygon(bLoop))
            result.Add(bLoop);

        if (result.Count == 0)
            return null;

        return result;
    }

    private static (bool hasIntersection, int i, int j, Vector2 point) TryFindFirstIntersection(List<Vector2> closed)
    {
        int n = closed.Count - 1;
        for (int i = 0; i < n; i++)
        {
            var a1 = closed[i];
            var a2 = closed[i + 1];

            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1)
                    continue;

                var b1 = closed[j];
                var b2 = closed[j + 1];

                if (TrySegmentIntersectionPoint(a1, a2, b1, b2, out var ip))
                    return (true, i, j, ip);
            }
        }

        return (false, -1, -1, default);
    }

    private static Vector2[] BuildLoopFromCut(List<Vector2> closed, int startEdge, int endEdge, Vector2 intersection)
    {
        int n = closed.Count - 1;

        var poly = new List<Vector2>(n + 2);
        poly.Add(intersection);

        int v = startEdge + 1;
        while (true)
        {
            if (v >= n)
                v -= n;

            if (v == endEdge + 1)
                break;

            poly.Add(closed[v]);
            v++;
        }

        poly.Add(intersection);

        if (poly.Count >= 2 && poly[0] == poly[poly.Count - 1])
            poly.RemoveAt(poly.Count - 1);

        return poly.ToArray();
    }

    private static bool IsValidSimplePolygon(Vector2[] poly)
    {
        if (poly == null || poly.Length < 3)
            return false;

        float area = Mathf.Abs(SignedArea(poly));
        if (area < 0.0001f)
            return false;

        if (IsSelfIntersecting(poly))
            return false;

        return true;
    }

    private static float SignedArea(Vector2[] points)
    {
        float a = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            var q = points[(i + 1) % points.Length];
            a += p.x * q.y - q.x * p.y;
        }
        return a * 0.5f;
    }

    private static bool IsSelfIntersecting(Vector2[] polygon)
    {
        int n = polygon.Length;
        if (n < 4)
            return false;

        for (int i = 0; i < n; i++)
        {
            var a1 = polygon[i];
            var a2 = polygon[(i + 1) % n];

            for (int j = i + 1; j < n; j++)
            {
                int i2 = (i + 1) % n;
                int j2 = (j + 1) % n;

                if (i == j || i2 == j || j2 == i)
                    continue;

                if (i == 0 && j2 == 0)
                    continue;

                var b1 = polygon[j];
                var b2 = polygon[j2];

                if (TrySegmentIntersectionPoint(a1, a2, b1, b2, out _))
                    return true;
            }
        }

        return false;
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

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static class Triangulator
    {
        public static int[] Triangulate(IReadOnlyList<Vector2> points)
        {
            int n = points.Count;
            if (n < 3)
                return null;

            var indices = new int[n];
            if (Area(points) > 0f)
            {
                for (int v = 0; v < n; v++)
                    indices[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    indices[v] = (n - 1) - v;
            }

            var tris = new List<int>((n - 2) * 3);

            int nv = n;
            int count = 2 * nv;

            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return null;

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(points, u, v, w, nv, indices))
                {
                    int a = indices[u];
                    int b = indices[v];
                    int c = indices[w];

                    tris.Add(a);
                    tris.Add(b);
                    tris.Add(c);

                    for (int s = v, t = v + 1; t < nv; s++, t++)
                        indices[s] = indices[t];

                    nv--;
                    count = 2 * nv;
                }
            }

            return tris.ToArray();
        }

        private static float Area(IReadOnlyList<Vector2> points)
        {
            int n = points.Count;
            float a = 0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                var pval = points[p];
                var qval = points[q];
                a += pval.x * qval.y - qval.x * pval.y;
            }
            return a * 0.5f;
        }

        private static bool Snip(IReadOnlyList<Vector2> points, int u, int v, int w, int n, int[] indices)
        {
            var a = points[indices[u]];
            var b = points[indices[v]];
            var c = points[indices[w]];

            if (Mathf.Epsilon > (((b.x - a.x) * (c.y - a.y)) - ((b.y - a.y) * (c.x - a.x))))
                return false;

            for (int p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                    continue;

                var pt = points[indices[p]];
                if (InsideTriangle(a, b, c, pt))
                    return false;
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            var ab = b - a;
            var bc = c - b;
            var ca = a - c;

            var ap = p - a;
            var bp = p - b;
            var cp = p - c;

            float c1 = ab.x * ap.y - ab.y * ap.x;
            float c2 = bc.x * bp.y - bc.y * bp.x;
            float c3 = ca.x * cp.y - ca.y * cp.x;

            bool hasNeg = (c1 < 0f) || (c2 < 0f) || (c3 < 0f);
            bool hasPos = (c1 > 0f) || (c2 > 0f) || (c3 > 0f);
            return !(hasNeg && hasPos);
        }
    }
}
