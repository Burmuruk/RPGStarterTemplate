using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "ScriptableObjects/Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] public DialogueNode dialogueNode = new();
        [SerializeField] public string id;
        [SerializeField] public List<string> characters;

        public void UpdateDialogue(DialogueNode startNode)
        {
            dialogueNode = startNode;
        }
    }
}
