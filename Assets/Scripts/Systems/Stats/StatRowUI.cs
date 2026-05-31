using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text valueText;

    public void SetValue(float value, float max)
    {
        fill.fillAmount = value / max;
        valueText.text = value.ToString("0.##");
    }
}
