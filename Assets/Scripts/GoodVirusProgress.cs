using System;
using UnityEngine;

public class GoodVirusProgress : MonoBehaviour
{
    public static GoodVirusProgress Instance { get; private set; }

    [SerializeField] private int targetCount = 10;

    public int TargetCount => targetCount;
    public int CurrentCount { get; private set; }

    public event Action<int, int> ProgressChanged;
    public event Action ProgressCompleted;

    private bool _completed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        NotifyChanged();
    }

    public void AddProgress(int amount)
    {
        if (_completed)
            return;

        if (amount <= 0)
            return;

        CurrentCount = Mathf.Clamp(CurrentCount + amount, 0, Mathf.Max(1, targetCount));
        NotifyChanged();

        if (CurrentCount >= targetCount)
        {
            _completed = true;
            ProgressCompleted?.Invoke();
        }
    }

    private void NotifyChanged()
    {
        ProgressChanged?.Invoke(CurrentCount, targetCount);
    }
}
