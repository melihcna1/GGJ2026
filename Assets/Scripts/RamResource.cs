using System.Collections;
using UnityEngine;

public class RamResource : MonoBehaviour
{
    [SerializeField] private float maxRam = 100f;
    [SerializeField] private float regenPerSecond = 30f;

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

        while (remaining > 0f)
        {
            float delta = regenPerSecond * Time.deltaTime;
            float add = Mathf.Min(delta, remaining);

            CurrentRam = Mathf.Min(maxRam, CurrentRam + add);
            remaining -= add;

            yield return null;
        }
    }
}
