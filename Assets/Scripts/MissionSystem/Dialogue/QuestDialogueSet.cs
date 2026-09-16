using Duna.DialogueSystem;
using UnityEngine;

namespace Duna.QuestSystem
{
    [CreateAssetMenu(
        fileName = "QuestDialogueSet",
        menuName = "Duna/Quest System/Quest Dialogue Set")]
    public class QuestDialogueSet : ScriptableObject
    {
        //================================================//
        // START
        //================================================//

        [Header("Start Dialogue")]

        [SerializeField]
        private DialogueData startDialogue;


        //================================================//
        // IN PROGRESS
        //================================================//

        [Header("In Progress Dialogue")]

        [SerializeField]
        private DialogueData inProgressDialogue;


        //================================================//
        // COMPLETION
        //================================================//

        [Header("Completion Dialogue")]

        [SerializeField]
        private DialogueData completionDialogue;


        //================================================//
        // GETTERS
        //================================================//

        public DialogueData StartDialogue =>
            startDialogue;

        public DialogueData InProgressDialogue =>
            inProgressDialogue;

        public DialogueData CompletionDialogue =>
            completionDialogue;
    }
}
