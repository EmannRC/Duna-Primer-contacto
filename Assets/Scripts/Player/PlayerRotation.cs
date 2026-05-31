using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private PlayerContext ctx;

    Vector3 attackDirection;
    bool attackRotation;

    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        if (attackRotation)
        {
            FaceDirection(attackDirection);
            return;
        }

        if (ctx.targeting && ctx.targeting.isAiming)
        {
            FaceCamera();
            return;
        }

        if (ctx.movement.MoveDirection.sqrMagnitude > 0.01f)
        {
            FaceDirection(ctx.movement.MoveDirection);
        }
    }

    void FaceCamera()
    {
        Vector3 dir = Camera.main.transform.forward;

        dir.y = 0;

        FaceDirection(dir);
    }

    void FaceDirection(Vector3 dir)
    {
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation,targetRot,Time.deltaTime * 10f);
    }

    public void LookAt(Vector3 direction)
    {
        attackDirection = direction;
        attackRotation = true;
    }

    public void StopAttackRotation()
    {
        attackRotation = false;
    }
}
