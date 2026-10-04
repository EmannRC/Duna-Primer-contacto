using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public GameObject crosshair;
    private TargetingSystem targetingSystem;

    public void Bind(TargetingSystem targeting)
    {
        targetingSystem = targeting;

        Debug.Log(
            $"CrosshairUI vinculado a: {targetingSystem?.gameObject.name}"
        );
    }

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

        if (crosshair != null)
            crosshair.SetActive(targetingSystem.isAiming);
    }
}
