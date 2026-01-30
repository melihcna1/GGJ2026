using UnityEngine;
using UnityEngine.UI;

public class RamUIBar : MonoBehaviour
{
    [SerializeField] private RamResource ram;
    [SerializeField] private Image fillImage;

    private void Update()
    {
        if (ram == null || fillImage == null)
            return;

        fillImage.fillAmount = ram.Normalized;
    }
}
