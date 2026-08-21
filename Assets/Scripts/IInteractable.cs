using UnityEngine;

namespace Duna.InteractionSystem
{
    public interface IInteractable
    {
        void Interact(
            GameObject player
        );
    }
}