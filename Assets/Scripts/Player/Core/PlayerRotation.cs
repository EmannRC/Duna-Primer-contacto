using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerRotation : NetworkBehaviour
{
    private PlayerContext ctx;
    private Vector3 attackDirection;
    private bool attackRotation;

    //=======================================================//
    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }

    //=======================================================//
    private void Update()
    {
        if (!IsOwner)
            return;

        Rotate();
    }

    //=======================================================//
    private void Rotate()
    {
        if (attackRotation)
        {
            FaceDirection(attackDirection);
            return;
        }

        if (ctx.targeting != null && ctx.targeting.isAiming)
        {
            FaceCamera();
            return;
        }

        if (ctx.movement.MoveDirection.sqrMagnitude > 0.01f)
        {
            FaceDirection(ctx.movement.MoveDirection);
        }
    }

    //=======================================================//
    private void FaceCamera()
    {
        if (ctx.mainCamera == null)
            return;

        Vector3 dir = ctx.mainCamera.transform.forward;

        dir.y = 0;

        FaceDirection(dir);
    }

    //=======================================================//
    private void FaceDirection(Vector3 dir)
    {
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 10f);
    }

    //=======================================================//
    public void LookAt(Vector3 direction)
    {
        attackDirection = direction;
        attackRotation = true;
    }

    //=======================================================//
    public void StopAttackRotation()
    {
        attackRotation = false;
    }
}
