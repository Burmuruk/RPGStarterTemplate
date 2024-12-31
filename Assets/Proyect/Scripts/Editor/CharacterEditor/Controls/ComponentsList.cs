using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BaseLevelEditor;

namespace Burmuruk.Tesis.Editor
{
    public class ComponentsList
    {
        public const string CONTAINER_NAME = "infoComponents";
    }

    public class ComponentsList<T> : ComponentsList where T : ElementCreationUI, new()
    {
        List<int> _amounts;
        protected Action<string, BorderColour> notifyCallback;
        public Action<int> bindElementBtn = delegate { };
        
        public VisualElement Parent { get; private set; }
        public VisualElement Container { get; private set; }
        public List<T> Components { get; private set; }
        public List<int> Amounts { get => _amounts; }

        public T this[int index]
        {
            get => Components[index];
            set => Components[index] = value;
        }

        public ComponentsList(VisualElement container, Action<string, BorderColour> notifyCallback)
        {
            Parent = container;
            Container = container.Q<VisualElement>("componentsConatiner");

            _amounts = new();
            Components = new();
            this.notifyCallback = notifyCallback;

        }

        public void IncrementElement(int idx, bool shouldIncrement = true, int value = 1)
        {
            _amounts[idx] += shouldIncrement ? value : -value;
            Components[idx].IFAmount.value = _amounts[idx];
        }

        public bool ChangeAmount(int idx, int amount)
        {
            if (amount < 0)
            {
                Components[idx].IFAmount.value = _amounts[idx];
                return false;
            }

            _amounts[idx] = amount;
            Components[idx].IFAmount.value = amount;

            return true;
        }

        public void StartAmount(T element, int idx)
        {
            element.IFAmount.value = 1;
            _amounts[idx] = 1;
        }

        public void Disable_CharacterComponents() =>
            Components.ForEach(c => EnableContainer(c.element, false));

        public void RestartValues()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Amounts[i] = 0;
                EnableContainer(Components[i].element, false);
            }
        }

        protected bool Check_HasCharacterComponent(string value)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (!Components[i].element.ClassListContains("Disable") && Components[i].NameButton.text.Contains(value))
                    return true;
            }

            return false;
        }

        public void AddElement(string value)
        {
            int componentIdx = -1;

            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].element.ClassListContains("Disable"))
                {
                    componentIdx = i;
                    EnableContainer(Components[i].element, true);
                    break;
                }
            }

            if (componentIdx == -1)
                CreateNewComponent(value, out componentIdx);

            Components[componentIdx].NameButton.text = value;

            if (Components[componentIdx] is ElementComponent)
            {
                var compType = (ComponentType)Components[componentIdx].Type;
                Setup_ComponentButton(compType, componentIdx);

                if (compType == ComponentType.Equipment && !(from comp in Components where ((ComponentType)comp.Type) == ComponentType.Inventory select comp).Any())
                {
                    AddElement(ComponentType.Inventory.ToString());
                }
            }
        }

        protected virtual T CreateNewComponent(string value, out int idx)
        {
            idx = Components.Count;

            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Elements/ElementComponent.uxml");
            var component = new T();
            component.Initialize(element.Instantiate(), idx);

            Components.Add(component);
            Container.Add(Components[idx].element);
            Amounts.Add(idx);
            StartAmount(component, idx);
            return component;
        }

        private void Setup_ComponentButton(ComponentType type, int componentIdx)
        {
            switch (type)
            {
                case ComponentType.Inventory:
                    goto case ComponentType.Health;

                case ComponentType.Equipment:
                    goto case ComponentType.Health;

                case ComponentType.Health:
                    //SetClickableButtonColour(componentIdx);
                    break;

                default:
                    if (Components[componentIdx].NameButton.ClassListContains("ClickableBtn"))
                        Components[componentIdx].NameButton.RemoveFromClassList("ClickableBtn");
                    Components[componentIdx].NameButton.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
                    break;
            }
        }

        protected virtual void RemoveComponent(int idx)
        {
            if ((ComponentType)Components[idx].Type == ComponentType.Inventory &&
                (from comp in Components where (ComponentType)comp.Type == ComponentType.Equipment select comp).Any())
            {
                notifyCallback("Equipment requires an Inventory component to store the items", BorderColour.Error);
                return;
            }

            ChangeAmount(idx, 0);
            EnableContainer(Components[idx].element, false);
        }
    }
}
