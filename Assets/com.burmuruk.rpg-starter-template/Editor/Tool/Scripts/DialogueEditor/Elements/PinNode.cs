using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
	public class PinNode
	{
        private VisualElement _container;
        public string nodeId;
        public string note;
        public VisualElement Element { get; private set; }

        public void Initialize(VisualElement container, string id)
        {
            this._container = container;
            this.nodeId = id;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/DialogueEditor/Elements/Node.uxml");
            var instance = visualTree.Instantiate();
            this.Element = instance;
            _container.Add(instance);
        }
    } 
}
