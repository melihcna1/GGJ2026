using System;
using System.Collections;
using UnityEngine;

public class VirusRhythmClock : MonoBehaviour
{
    public static VirusRhythmClock Instance { get; private set; }

    [SerializeField] private float startBpm = 70f;
    [SerializeField] private float endBpm = 110f;
    [SerializeField] private int stageCount = 5;
    [SerializeField] private float stageIntervalSeconds = 30f;

    public float CurrentBpm { get; private set; }
    public float VirusRythmSeconds => SecondsPerBeat;
    public float SecondsPerBeat => 60f / Mathf.Max(0.0001f, CurrentBpm);

    public event Action Beat;

    private Coroutine _routine;
    private int _stageIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetStage(0);
    }

    private void OnEnable()
    {
        StartClock();
    }

    private void OnDisable()
    {
        StopClock();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public float GetIntervalSeconds(float divisor)
    {
        if (divisor <= 0f)
            return float.PositiveInfinity;

        return Mathf.Max(0.0001f, SecondsPerBeat / divisor);
    }

    public void StartClock()
    {
        if (_routine != null)
            return;

        _routine = StartCoroutine(ClockRoutine());
    }

    public void StopClock()
    {
        if (_routine == null)
            return;

        StopCoroutine(_routine);
        _routine = null;
    }

    private IEnumerator ClockRoutine()
    {
        float nextStageTime = Time.time + Mathf.Max(0f, stageIntervalSeconds);
        while (true)
        {
            float wait = Mathf.Max(0.0001f, SecondsPerBeat);
            yield return new WaitForSeconds(wait);
            Beat?.Invoke();

            if (stageIntervalSeconds > 0f)
            {
                while (Time.time >= nextStageTime)
                {
                    AdvanceStage();
                    nextStageTime += stageIntervalSeconds;
                }
            }
        }
    }

    private void AdvanceStage()
    {
        int maxStageIndex = Mathf.Max(0, stageCount - 1);
        if (_stageIndex >= maxStageIndex)
            return;

        SetStage(_stageIndex + 1);
    }

    private void SetStage(int stageIndex)
    {
        _stageIndex = Mathf.Clamp(stageIndex, 0, Mathf.Max(0, stageCount - 1));

        float bpm;
        if (stageCount <= 1)
        {
            bpm = startBpm;
        }
        else
        {
            float step = (endBpm - startBpm) / (stageCount - 1);
            bpm = startBpm + step * _stageIndex;
        }

        CurrentBpm = Mathf.Max(0.0001f, bpm);
    }
}
