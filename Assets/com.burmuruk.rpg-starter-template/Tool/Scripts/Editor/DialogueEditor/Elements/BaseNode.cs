using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public class BaseNode : ScriptableObject
    {
        [SerializeField] public string characterID;
        [SerializeField] bool isPlayerSpeaking = false;
        [SerializeField] string text;
        [SerializeField] List<string> children = new();
        [SerializeField] string onEnterAction;
        [SerializeField] string onExitAction;
        private bool isInfoDisplayed;
        private bool isCreatingNode;
        private bool _isPinned = false;

        public event Action<BaseNode> OnSelected;
        public event Action<BaseNode> OnDeselected;
        public event Action<BaseNode, bool> OnPinned;

        public VisualElement Element { get; private set; }
        public GraphViewNode GraphViewNode { get; private set; }
        public Button BtnPin { get; private set; }
        public string Text
        {
            get => text;
            set
            {
#if UNITY_EDITOR
                if (text != value)
                {
                    Undo.RecordObject(this, "Update Dialogue Text");
                    text = value;
                    EditorUtility.SetDirty(this);
                }
#endif
            }
        }
        public string Id { get; private set; }
        public TextField TxtCharacterId { get; private set; }
        public string Title { get => GraphViewNode.title; set => GraphViewNode.title = value; }
        public bool IsPlayerSpeaking
        {
            get => isPlayerSpeaking;
            set
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Change dialogue speaker");
                isPlayerSpeaking = value;
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public virtual void Initilize(DialogueGraphView graph, Vector2 startPosition)
        {
            GraphViewNode = new GraphViewNode(graph);
            Id = Guid.NewGuid().ToString();
            SetPosition(startPosition);
            BtnPin = GraphViewNode.Q<Button>(GraphViewNode.PIN_BUTTON_NAME);

            GraphViewNode.OnSelect += () => OnSelected?.Invoke(this);
            GraphViewNode.OnDeselected += () => OnDeselected?.Invoke(this);
            BtnPin.clicked += () =>
            {
                _isPinned = !_isPinned;
                BtnPin.style.unityBackgroundImageTintColor = _isPinned ?
                    new Color(0.7960784f, 0.6313726f, 0.1019608f) : 
                    new Color(0.5169811f, 0.5169811f, 0.5169811f);
                OnPinned?.Invoke(this, _isPinned);
            };
        }

        public List<string> GetChildren()
        {
            return children;
        }

#if UNITY_EDITOR
        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            GraphViewNode.SetPosition(new Rect(newPosition, new Vector2(200, 150)));
            EditorUtility.SetDirty(this);
        }

        public void AddChild(string childId)
        {
            Undo.RecordObject(this, "Add dialogue link");
            children.Add(childId);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childId)
        {
            Undo.RecordObject(this, "Remove dialogue link");
            children.Remove(childId);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
