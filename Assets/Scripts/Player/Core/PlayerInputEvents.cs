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

        if (ctx != null)
            ctx.skillSystem?.TryImpulse();
            //ctx.playerAnimation.PlayJumpAnimation();
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

    //======================//
    //== INTERACTIVE MENU ==//
    //======================//
    public void OnMenu(InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;
        if (ctx.health.IsDead.Value)
            return;

        UI_Manager.Instance.ToggleInteractiveMenu();
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

        ctx.targeting.SetAiming(
            input.ReadValueAsButton()
        );
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

    public void OnInteract(InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;

        if (ctx == null)
            return;

        if (ctx.health.IsDead.Value)
            return;


        InteractPressed?.Invoke();
    }

    //=====================//
    //=====  COMBAT  ======//
    //=====================//

    public void OnShoot(InputAction.CallbackContext input)
    {
        if (ctx == null)
            return;

        if (ctx.health.IsDead.Value)
        {
            ctx.combat?.SetShootInput(false);
            return;
        }

        if (ctx.combat == null)
            return;

        if (input.performed)
        {
            Debug.Log("SHOOT INPUT PRESSED");
            ctx.combat.SetShootInput(true);
        }
        else if (input.canceled)
        {
            Debug.Log("SHOOT INPUT RELEASED");
            ctx.combat.SetShootInput(false);
        }
    }

    //=====================//
    //====  ABILITY  ======//
    //=====================//

    public void OnCelerity()
    {
        if (ctx != null)
            ctx.skillSystem?.TryCelerity();
    }

    public void OnRepel()
    {
        if (ctx != null)
            ctx.skillSystem?.TryRepel();
    }



    //=====================//
    //====  DANCE  ====//
    //=====================//

    public void OnDance(InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;
        if (ctx.health.IsDead.Value)
            return;

        ctx.playerAnimation.PlayDanceAnimation();
    }


    public void OnDash(InputAction.CallbackContext input)
    {
        if (!input.performed)
            return;

        if (ctx == null || ctx.health.IsDead.Value)
            return;

        ctx.skillSystem?.TryCelerity();
    }
}
