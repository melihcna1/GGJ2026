using UnityEngine;

public class DifficultyCurveController : MonoBehaviour
{
    [Header("Curve")]
    [SerializeField] private AnimationCurve difficulty01 = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.25f, 1f),
        new Keyframe(0.5f, 0f),
        new Keyframe(0.75f, 1f),
        new Keyframe(1f, 0f)
    );

    [SerializeField] private float cycleDurationSeconds = 60f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float timeOffsetSeconds = 0f;

    [Header("Output")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;

    [Header("Additive Looping")]
    [SerializeField] private bool additivePerLoop = false;

    public float GetValue01()
    {
        float duration = Mathf.Max(0.0001f, cycleDurationSeconds);
        float t = (Time.time + timeOffsetSeconds) / duration;

        if (loop)
            t = Mathf.Repeat(t, 1f);
        else
            t = Mathf.Clamp01(t);

        float v = difficulty01 != null ? difficulty01.Evaluate(t) : 0f;
        return Mathf.Clamp01(v);
    }

    public float GetValue()
    {
        float t01 = GetValue01();
        float baseMin = minValue;
        float baseMax = maxValue;

        if (loop && additivePerLoop)
        {
            float duration = Mathf.Max(0.0001f, cycleDurationSeconds);
            float t = (Time.time + timeOffsetSeconds) / duration;
            int loopIndex = Mathf.FloorToInt(t);

            float step = maxValue - minValue;
            baseMin = minValue + loopIndex * step;
            baseMax = maxValue + loopIndex * step;
        }

        return Mathf.Lerp(baseMin, baseMax, t01);
    }
}
