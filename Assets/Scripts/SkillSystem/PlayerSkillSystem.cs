using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Duna.Player
{
    [DisallowMultipleComponent]
    public class PlayerSkillSystem : NetworkBehaviour
    {
        public enum Skill
        {
            Celerity = 0,
            Impulse = 1,
            Repel = 2
        }

        [Header("Energy")]
        [SerializeField, Min(1)] private int maxEnergy = 10;
        [SerializeField, Min(0)] private int startingEnergy = 10;

        private readonly NetworkVariable<int> energy = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public int MaxEnergy => maxEnergy;
        public int CurrentEnergy => energy.Value;

        public event Action<int, int> OnEnergyChanged;

        [Header("Celerity - Q")]
        [SerializeField, Min(0)] private int celerityCost = 2;
        [SerializeField, Min(0f)] private float celerityCooldown = 1f;

        [Header("Impulse - Space")]
        [SerializeField, Min(0)] private int impulseCost = 4;
        [SerializeField, Min(0f)] private float impulseCooldown = 3f;
        [SerializeField, Min(0.1f)] private float impulseHeight = 5f;

        [Header("Repel - X")]
        [SerializeField, Min(0)] private int repelCost = 8;
        [SerializeField, Min(0f)] private float repelCooldown = 3f;
        [SerializeField, Min(0f)] private float repelRadius = 5f;
        [SerializeField, Min(0f)] private float repelForce = 8f;
        [SerializeField, Min(0.1f)] private float repelDuration = 0.3f;
        [SerializeField] private LayerMask enemyLayers = ~0;

        [Header("References")]
        [SerializeField] private PlayerMovement movement;

        private PlayerContext playerContext;

        private double nextCelerityTime;
        private double nextImpulseTime;
        private double nextRepelTime;

        private bool isInitialized;

        public float CelerityCooldownRemaining =>
            GetCooldownRemaining(nextCelerityTime);

        public float ImpulseCooldownRemaining =>
            GetCooldownRemaining(nextImpulseTime);

        public float RepelCooldownRemaining =>
            GetCooldownRemaining(nextRepelTime);

        public bool CanUseCelerity =>
            CurrentEnergy >= celerityCost &&
            CelerityCooldownRemaining <= 0f;

        public bool CanUseImpulse =>
            CurrentEnergy >= impulseCost &&
            ImpulseCooldownRemaining <= 0f;

        public bool CanUseRepel =>
            CurrentEnergy >= repelCost &&
            RepelCooldownRemaining <= 0f;

        private void Awake()
        {
            playerContext = GetComponent<PlayerContext>();

            if (movement == null)
                movement = GetComponent<PlayerMovement>();

            if (playerContext != null)
                playerContext.skillSystem = this;
        }

        public override void OnNetworkSpawn()
        {
            energy.OnValueChanged += HandleEnergyChanged;

            if (IsServer)
            {
                energy.Value = Mathf.Clamp(
                    startingEnergy,
                    0,
                    maxEnergy
                );
            }

            isInitialized = true;
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
        }

        public override void OnNetworkDespawn()
        {
            energy.OnValueChanged -= HandleEnergyChanged;
            isInitialized = false;
        }

        private void HandleEnergyChanged(int previous, int current)
        {
            OnEnergyChanged?.Invoke(current, maxEnergy);
        }

        private float GetCooldownRemaining(double nextTime)
        {
            if (!IsSpawned)
                return 0f;

            return Mathf.Max(
                0f,
                (float)(nextTime - NetworkManager.ServerTime.Time)
            );
        }

        // --------------------------------------------------
        // PUBLIC SKILL REQUESTS
        // Call these from the player's input events.
        // --------------------------------------------------

        public void TryCelerity()
        {
            RequestSkillRpc((int)Skill.Celerity);
        }

        public void TryImpulse()
        {
            RequestSkillRpc((int)Skill.Impulse);
        }

        public void TryRepel()
        {
            RequestSkillRpc((int)Skill.Repel);
        }

        // --------------------------------------------------
        // ENERGY
        // --------------------------------------------------

        /// <summary>
        /// Adds energy on the server, for example when collecting
        /// a skill crystal. The pickup should call this on the server.
        /// </summary>
        public void AddEnergy(int amount)
        {
            if (!IsServer || amount <= 0)
                return;

            energy.Value = Mathf.Clamp(
                energy.Value + amount,
                0,
                maxEnergy
            );
        }

        public bool HasEnergy(int amount)
        {
            return CurrentEnergy >= amount;
        }

        private bool TrySpendEnergy(int amount)
        {
            if (!IsServer || amount < 0 || energy.Value < amount)
                return false;

            energy.Value -= amount;
            return true;
        }

        // --------------------------------------------------
        // NETWORK REQUEST
        // --------------------------------------------------

        [Rpc(
            SendTo.Server,
            InvokePermission = RpcInvokePermission.Owner
        )]
        private void RequestSkillRpc(
            int skill,
            RpcParams rpcParams = default)
        {
            if (!IsServer || !isInitialized)
                return;

            // Only the owner of this player can request its skills.
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            switch ((Skill)skill)
            {
                case Skill.Celerity:
                    HandleCelerityRequest();
                    break;

                case Skill.Impulse:
                    HandleImpulseRequest();
                    break;

                case Skill.Repel:
                    HandleRepelRequest();
                    break;
            }
        }

        // --------------------------------------------------
        // CELERITY
        // --------------------------------------------------

        private void HandleCelerityRequest()
        {
            double now = NetworkManager.ServerTime.Time;

            if (now < nextCelerityTime)
                return;

            if (!TrySpendEnergy(celerityCost))
                return;

            nextCelerityTime = now + celerityCooldown;

            ExecuteSkillOnOwnerRpc((int)Skill.Celerity);
        }

        // --------------------------------------------------
        // IMPULSE
        // --------------------------------------------------

        private void HandleImpulseRequest()
        {
            double now = NetworkManager.ServerTime.Time;

            if (now < nextImpulseTime)
                return;

            if (!TrySpendEnergy(impulseCost))
                return;

            nextImpulseTime = now + impulseCooldown;

            ExecuteSkillOnOwnerRpc((int)Skill.Impulse);
        }

        // --------------------------------------------------
        // REPEL
        // --------------------------------------------------

        private void HandleRepelRequest()
        {
            double now = NetworkManager.ServerTime.Time;

            if (now < nextRepelTime)
                return;

            // Repel requires the player to be grounded and not dashing.
            // This checks the movement state available on the server.
            if (movement != null)
            {
                if (!movement.IsGrounded || movement.IsDashing)
                    return;
            }

            if (!TrySpendEnergy(repelCost))
                return;

            nextRepelTime = now + repelCooldown;

            // The enemy knockback is executed on the server.
            ExecuteRepelOnServer();

            // The owner's player performs the corresponding local action.
            ExecuteSkillOnOwnerRpc((int)Skill.Repel);
        }

        // --------------------------------------------------
        // OWNER EXECUTION
        // --------------------------------------------------

        [Rpc(
            SendTo.Owner,
            InvokePermission = RpcInvokePermission.Server
        )]
        private void ExecuteSkillOnOwnerRpc(int skill)
        {
            if (!IsOwner)
                return;

            if (movement == null)
            {
                Debug.LogError(
                    "[PlayerSkillSystem] PlayerMovement is missing.",
                    this
                );
                return;
            }

            switch ((Skill)skill)
            {
                case Skill.Celerity:
                    movement.ActivateDashFromSkill();
                    break;

                case Skill.Impulse:
                    movement.ApplyImpulseFromSkill(impulseHeight);
                    break;

                case Skill.Repel:
                    
                    break;
            }
        }

        // --------------------------------------------------
        // SERVER ENEMY KNOCKBACK
        // --------------------------------------------------

        private void ExecuteRepelOnServer()
        {
            if (!IsServer)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                repelRadius,
                enemyLayers,
                QueryTriggerInteraction.Ignore
            );

            HashSet<EnemyMovementBase> affectedEnemies = new();

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                EnemyMovementBase enemy =
                    hit.GetComponentInParent<EnemyMovementBase>();

                if (enemy == null || !affectedEnemies.Add(enemy))
                    continue;

                enemy.ApplyRepel(
                    transform.position,
                    repelForce,
                    repelDuration
                );
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, repelRadius);
        }
    }
}
