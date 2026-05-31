using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    private CameraController cameraController;

    public bool isAiming;

    //========================================//
    public void Bind(CameraController cam)
    {
        cameraController = cam;
    }

    //========================================//
    public void Look(Vector2 input)
    {
        if (cameraController == null) return;

        cameraController.SetLookInput(input);
    }

    //========================================//
    public void Zoom(float value)
    {
        if (cameraController == null) return;

        cameraController.SetZoomInput(value);
    }
}
