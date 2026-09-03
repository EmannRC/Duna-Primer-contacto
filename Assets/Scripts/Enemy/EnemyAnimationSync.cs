using Unity.Netcode;
using UnityEngine;

public class EnemyAnimationSync : NetworkBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    public NetworkVariable<float> VelX =
        new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<float> VelY =
        new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> AttackCounter =
    new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GroundEnemyMovement movement;

    private static readonly int VelXHash =
        Animator.StringToHash("VelX");

    private static readonly int VelYHash =
        Animator.StringToHash("VelY");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    //============================================================//
    private void Awake()
    {
        movement = GetComponent<GroundEnemyMovement>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        VelX.OnValueChanged += OnVelXChanged;
        VelY.OnValueChanged += OnVelYChanged;
        AttackCounter.OnValueChanged += OnAttackChanged;

        UpdateAnimator();
    }

    public override void OnNetworkDespawn()
    {
        VelX.OnValueChanged -= OnVelXChanged;
        VelY.OnValueChanged -= OnVelYChanged;
        AttackCounter.OnValueChanged -= OnAttackChanged;

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        UpdateVelocity();
    }

    private void UpdateVelocity()
    {
        Vector3 velocity = movement.Velocity;

        // Convertimos la velocidad mundial
        // a velocidad relativa al enemigo.
        Vector3 localVelocity =
            transform.InverseTransformDirection(velocity);

        UpdateFloat(VelX, localVelocity.x);
        UpdateFloat(VelY, localVelocity.z);
    }

    private void UpdateFloat(
        NetworkVariable<float> variable,
        float value)
    {
        if (Mathf.Abs(variable.Value - value) < 0.01f)
            return;

        variable.Value = value;
    }
    public void NotifyAttack()
    {
        if (!IsServer)
            return;

        AttackCounter.Value++;
    }

    //============================================================//
    private void OnVelXChanged(float previous, float current)
    {
        UpdateAnimator();
    }

    private void OnVelYChanged(float previous, float current)
    {
        UpdateAnimator();
    }
    private void OnAttackChanged(int previous, int current)
    {
        if (animator == null)
            return;

        animator.SetTrigger(AttackHash);
    }


    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(VelXHash, VelX.Value);
        animator.SetFloat(VelYHash, VelY.Value);
    }

   
}
