using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Conecta el sistema de diálogo con las misiones.
    /// </summary>
    public class QuestDialogueBridge : MonoBehaviour
    {
        private NPCIdentity identity;


        private void Awake()
        {
            identity = GetComponent<NPCIdentity>();


            if (identity == null)
            {
                Debug.LogError(
                    "Falta NPCIdentity en " + gameObject.name
                );
            }
        }


        /// <summary>
        /// Llamar cuando termina el diálogo.
        /// </summary>
        public void DialogueFinished()
        {
            if (identity == null)
                return;


            QuestEvents.RaiseTalkToNPC(
                identity.NPCID
            );
        }
    }
}
