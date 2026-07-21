using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Identificador único de un NPC para las misiones.
    /// </summary>
    public class NPCIdentity : MonoBehaviour
    {
        [SerializeField]
        private string npcID;


        public string NPCID => npcID;
    }
}
