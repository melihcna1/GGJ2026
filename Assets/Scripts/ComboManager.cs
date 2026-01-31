using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    public static ComboManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("ComboManager");
        var mgr = go.AddComponent<ComboManager>();
        return mgr;
    }

    [SerializeField] private float comboWindowSeconds = 6f;

    public int ComboCount { get; private set; }
    public int Multiplier { get; private set; } = 1;

    public event Action<int> MultiplierChanged;
    public event Action ComboReset;

    private float _lastKillTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ComboCount = 0;
        SetMultiplier(1);
    }

    public void RegisterKill()
    {
        float window = Mathf.Max(0f, comboWindowSeconds);
        bool inWindow = (Time.time - _lastKillTime) <= window;

        if (!inWindow)
            ComboCount = 0;

        ComboCount++;
        _lastKillTime = Time.time;

        int newMultiplier = Mathf.Max(1, ComboCount);
        SetMultiplier(newMultiplier);

        ScoreManager.EnsureInstance().AddKillScore(Multiplier);
    }

    public void ResetCombo()
    {
        ComboCount = 0;
        _lastKillTime = -999f;
        SetMultiplier(1);
        ComboReset?.Invoke();
    }

    private void SetMultiplier(int value)
    {
        int clamped = Mathf.Max(1, value);
        if (Multiplier == clamped)
            return;

        Multiplier = clamped;
        MultiplierChanged?.Invoke(Multiplier);
    }
}
