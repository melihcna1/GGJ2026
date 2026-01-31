using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RecycleBinDragSpawner : MonoBehaviour, IPointerClickHandler
{
    [Header("Dummy Prefabs")]
    [SerializeField] private List<GameObject> dummyPrefabs;

    [Header("Cooldown")]
    [SerializeField] private float recycleCooldown = 5f;

    [Header("Cooldown UI")]
    [SerializeField] private GameObject cooldownBarRoot;
    [SerializeField] private Image cooldownBarFill;
    [SerializeField] private bool cooldownFillGoesUp = true;

    private bool onCooldown;
    private float cooldownTimer;

    private GameObject currentDummy;
    private Camera cam;

    private bool isPlacing;
    private readonly List<SpriteRenderer> _previewSpriteRenderers = new List<SpriteRenderer>(64);
    private readonly List<MeshRenderer> _previewMeshRenderers = new List<MeshRenderer>(64);
    private readonly List<Collider2D> _previewColliders2D = new List<Collider2D>(64);
    private readonly List<Collider> _previewColliders3D = new List<Collider>(64);
    private readonly Dictionary<Object, Color> _originalColors = new Dictionary<Object, Color>(128);

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleCooldown();

        if (cam == null)
            cam = Camera.main;

        UpdateCooldownUI();

        if (isPlacing && currentDummy != null)
        {
            FollowPointer();

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                CancelPlacement();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CancelPlacement();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
                PlaceDummy();
        }
    }

    public void OnRecycleBinClicked()
    {
        if (onCooldown || currentDummy != null || isPlacing)
            return;

        StartPlacement();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnRecycleBinClicked();
    }

    private void StartPlacement()
    {
        SpawnPreviewDummy();
        if (currentDummy == null)
            return;

        SetupAsPreview(currentDummy);
        isPlacing = true;
    }

    // ------------------------

    void HandleCooldown()
    {
        if (!onCooldown)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
            onCooldown = false;
    }

    private void UpdateCooldownUI()
    {
        if (cooldownBarRoot != null)
            cooldownBarRoot.SetActive(onCooldown);

        if (cooldownBarFill == null)
            return;

        float total = Mathf.Max(0.0001f, recycleCooldown);
        float remaining01 = Mathf.Clamp01(cooldownTimer / total);
        cooldownBarFill.fillAmount = cooldownFillGoesUp ? (1f - remaining01) : remaining01;
    }

    void SpawnPreviewDummy()
    {
        if (dummyPrefabs == null || dummyPrefabs.Count == 0)
        {
            Debug.LogWarning("RecycleBin: Dummy prefab list empty!");
            return;
        }

        int index = Random.Range(0, dummyPrefabs.Count);
        currentDummy = Instantiate(dummyPrefabs[index]);
    }

    private void SetupAsPreview(GameObject go)
    {
        _previewSpriteRenderers.Clear();
        _previewMeshRenderers.Clear();
        _previewColliders2D.Clear();
        _previewColliders3D.Clear();
        _originalColors.Clear();

        go.GetComponentsInChildren(true, _previewSpriteRenderers);
        go.GetComponentsInChildren(true, _previewMeshRenderers);
        go.GetComponentsInChildren(true, _previewColliders2D);
        go.GetComponentsInChildren(true, _previewColliders3D);

        for (int i = 0; i < _previewSpriteRenderers.Count; i++)
        {
            var sr = _previewSpriteRenderers[i];
            if (sr == null)
                continue;
            _originalColors[sr] = sr.color;
            var c = sr.color;
            c.a *= 0.5f;
            sr.color = c;
        }

        for (int i = 0; i < _previewMeshRenderers.Count; i++)
        {
            var mr = _previewMeshRenderers[i];
            if (mr == null)
                continue;

            var mat = mr.material;
            if (mat == null || !mat.HasProperty("_Color"))
                continue;

            _originalColors[mat] = mat.color;
            var c = mat.color;
            c.a *= 0.5f;
            mat.color = c;
        }

        for (int i = 0; i < _previewColliders2D.Count; i++)
        {
            var col = _previewColliders2D[i];
            if (col != null)
                col.enabled = false;
        }

        for (int i = 0; i < _previewColliders3D.Count; i++)
        {
            var col = _previewColliders3D[i];
            if (col != null)
                col.enabled = false;
        }
    }

    void FollowPointer()
    {
        if (cam == null || Mouse.current == null)
            return;

        Vector3 pos = Mouse.current.position.ReadValue();
        pos.z = Mathf.Abs(cam.transform.position.z);
        var wp = cam.ScreenToWorldPoint(pos);
        currentDummy.transform.position = new Vector3(wp.x, wp.y, 0f);
    }

    void PlaceDummy()
    {
        if (currentDummy == null)
            return;

        RestoreFromPreview(currentDummy);

        currentDummy = null;
        isPlacing = false;
        onCooldown = true;
        cooldownTimer = recycleCooldown;
    }

    private void RestoreFromPreview(GameObject go)
    {
        for (int i = 0; i < _previewSpriteRenderers.Count; i++)
        {
            var sr = _previewSpriteRenderers[i];
            if (sr == null)
                continue;
            if (_originalColors.TryGetValue(sr, out var c))
                sr.color = c;
        }

        for (int i = 0; i < _previewMeshRenderers.Count; i++)
        {
            var mr = _previewMeshRenderers[i];
            if (mr == null)
                continue;

            var mat = mr.material;
            if (mat == null)
                continue;
            if (_originalColors.TryGetValue(mat, out var c))
                mat.color = c;
        }

        for (int i = 0; i < _previewColliders2D.Count; i++)
        {
            var col = _previewColliders2D[i];
            if (col != null)
                col.enabled = true;
        }

        for (int i = 0; i < _previewColliders3D.Count; i++)
        {
            var col = _previewColliders3D[i];
            if (col != null)
                col.enabled = true;
        }
    }

    private void CancelPlacement()
    {
        if (currentDummy != null)
            Destroy(currentDummy);

        currentDummy = null;
        isPlacing = false;
    }

    private static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Mouse.current != null)
            return EventSystem.current.IsPointerOverGameObject();

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(id);
        }

        return false;
    }
    
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (!isPlacing)
            OnRecycleBinClicked();
        else
        {
            if (!IsPointerOverUI())
                PlaceDummy();
        }
    }
}
