using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ComponentsList
    {
        public const string CONTAINER_NAME = "infoComponents";
    }

    public class ComponentsList<T> : ComponentsList, IClearable where T : ElementCreationUI, new()
    {
        List<int> _amounts;
        public Action<int> OnElementClicked = delegate { };

        public Action<T> OnElementCreated = delegate { };
        public Action<T> OnElementAdded = delegate { };
        public Func<IList, string, int?> CreationValidator = null;

        public VisualElement Parent { get; private set; }
        public VisualElement Container { get; private set; }
        public List<T> Components { get; private set; }
        public List<int> Amounts { get => _amounts; }

        public T this[int index]
        {
            get => Components[index];
            set => Components[index] = value;
        }

        public ComponentsList(VisualElement container)
        {
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BasicSS.uss");
            container.styleSheets.Add(styleSheetColour);
            Parent = container;
            Container = container.Q<VisualElement>("componentsConatiner");
            if (Container ==  null)
            {
                Container = container.Q<VisualElement>("elementsContainer");
            }

            _amounts = new();
            Components = new();
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

        public bool AddElement(string name, string type)
        {
            if (!AddNewElement(name, type, out int? componentIdx))
                return false;

            OnElementAdded(Components[componentIdx.Value]);
            return true;
        }

        public bool AddElement(string name)
        {
            if (!AddNewElement(name, name, out int? componentIdx))
                return false;

            OnElementAdded(Components[componentIdx.Value]);
            return true;
        }

        private bool AddNewElement(string name, string type, out int? componentIdx)
        {
            componentIdx = null;
            if (name == "None") return false;

            if (CreationValidator == null)
            {
                componentIdx = DefaultCreationValidator();
            }
            else
            {
                componentIdx = CreationValidator(Components, name);
            }

            if (componentIdx == -1)
            {
                int newIdx = 0;
                CreateNewComponent(name, type, out newIdx);
                componentIdx = newIdx;
            }
            else if (!componentIdx.HasValue)
                return false;

            Components[componentIdx.Value].NameButton.text = name;
            Components[componentIdx.Value].SetType(type);

            if (Components[componentIdx.Value] is ElementComponent)
            {
                var compType = (ComponentType)Components[componentIdx.Value].Type;
                Setup_ComponentButton(compType, componentIdx.Value);

                if (compType == ComponentType.Equipment)
                {
                    var comps = (from c in Components
                                 where (ComponentType)c.Type == ComponentType.Inventory && !c.element.ClassListContains("Disable")
                                 select c).ToArray();

                    if (comps == null || comps.Length == 0)
                    {
                        Debug.Log("no inv");
                        AddElement(ComponentType.Inventory.ToString());
                    }
                }
            }

            return true;
        }

        private int? DefaultCreationValidator()
        {
            int? componentIdx = -1;

            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].element.ClassListContains("Disable"))
                {
                    componentIdx = i;
                    EnableContainer(Components[i].element, true);
                    break;
                }
            }

            return componentIdx;
        }

        protected virtual T CreateNewComponent(string value, string type, out int idx)
        {
            idx = Components.Count;

            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Elements/ElementComponent.uxml");
            StyleSheet basicStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BasicSS.uss");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BorderColours.uss");
            var component = new T();
            component.Initialize(element.Instantiate(), idx);
            component.SetType(type);

            Components.Add(component);
            Container.Add(Components[idx].element);
            Amounts.Add(idx);

            int newIdx = idx;
            component.NameButton.clicked += () => OnElementClicked(newIdx);
            component.element.styleSheets.Add(basicStyle);
            component.element.styleSheets.Add(styleSheet);
            component.element.styleSheets.Add(styleSheetColour);
            StartAmount(component, idx);
            OnElementCreated(component);

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
                    SetClickableButtonColour(componentIdx);
                    break;

                default:
                    if (Components[componentIdx].NameButton.ClassListContains("ClickableBtn"))
                        Components[componentIdx].NameButton.RemoveFromClassList("ClickableBtn");
                    Components[componentIdx].NameButton.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
                    break;
            }
        }

        private void SetClickableButtonColour(int componentIdx)
        {
            Components[componentIdx].NameButton.AddToClassList("ClickableBtn");
            Components[componentIdx].NameButton.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }

        protected virtual void RemoveComponent(int idx)
        {
            if (((ComponentType)Components[idx].Type) == ComponentType.Inventory)
            {
                foreach (var component in Components)
                {
                    if (!component.element.ClassListContains("Disable"))
                    {
                        if ((ComponentType)component.Type == ComponentType.Equipment)
                        {
                            Notify("Equipment requires an Inventory component to store the items", BorderColour.Error);
                            return;
                        }
                    }
                    else
                        break;
                }
            }

            ChangeAmount(idx, 0);
            EnableContainer(Components[idx].element, false);
        }

        public virtual void Clear()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                EnableContainer(Components[i].element, false);
                ChangeAmount(i, 0);
            }
        }
    }
}
