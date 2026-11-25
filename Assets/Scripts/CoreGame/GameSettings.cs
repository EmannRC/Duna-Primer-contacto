using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Referencias UI (Opcional según qué uses)")]
    [SerializeField] private Toggle fpsLimitToggle;

    private const string FPS_LIMIT_KEY = "LimitFPS";
    private bool isFpsLimited = true;

    private void Awake()
    {
        // Preferencia guardada (por defecto 1 = true / 60 FPS)
        isFpsLimited = PlayerPrefs.GetInt(FPS_LIMIT_KEY, 1) == 1;
        ApplyFpsSetting();
    }

    private void Start()
    {
        if (fpsLimitToggle != null)
        {
            fpsLimitToggle.SetIsOnWithoutNotify(isFpsLimited);
            fpsLimitToggle.onValueChanged.AddListener(SetFpsLimit);
        }
    }

    public void SetFpsLimit(bool limitTo60)
    {
        isFpsLimited = limitTo60;
        ApplyFpsSetting();
        SaveSetting();
    }


    private void ApplyFpsSetting()
    {
        // Desactiva VSync para que Unity respete Application.targetFrameRate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = isFpsLimited ? 60 : -1; // -1 = sin límite
    }

    private void SaveSetting()
    {
        PlayerPrefs.SetInt(FPS_LIMIT_KEY, isFpsLimited ? 1 : 0);
        PlayerPrefs.Save();
    }

}
