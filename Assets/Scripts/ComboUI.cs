using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [SerializeField] private Text multiplierText;

    [Header("Visuals")]
    [SerializeField] private bool hideWhenMultiplierIsOne = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private float flashDurationSeconds = 0.15f;
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popDurationSeconds = 0.12f;

    private Vector3 _baseScale;
    private Coroutine _anim;

    private void Awake()
    {
        if (multiplierText == null)
            multiplierText = GetComponentInChildren<Text>();

        if (multiplierText != null)
        {
            _baseScale = multiplierText.rectTransform.localScale;
            multiplierText.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
            if (hideWhenMultiplierIsOne)
                multiplierText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        var mgr = ComboManager.EnsureInstance();
        mgr.MultiplierChanged += OnMultiplierChanged;
        mgr.ComboReset += OnComboReset;
        OnMultiplierChanged(mgr.Multiplier);
    }

    private void OnDisable()
    {
        if (ComboManager.Instance == null)
            return;

        ComboManager.Instance.MultiplierChanged -= OnMultiplierChanged;
        ComboManager.Instance.ComboReset -= OnComboReset;
    }

    private void OnMultiplierChanged(int multiplier)
    {
        if (multiplierText == null)
            return;

        multiplierText.text = "x" + Mathf.Max(1, multiplier);
        multiplierText.color = normalColor;

        bool shouldShow = !(hideWhenMultiplierIsOne && multiplier <= 1);
        multiplierText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        if (_anim != null)
            StopCoroutine(_anim);
        _anim = StartCoroutine(PopFlash());
    }

    private void OnComboReset()
    {
        OnMultiplierChanged(1);
    }

    private IEnumerator PopFlash()
    {
        if (multiplierText == null)
            yield break;

        var rt = multiplierText.rectTransform;
        rt.localScale = _baseScale;

        float popT = Mathf.Max(0.0001f, popDurationSeconds);
        float flashT = Mathf.Max(0.0001f, flashDurationSeconds);

        float t = 0f;
        while (t < popT)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / popT);
            float s = Mathf.Lerp(1f, popScale, a);
            rt.localScale = _baseScale * s;
            yield return null;
        }

        t = 0f;
        while (t < popT)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / popT);
            float s = Mathf.Lerp(popScale, 1f, a);
            rt.localScale = _baseScale * s;
            yield return null;
        }

        t = 0f;
        while (t < flashT)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / flashT);
            multiplierText.color = Color.Lerp(flashColor, normalColor, a);
            yield return null;
        }

        multiplierText.color = normalColor;
        rt.localScale = _baseScale;
        _anim = null;
    }
}
