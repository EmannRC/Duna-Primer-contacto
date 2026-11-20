using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Duna.Player.PlayerSkillSystem;

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

        public float ImpulseHeight => impulseHeight;

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

        [Header("Skill VFX")]

        [Header("Celerity")]
        [SerializeField] private GameObject celerityVFXPrefab;
        [SerializeField] private Transform celerityVFXPoint;

        [Header("Impulse")]
        [SerializeField] private GameObject impulseVFXPrefab;
        [SerializeField] private Transform impulseVFXPoint;

        [Header("Repel")]
        [SerializeField] private GameObject repelVFXPrefab;
        [SerializeField] private Transform repelVFXPoint;

        [Header("Skill SFX")]

        [Header("Celerity sound")]
        [SerializeField] private AudioSource celeritySound;

        [Header("Impulse sound")]
        [SerializeField] private AudioSource impulseSound;

        [Header("Repel sound")]
        [SerializeField] private AudioSource repelSound;

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
            playerContext != null &&
            playerContext.state != null &&
            playerContext.state.CanUseSkills &&
            CurrentEnergy >= celerityCost &&
            CelerityCooldownRemaining <= 0f;

        public bool CanUseImpulse =>
            playerContext != null &&
            playerContext.state != null &&
            playerContext.state.CanUseSkills &&
            CurrentEnergy >= impulseCost &&
            ImpulseCooldownRemaining <= 0f;

        public bool CanUseRepel =>
            playerContext != null &&
            playerContext.state != null &&
            playerContext.state.CanUseSkills &&
            CurrentEnergy >= repelCost &&
            RepelCooldownRemaining <= 0f;

        private void Awake()
        {
            playerContext = GetComponentInParent<PlayerContext>();

            if (movement == null)
                movement = GetComponent<PlayerMovement>();

            if (playerContext != null)
                playerContext.skillSystem = this;
        }

        private void Update()
        {
            if (!IsOwner || playerContext == null || playerContext.state == null)
                return;

            Debug.Log(
                $"[SkillSystem] Estado: {playerContext.state.CurrentState} | " +
                $"Puede usar habilidades: {playerContext.state.CanUseSkills}"
            );
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

        //--------------------------------------------------

        public void TryCelerity()
        {
            if (!CanRequestSkillLocally())
                return;

            RequestSkillRpc((int)Skill.Celerity);
        }

        public void TryImpulse()
        {
            if (!CanRequestSkillLocally())
                return;

            RequestSkillRpc((int)Skill.Impulse);
        }

        public void TryRepel()
        {
            if (!CanRequestSkillLocally())
                return;

            if (movement != null && (!movement.IsGrounded || movement.IsDashing))
                return;

            RequestSkillRpc((int)Skill.Repel);
        }

        private bool CanRequestSkillLocally()
        {
            return IsOwner &&
                   playerContext != null &&
                   playerContext.state != null &&
                   playerContext.state.CanUseSkills;
        }

        // --------------------------------------------------
        // ENERGY
        // --------------------------------------------------


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

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void RequestSkillRpc(int skill, RpcParams rpcParams = default)
        {
            if (!IsServer || !isInitialized)
                return;

            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            if (playerContext == null ||
                playerContext.state == null ||
                !playerContext.state.CanUseSkills)
            {
                return;
            }

            if (!System.Enum.IsDefined(typeof(Skill), skill))
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

            PlaySkillVFXRpc((int)Skill.Celerity);
            PlaySkillSoundRpc((int)Skill.Celerity);
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

            PlaySkillVFXRpc((int)Skill.Impulse);
            PlaySkillSoundRpc((int)Skill.Impulse);
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

            // Eliminado el chequeo de movement.IsGrounded / movement.IsDashing aquí,
            // ya que PlayerMovement solo actualiza groundedTimer en IsOwner.

            if (!TrySpendEnergy(repelCost))
                return;

            nextRepelTime = now + repelCooldown;

            ExecuteRepelOnServer();

            PlaySkillVFXRpc((int)Skill.Repel);
            PlaySkillSoundRpc((int)Skill.Repel);
            ExecuteSkillOnOwnerRpc((int)Skill.Repel);
        }

        // --------------------------------------------------
        // OWNER EXECUTION
        // --------------------------------------------------

        [Rpc(SendTo.Owner,InvokePermission = RpcInvokePermission.Server)]
        private void ExecuteSkillOnOwnerRpc(int skill)
        {
            if (!IsOwner)
                return;

            if (playerContext == null ||
                playerContext.state == null ||
                !playerContext.state.CanUseSkills)
            {
                return;
            }

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

                    if (playerContext.playerAnimation != null)
                        playerContext.playerAnimation.PlayRepelAnimation();

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

        //VFX
        [Rpc(SendTo.ClientsAndHost)]
        private void PlaySkillVFXRpc(int skill)
        {
            GameObject prefab = null;
            Transform vfxPoint = null;

            switch ((Skill)skill)
            {
                case Skill.Celerity:
                    prefab = celerityVFXPrefab;
                    vfxPoint = celerityVFXPoint;
                    //celeritySound.Play();
                    break;

                case Skill.Impulse:
                    prefab = impulseVFXPrefab;
                    vfxPoint = impulseVFXPoint;
                    //impulseSound.Play();
                    break;

                case Skill.Repel:
                    prefab = repelVFXPrefab;
                    vfxPoint = repelVFXPoint;
                    //repelSound.Play();
                    break;
            }

            if (prefab == null)
                return;

            Vector3 position = vfxPoint != null
                ? vfxPoint.position
                : transform.position;

            Quaternion rotation = vfxPoint != null
                ? vfxPoint.rotation
                : transform.rotation;

            Instantiate(prefab, position, rotation);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlaySkillSoundRpc(int skill)
        {
            switch ((Skill)skill)
            {
                case Skill.Celerity:
                    if (celeritySound != null)
                        celeritySound.Play();
                    break;

                case Skill.Impulse:
                    if (impulseSound != null)
                        impulseSound.Play();
                    break;

                case Skill.Repel:
                    if (repelSound != null)
                        repelSound.Play();
                    break;
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, repelRadius);
        }
    }
}
