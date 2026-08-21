using TMPro;
using UnityEngine;
using System.Collections;

namespace Duna.DialogueSystem
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject dialoguePanel;


        [Header("Text")]
        [SerializeField]
        private TextMeshProUGUI speakerNameText;


        [SerializeField]
        private TextMeshProUGUI dialogueText;


        [Header("Continue")]
        [SerializeField]
        private GameObject continueIndicator;


        [Header("Typewriter")]
        [SerializeField]
        private float characterDelay =
            0.03f;


        private Coroutine typingCoroutine;


        private string currentText;


        public bool IsTyping =>
            typingCoroutine != null;


        private void Awake()
        {
            // El panel debe empezar oculto. Esto evita que un panel vacío
            // quede visible si la escena lo dejó activo por accidente.
            Close();
        }


        //================================================//
        public void Open()
        {
            if (dialoguePanel == null)
            {
                Debug.LogError("DialogueUI no tiene panel asignado.");
                return;
            }

            dialoguePanel.SetActive(
                true
            );


            if (
                continueIndicator != null
            )
            {
                continueIndicator.SetActive(
                    false
                );
            }
        }


        //================================================//
        public void Close()
        {
            if (
                typingCoroutine != null
            )
            {
                StopCoroutine(
                    typingCoroutine
                );


                typingCoroutine =
                    null;
            }


            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }


        //================================================//
        public void ShowLine(
            string speakerName,
            string text)
        {
            if (speakerNameText == null || dialogueText == null)
            {
                Debug.LogError("DialogueUI no tiene los textos asignados.");
                return;
            }

            speakerNameText.text =
                speakerName ?? string.Empty;


            currentText = text ?? string.Empty;


            dialogueText.text =
                "";


            if (
                continueIndicator != null
            )
            {
                continueIndicator.SetActive(
                    false
                );
            }


            typingCoroutine =
                StartCoroutine(
                    TypeText()
                );
        }


        //================================================//
        private IEnumerator TypeText()
        {
            foreach (
                char character
                in currentText
            )
            {
                dialogueText.text +=
                    character;


                yield return new WaitForSeconds(
                    characterDelay
                );
            }


            typingCoroutine =
                null;


            if (
                continueIndicator != null
            )
            {
                continueIndicator.SetActive(
                    true
                );
            }
        }


        //================================================//
        public void CompleteTyping()
        {
            if (
                typingCoroutine == null
            )
            {
                return;
            }


            StopCoroutine(
                typingCoroutine
            );


            typingCoroutine =
                null;


            dialogueText.text =
                currentText;


            if (
                continueIndicator != null
            )
            {
                continueIndicator.SetActive(
                    true
                );
            }
        }
    }
}
