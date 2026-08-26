using System;
using UnityEngine;

namespace Duna.DialogueSystem
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;

        [TextArea(2, 5)]
        public string text;
    }
}
