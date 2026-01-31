using System;
using System.Collections;
using UnityEngine;

public class VirusRhythmClock : MonoBehaviour
{
    public static VirusRhythmClock Instance { get; private set; }

    [SerializeField] private float virusRythm = 1f;

    public float VirusRythmSeconds => virusRythm;

    public event Action Beat;

    private Coroutine _routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        return Mathf.Max(0.0001f, virusRythm / divisor);
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
        while (true)
        {
            float wait = Mathf.Max(0.0001f, virusRythm);
            yield return new WaitForSeconds(wait);
            Beat?.Invoke();
        }
    }
}
