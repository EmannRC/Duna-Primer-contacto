using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace Duna.Player
{
    public enum PlayerActionState
    {
        Normal,
        InMenu,
        InDialogue,
        Interacting,
        Stunned,
        Dead
    }

    [DisallowMultipleComponent]
    public class PlayerState : NetworkBehaviour
    {
        private readonly NetworkVariable<PlayerActionState> currentState =
            new(
                PlayerActionState.Normal,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

        public PlayerActionState CurrentState => currentState.Value;

        public bool CanMove => CurrentState == PlayerActionState.Normal;
        public bool CanAttack => CurrentState == PlayerActionState.Normal;
        public bool CanUseSkills => CurrentState == PlayerActionState.Normal;
        public bool CanInteract => CurrentState == PlayerActionState.Normal;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                currentState.Value = PlayerActionState.Normal;
        }

        // Para cambios solicitados por el jugador propietario.
        public void RequestLocalState(PlayerActionState newState)
        {
            if (!IsOwner)
                return;

            if (!IsValidLocalState(newState))
                return;

            if (IsServer)
                currentState.Value = newState;
            else
                RequestLocalStateRpc(newState);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void RequestLocalStateRpc(PlayerActionState newState)
        {
            if (!IsValidLocalState(newState))
                return;

            currentState.Value = newState;
        }

        // Para cambios que únicamente puede decidir el servidor.
        public void SetServerState(PlayerActionState newState)
        {
            if (!IsServer)
                return;

            currentState.Value = newState;
        }

        private bool IsValidLocalState(PlayerActionState state)
        {
            return state == PlayerActionState.Normal ||
                   state == PlayerActionState.InMenu ||
                   state == PlayerActionState.InDialogue ||
                   state == PlayerActionState.Interacting;
        }
    }
}
