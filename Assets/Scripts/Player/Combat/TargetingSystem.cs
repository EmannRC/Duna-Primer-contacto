using Duna.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    private CameraController cameraController;
    private PlayerContext ctx;

    public bool isAiming { get; private set; }

    private bool CanAim =>
        ctx != null &&
        ctx.state != null &&
        ctx.state.CurrentState == PlayerActionState.Normal;

    //========================================================//
    // INITIALIZATION
    //========================================================//

    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //========================================================//
    // STATE
    //========================================================//

    private void Update()
    {
        if (!CanAim && isAiming)
        {
            SetAiming(false);
        }
    }

    //========================================================//
    // AIM
    //========================================================//

    public void SetAiming(bool aiming)
    {
        // No permitir activar el apuntado en estados bloqueados.
        if (aiming && !CanAim)
            return;

        if (isAiming == aiming)
            return;

        isAiming = aiming;

        if (ctx != null && ctx.movement != null)
        {
            ctx.movement.SetAiming(aiming);
        }
    }

    //========================================================//
    // CAMERA
    //========================================================//

    public void Bind(CameraController cam)
    {
        cameraController = cam;
    }

    public void Look(Vector2 input)
    {
        if (cameraController == null)
            return;

        cameraController.SetLookInput(input);
    }

    public void Zoom(float value)
    {
        if (cameraController == null)
            return;

        cameraController.SetZoomInput(value);
    }
}
