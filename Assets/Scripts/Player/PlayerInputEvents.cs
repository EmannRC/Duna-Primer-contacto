using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputEvents : MonoBehaviour
{
    private PlayerContext ctx;

    public event Action InteractPressed;

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
        if (ctx.health.IsDead.Value)
            return;

        ctx.movement.SetMoveInput(input.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext input)
    {
        if (ctx == null) return;
        if (ctx.health.IsDead.Value)
            return;

        if (input.performed)
            ctx.movement.SetJump(true);
    }

    public void OnSprint(InputAction.CallbackContext input)
    {
        if (ctx == null) return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.movement.SetSprint(input.ReadValueAsButton());
    }

    public void OnCrouch(InputAction.CallbackContext input)
    {
        if (ctx == null) return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.movement.SetCrouch(input.ReadValueAsButton());
    }

    //=====================//
    //====  INVENTORY  ====//
    //=====================//

    public void OnInventory(InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;
        if (ctx.health.IsDead.Value)
            return;

        UI_Manager.Instance.ToggleInventory();
    }

    //==================//
    //====  CAMERA  ====//
    //==================//

    public void OnAim(InputAction.CallbackContext input)
    {
        if (ctx == null)
            return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.targeting.isAiming = input.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext input)
    {
        if (ctx == null)
            return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.targeting.Look(input.ReadValue<Vector2>());
    }

    public void OnZoom(InputAction.CallbackContext input)
    {
        if (ctx == null)
            return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.targeting.Zoom(input.ReadValue<Vector2>().y);
    }

    public void OnInteract(
        InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;

        Debug.Log("E PRESIONADA");

        if (ctx == null)
            return;


        if (ctx.health.IsDead.Value)
            return;


        InteractPressed?.Invoke();
    }
}
