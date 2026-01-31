using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class RamResource : MonoBehaviour
{
    [SerializeField] private float maxRam = 100f;
    [SerializeField] private float regenPerSecond = 30f;
    [SerializeField] private bool useRhythmRegen = true;
    [SerializeField] private float ramRegenRythm = 1f;
    [FormerlySerializedAs("realtimeRegenWeight")]
    [SerializeField] private float popupSlowdownPerPopup = 0.2f;

    private Coroutine _rhythmRegenRoutine;

    public float MaxRam => maxRam;
    public float CurrentRam { get; private set; }

    public float Normalized => maxRam <= 0f ? 0f : Mathf.Clamp01(CurrentRam / maxRam);

    private void Awake()
    {
        CurrentRam = maxRam;
    }

    private void OnEnable()
    {
        StartRhythmRegen();
    }

    private void OnDisable()
    {
        StopRhythmRegen();
    }

    private void StartRhythmRegen()
    {
        if (_rhythmRegenRoutine != null)
            return;

        if (!useRhythmRegen)
            return;

        _rhythmRegenRoutine = StartCoroutine(RhythmRegenRoutine());
    }

    private void StopRhythmRegen()
    {
        if (_rhythmRegenRoutine == null)
            return;

        StopCoroutine(_rhythmRegenRoutine);
        _rhythmRegenRoutine = null;
    }

    private IEnumerator RhythmRegenRoutine()
    {
        while (true)
        {
            if (!useRhythmRegen)
            {
                _rhythmRegenRoutine = null;
                yield break;
            }

            if (VirusRhythmClock.Instance == null)
            {
                yield return null;
                continue;
            }

            float interval = VirusRhythmClock.Instance.GetIntervalSeconds(ramRegenRythm);
            if (interval <= 0f || float.IsInfinity(interval))
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(interval);

            if (CurrentRam >= maxRam)
                continue;

            int popupCount = PopupWindow.ActivePopupCount;
            float slowdown = 1f + Mathf.Max(0f, popupSlowdownPerPopup) * Mathf.Max(0, popupCount);

            float amountPerTick = (regenPerSecond / slowdown) * interval;
            if (amountPerTick <= 0f)
                continue;

            CurrentRam = Mathf.Min(maxRam, CurrentRam + amountPerTick);
        }
    }

    public bool CanSpend(float amount)
    {
        return amount > 0f && CurrentRam >= amount;
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f)
            return true;

        if (CurrentRam < amount)
            return false;

        CurrentRam -= amount;
        return true;
    }
}
