using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputEvents : MonoBehaviour
{
    private PlayerContext ctx;

    //==============================================//
    public void Bind(PlayerContext playerContext)
    {
        ctx = playerContext;
    }


    //======================//
    //=====  MOVEMENT  =====//
    //======================//
    public void OnMove(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.movement.SetMoveInput(input.ReadValue<Vector2>());
    }

    //=======================================================//
    public void OnJump(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        if (input.performed)
            ctx.movement.SetJump(true);
    }

    //=======================================================//
    public void OnSprint(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.movement.SetSprint(input.ReadValueAsButton());
    }

    //=======================================================//
    public void OnCrouch(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.movement.SetCrouch(input.ReadValueAsButton());
    }


    //=====================//
    //====  INVENTORY  ====//
    //=====================//
    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        UI_Manager.Instance.ToggleInventory();
    }


    //==================//
    //====  CAMERA  ====//
    //==================//
    public void OnAim(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.targeting.isAiming = input.ReadValueAsButton();
    }

    //=======================================================//
    public void OnLook(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.targeting.Look(input.ReadValue<Vector2>());
    }

    //=======================================================//
    public void OnZoom(InputAction.CallbackContext input)
    {
        if (ctx == null) return;

        ctx.targeting.Zoom(input.ReadValue<Vector2>().y);
    }
}
