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

    private float _regenTimer;

    public float MaxRam => maxRam;
    public float CurrentRam { get; private set; }

    public float Normalized => maxRam <= 0f ? 0f : Mathf.Clamp01(CurrentRam / maxRam);

    private void Awake()
    {
        CurrentRam = maxRam;
    }

    private void Update()
    {
        if (!useRhythmRegen)
            return;

        if (VirusRhythmClock.Instance == null)
            return;

        float interval = VirusRhythmClock.Instance.GetIntervalSeconds(ramRegenRythm);
        if (interval <= 0f || float.IsInfinity(interval))
            return;

        _regenTimer += Time.deltaTime;
        if (_regenTimer < interval)
            return;

        float tickDt = _regenTimer;
        _regenTimer = 0f;

        int popupCount = PopupWindow.ActivePopupCount;
        float slowdown = 1f + Mathf.Max(0f, popupSlowdownPerPopup) * Mathf.Max(0, popupCount);

        float amountPerTick = (regenPerSecond / slowdown) * tickDt;
        if (amountPerTick <= 0f)
            return;

        CurrentRam = Mathf.Min(maxRam, CurrentRam + amountPerTick);
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

    public void RegenerateAmountOverTime(float amount)
    {
        if (amount <= 0f)
            return;

        StartCoroutine(RegenerateRoutine(amount));
    }

    private IEnumerator RegenerateRoutine(float amount)
    {
        float remaining = Mathf.Max(0f, amount);

        float lastRealTime = Time.realtimeSinceStartup;

        while (remaining > 0f)
        {
            float now = Time.realtimeSinceStartup;
            float realtimeDt = Mathf.Max(0f, now - lastRealTime);
            lastRealTime = now;

            int popupCount = PopupWindow.ActivePopupCount;
            float slowdown = 1f + Mathf.Max(0f, popupSlowdownPerPopup) * Mathf.Max(0, popupCount);

            float delta = (regenPerSecond / slowdown) * realtimeDt;
            float add = Mathf.Min(delta, remaining);

            CurrentRam = Mathf.Min(maxRam, CurrentRam + add);
            remaining -= add;

            yield return null;
        }
    }
}
