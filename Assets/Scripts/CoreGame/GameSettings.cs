using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
}
