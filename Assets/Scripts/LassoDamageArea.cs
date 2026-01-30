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

    public void Initialize(IReadOnlyList<Vector2> closedLoopPoints, Color fillColor, float delaySeconds)
    {
        if (closedLoopPoints == null || closedLoopPoints.Count < 4)
        {
            if (destroyAfterDamage)
                Destroy(gameObject);
            return;
        }

        var points = new Vector2[closedLoopPoints.Count - 1];
        for (int i = 0; i < points.Length; i++)
            points[i] = closedLoopPoints[i];

        _collider.pathCount = 1;
        _collider.SetPath(0, points);

        BuildMesh(points, fillColor);

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

        for (int i = 0; i < _overlaps.Count; i++)
        {
            var col = _overlaps[i];
            if (col == null)
                continue;

            var enemy = col.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damageAmount);
        }

        if (destroyAfterDamage)
            Destroy(gameObject);
    }

    private void BuildMesh(Vector2[] polygon, Color color)
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = color;

        var mesh = new Mesh();

        var verts = new Vector3[polygon.Length];
        for (int i = 0; i < polygon.Length; i++)
            verts[i] = new Vector3(polygon[i].x, polygon[i].y, 0f);

        var tris = Triangulator.Triangulate(polygon);
        if (tris == null || tris.Length < 3)
        {
            Destroy(mesh);
            return;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        var uv = new Vector2[verts.Length];
        for (int i = 0; i < uv.Length; i++)
            uv[i] = polygon[i];
        mesh.uv = uv;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        _meshFilter.sharedMesh = mesh;
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
