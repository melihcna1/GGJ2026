using UnityEngine;

public class TrailOnMove : MonoBehaviour
{
    [SerializeField] private float minSpeedToEmit = 0.05f;

    [Header("Trail")]
    [SerializeField] private float trailTime = 0.2f;
    [SerializeField] private float startWidth = 0.25f;
    [SerializeField] private float endWidth = 0f;
    [SerializeField] private Material material;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 0f);

    [SerializeField] private float minVertexDistance = 0.1f;
    [SerializeField] private LineTextureMode textureMode = LineTextureMode.Stretch;
    [SerializeField] private LineAlignment alignment = LineAlignment.View;
    [SerializeField] private int sortingOrder = 0;

    private TrailRenderer _trail;
    private Vector3 _lastPos;
    private bool _initialized;

    private void OnValidate()
    {
        EnsureTrail();
        ApplySettings();
    }

    private void Awake()
    {
        EnsureTrail();
        ApplySettings();
    }

    private void OnEnable()
    {
        _lastPos = transform.position;
        _initialized = true;

        if (_trail != null)
        {
            _trail.Clear();
            _trail.emitting = false;
        }
    }

    private void Update()
    {
        if (!_initialized)
        {
            _lastPos = transform.position;
            _initialized = true;
        }

        EnsureTrail();

        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        Vector3 pos = transform.position;
        float speed = (pos - _lastPos).magnitude / dt;
        _lastPos = pos;

        if (_trail != null)
            _trail.emitting = speed >= Mathf.Max(0f, minSpeedToEmit);
    }

    private void EnsureTrail()
    {
        if (_trail != null)
            return;

        _trail = GetComponent<TrailRenderer>();
        if (_trail == null)
            _trail = gameObject.AddComponent<TrailRenderer>();
    }

    private void ApplySettings()
    {
        if (_trail == null)
            return;

        _trail.time = Mathf.Max(0f, trailTime);
        _trail.startWidth = Mathf.Max(0f, startWidth);
        _trail.endWidth = Mathf.Max(0f, endWidth);

        _trail.minVertexDistance = Mathf.Max(0f, minVertexDistance);
        _trail.textureMode = textureMode;
        _trail.alignment = alignment;
        _trail.sortingOrder = sortingOrder;

        if (material != null)
            _trail.material = material;

        _trail.startColor = startColor;
        _trail.endColor = endColor;

        _trail.autodestruct = false;
        _trail.emitting = false;
    }

    public void SetMaterial(Material m)
    {
        material = m;
        if (_trail != null && material != null)
            _trail.material = material;
    }
}
