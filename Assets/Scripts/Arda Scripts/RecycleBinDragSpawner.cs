using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecycleBinDragSpawner : MonoBehaviour
{
    [Header("Dummy Prefabs")]
    [SerializeField] private List<GameObject> dummyPrefabs;

    [Header("Cooldown")]
    [SerializeField] private float recycleCooldown = 5f;

    private bool onCooldown;
    private float cooldownTimer;

    private GameObject currentDummy;
    private Camera cam;

    private bool isDragging;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleCooldown();

        if (isDragging && currentDummy != null)
            FollowPointer();
    }

    // NEW INPUT SYSTEM — sol tık basılı
    public void OnPointerDown()
    {
        if (onCooldown || currentDummy != null)
            return;

        if (!IsPointerOverRecycleBin())
            return;

        SpawnPreviewDummy();
        isDragging = true;
    }

    // NEW INPUT SYSTEM — sol tık bırakıldı
    public void OnPointerUp()
    {
        if (!isDragging || currentDummy == null)
            return;

        PlaceDummy();
        isDragging = false;
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

    void FollowPointer()
    {
        Vector3 pos = Mouse.current.position.ReadValue();
        pos.z = Mathf.Abs(cam.transform.position.z);
        currentDummy.transform.position = cam.ScreenToWorldPoint(pos);
    }

    void PlaceDummy()
    {
        currentDummy = null;
        onCooldown = true;
        cooldownTimer = recycleCooldown;
    }

    bool IsPointerOverRecycleBin()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = cam.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
    
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            OnPointerDown();

        if (ctx.canceled)
            OnPointerUp();
    }
}
