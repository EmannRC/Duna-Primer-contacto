using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public GameObject crosshair;
    private TargetingSystem targetingSystem;

    void Start()
    {
        Cursor.visible = false;

        targetingSystem = FindFirstObjectByType<TargetingSystem>();
    }

    void Update()
    {
        if (targetingSystem == null)
            return;

        crosshair.SetActive(targetingSystem.isAiming);
    }
}
