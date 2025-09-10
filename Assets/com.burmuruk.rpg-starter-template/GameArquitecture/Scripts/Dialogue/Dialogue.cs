using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "ScriptableObjects/Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<DialogueNode> nodes = new();
        [SerializeField] Vector2 newNodeOffset = new Vector2(250, 0);

        public void AddNode(string id, string message)
        {
            foreach (var node in nodes)
            {
                if (node.Id == id)
                {
                    return;
                }
            }

            DialogueNode newNode = new DialogueNode
            {
                Id = id,
                Message = message
            };

            nodes.Add(newNode);
        }

        public List<DialogueNode> GetNodes() => nodes;

        public void OnAfterDeserialize()
        {
            
        }

        public void OnBeforeSerialize()
        {
            
        }
    }
}
