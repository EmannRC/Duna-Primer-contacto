using Duna.QuestSystem;
using UnityEngine;

namespace Duna.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;


        [Header("UI")]

        [SerializeField]
        private DialogueUI dialogueUI;


        private DialogueData currentDialogue;


        private DialogueController currentNPC;


        private int currentLineIndex;


        private bool isDialogueActive;


        public bool IsDialogueActive =>
            isDialogueActive;


        //================================================//
        // AWAKE
        //================================================//

        private void Awake()
        {
            if (
                Instance != null &&
                Instance != this
            )
            {
                Destroy(gameObject);

                return;
            }


            Instance =
                this;
        }


        //================================================//
        // START DIALOGUE
        //================================================//

        public void StartDialogue(
            DialogueData dialogue,
            DialogueController npc
        )
        {
            if (
                dialogue == null
            )
            {
                Debug.LogError(
                    "DialogueData inválido."
                );


                return;
            }


            if (
                npc == null
            )
            {
                Debug.LogError(
                    "DialogueController inválido."
                );


                return;
            }


            if (
                dialogueUI == null
            )
            {
                Debug.LogError(
                    "DialogueUI no está asignado."
                );


                return;
            }


            if (
                isDialogueActive
            )
            {
                return;
            }


            currentDialogue =
                dialogue;


            currentNPC =
                npc;


            currentLineIndex =
                0;


            isDialogueActive =
                true;


            dialogueUI.Open();


            ShowCurrentLine();
        }


        //================================================//
        // SHOW CURRENT LINE
        //================================================//

        private void ShowCurrentLine()
        {
            if (
                currentDialogue == null
            )
            {
                EndDialogue();

                return;
            }


            if (
                currentDialogue.lines == null ||
                currentDialogue.lines.Length == 0
            )
            {
                EndDialogue();

                return;
            }


            if (
                currentLineIndex >=
                currentDialogue.lines.Length
            )
            {
                EndDialogue();

                return;
            }


            DialogueLine line =
                currentDialogue.lines[
                    currentLineIndex
                ];


            dialogueUI.ShowLine(
                line.speakerName,
                line.text
            );
        }


        //================================================//
        // ADVANCE DIALOGUE
        //================================================//

        public void AdvanceDialogue()
        {
            if (
                !isDialogueActive
            )
            {
                return;
            }


            if (
                dialogueUI.IsTyping
            )
            {
                dialogueUI.CompleteTyping();

                return;
            }


            currentLineIndex++;


            ShowCurrentLine();
        }


        //================================================//
        // END DIALOGUE
        //================================================//

        public void EndDialogue()
        {
            if (
                !isDialogueActive
            )
            {
                return;
            }


            isDialogueActive =
                false;


            dialogueUI.Close();


            //============================================//
            // AVISAR AL NPC
            //============================================//

            if (
                currentNPC != null
            )
            {
                currentNPC.DialogueFinished();
            }


            currentDialogue =
                null;


            currentNPC =
                null;


            currentLineIndex =
                0;
        }
    }
}
