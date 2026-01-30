using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float moveUpSpeed = 1.25f;

    private TextMesh _textMesh;
    private Color _baseColor;
    private float _age;

    private void Awake()
    {
        _textMesh = GetComponent<TextMesh>();
        _baseColor = _textMesh.color;
    }

    public void Initialize(int amount, Color color, float size)
    {
        if (_textMesh == null)
            _textMesh = GetComponent<TextMesh>();

        _textMesh.text = amount.ToString();
        _textMesh.color = color;
        _textMesh.characterSize = size;
        _baseColor = _textMesh.color;
        _age = 0f;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _age += dt;

        transform.position += Vector3.up * (moveUpSpeed * dt);

        float t = lifetime <= 0f ? 1f : Mathf.Clamp01(_age / lifetime);
        var c = _baseColor;
        c.a = 1f - t;
        if (_textMesh != null)
            _textMesh.color = c;

        if (_age >= lifetime)
            Destroy(gameObject);
    }
}
