using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.Tesis.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<DialogueNode> nodes = new();
        [SerializeField] Vector2 newNodeOffset = new Vector2(250, 0);

        Dictionary<string, DialogueNode> nodeLookup = new();

        private void OnValidate()
        {
            nodeLookup.Clear();

            foreach (DialogueNode node in GetAllNodes())
            {
                nodeLookup[node.name] = node;
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (var childId in parentNode.GetChildren())
            {
                if (nodeLookup.ContainsKey(childId))
                    yield return nodeLookup[childId];
            }
        }

#if UNITY_EDITOR
        public void CreateNode(DialogueNode parent)
        {
            DialogueNode newNode = MakeNode(parent);

            Undo.RegisterCreatedObjectUndo(newNode, "Created dialogue node");
            Undo.RecordObject(this, "Added dialogue node.");
            AddNode(newNode);
        }

        public void DeleteNode(DialogueNode nodeToDelete)
        {
            Undo.RecordObject(this, "Deleted dialogue node");
            nodes.Remove(nodeToDelete);
            OnValidate();
            CleanChildren(nodeToDelete);

            Undo.DestroyObjectImmediate(nodeToDelete);
        }

        private void AddNode(DialogueNode newNode)
        {
            nodes.Add(newNode);
            OnValidate();
        }

        private DialogueNode MakeNode(DialogueNode parent)
        {
            var newNode = CreateInstance<DialogueNode>();
            newNode.name = Guid.NewGuid().ToString();

            if (parent != null)
            {
                parent.AddChild(newNode.name);
                newNode.IsPlayerSpeaking = !newNode.IsPlayerSpeaking;
                newNode.SetPosition(parent.GetRect().position + newNodeOffset);
            }

            return newNode;
        }

        public void CleanChildren(DialogueNode nodeToDelete)
        {
            foreach (var node in GetAllNodes())
            {
                node.RemoveChild(nodeToDelete.name);
            }
        }

        internal IEnumerable<DialogueNode> GetPlayerChildren(DialogueNode currentNode)
        {
            foreach (var node in GetAllChildren(currentNode))
            {
                if (node.IsPlayerSpeaking)
                {
                    yield return node;
                }
            }
        }

        internal IEnumerable<DialogueNode> GetAIChildren(DialogueNode currentNode)
        {
            foreach (var node in GetAllChildren(currentNode))
            {
                if (!node.IsPlayerSpeaking)
                {
                    yield return node;
                }
            }
        }

#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                var newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (var node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
#endif
        }

        public void OnAfterDeserialize()
        {

        }
    }
}
