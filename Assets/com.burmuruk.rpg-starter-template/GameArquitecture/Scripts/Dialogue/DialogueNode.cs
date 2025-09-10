using System;
using System.Collections.Generic;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    [Serializable]
    public class DialogueNode
    {
        public string Id;
        public string Message;
        public List<string> Children = new List<string>();
    }
}
