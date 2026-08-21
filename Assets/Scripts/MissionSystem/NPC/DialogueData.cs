using UnityEngine;

namespace Duna.DialogueSystem
{
    [CreateAssetMenu(
        fileName = "Dialogue",
        menuName = "Duna/Dialogue/Dialogue Data"
    )]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue")]
        public string dialogueID;

        public DialogueLine[] lines;
    }
}