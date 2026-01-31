using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class RamResource : MonoBehaviour
{
    [SerializeField] private float maxRam = 100f;
    [SerializeField] private float regenPerSecond = 30f;
    [FormerlySerializedAs("realtimeRegenWeight")]
    [SerializeField] private float popupSlowdownPerPopup = 0.2f;

    public float MaxRam => maxRam;
    public float CurrentRam { get; private set; }

    public float Normalized => maxRam <= 0f ? 0f : Mathf.Clamp01(CurrentRam / maxRam);

    private void Awake()
    {
        CurrentRam = maxRam;
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
