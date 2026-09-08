using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public GameObject crosshair;
    private TargetingSystem targetingSystem;

    private void Start()
    {
        Cursor.visible = false;

        if (crosshair != null)
            crosshair.SetActive(false);
    }

    private void Update()
    {
        if (targetingSystem == null)
            return;

        crosshair.SetActive(targetingSystem.isAiming);
    }

    public void Bind(TargetingSystem targeting)
    {
        targetingSystem = targeting;
    }
}
