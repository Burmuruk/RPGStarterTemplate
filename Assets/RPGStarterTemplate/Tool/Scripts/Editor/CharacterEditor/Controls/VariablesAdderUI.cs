using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Utilities.VariablesAdder;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;
using System;
using Burmuruk.Tesis.Editor.Controls;


namespace Burmuruk.Tesis.Editor
{
    public class VariablesAdderUI
    {
        List<string> _headers = new();
        List<(Label label, VariableType type)> newVariables = new();
        VisualElement target;

        public Button ButtonAdd { get; private set; }
        public Button ButtonCancel { get; private set; }
        public VisualElement PMoreOptions { get; private set; }
        public TextField TxtHeader { get; private set; }
        public DropdownField DDFHeader { get; private set; }
        public EnumField EFType { get; private set; }
        public TextField TxtName { get; private set; }
        public VisualElement ValuesContainer { get; private set; }
        public ComponentsList<ElementCreation<VariableType>> VariablesList { get; private set; }

        public VariablesAdderUI(VisualElement container, List<string> headers)
        {
            _headers = headers;
            ButtonAdd = container.Q<Button>("btnAddBasicStats");
            ButtonCancel = container.Q<Button>("btnCancel");
            PMoreOptions = container.Q<VisualElement>("PNewValueControls");
            TxtHeader = container.Q<TextField>("txtHeader");
            DDFHeader = container.Q<DropdownField>("DDFHeader");
            EFType = container.Q<EnumField>();
            TxtName = container.Q<TextField>("txtName");
            ValuesContainer = container.Q<VisualElement>("componentsConatiner");
            VariablesList = new ComponentsList<ElementCreation<VariableType>>(container);

            VariablesList.OnElementCreated += SetElementStyle;
            VariablesList.OnElementAdded += _ => Enable_AplyButton(VariablesList.Components.Count);
            VariablesList.OnElementRemoved += _ => Enable_AplyButton(VariablesList.EnabledCount -1);
            this.target = ValuesContainer;
            ButtonAdd.clicked += OnClick_AddButton;
            EFType.Init(VariableType.Int);

            EnableContainer(PMoreOptions, false);
            EnableContainer(TxtHeader, false);
            
            Setup_DDFHeader();
            TxtHeader.RegisterValueChangedCallback(OnTxtHeaderChanged);
            TxtName.RegisterCallback<KeyUpEvent>(OnKeyUp_TxtName);
            ButtonCancel.clicked += () =>
            {
                ShowElements(false);
                RemoveExtraValues();
                ResetValues();
            };

            ShowElements(false);
        }

        private void Enable_AplyButton(int amount)
        {
            ButtonAdd.SetEnabled(amount > 0);
        }

        private void SetElementStyle(ElementCreation<VariableType> creation)
        {
            creation.RemoveButton.clicked += () =>
            {
                VariablesList.RemoveComponent(creation.idx);
            };
        }

        private void OnTxtHeaderChanged(ChangeEvent<string> evt)
        {
            VerifyHeaderName();
        }

        private bool VerifyHeaderName()
        {
            foreach (var choice in DDFHeader.choices)
            {
                if (choice == TxtHeader.value)
                {
                    Highlight(TxtHeader, true, BorderColour.Error);
                    Notify("Header name in use", BorderColour.Error);
                    return false;
                }
            }

            Highlight(TxtHeader, false);
            DisableNotification();
            return true;
        }

        private void Setup_DDFHeader()
        {
            DDFHeader.RegisterValueChangedCallback(OnDDFHederChanged);
            DDFHeader.choices.Clear();
            DDFHeader.choices.Add("New");

            if (_headers.Count > 0)
            {
                DDFHeader.choices.AddRange(_headers);
                DDFHeader.SetValueWithoutNotify(_headers[0]);
            }
            else
            {
                DDFHeader.SetValueWithoutNotify("None");
            }
        }

        private void OnDDFHederChanged(ChangeEvent<string> evt)
        {
            bool enable = evt.newValue == "New";
            EnableContainer(TxtHeader, enable);

            if (enable)
            {
                TxtHeader.SetValueWithoutNotify("New header");
            }
            else
                TxtHeader.SetValueWithoutNotify(evt.newValue);
        }

        public bool AddVariable()
        {
            if (!VerifyVariableName(TxtName.value))
            {
                Highlight(TxtName, true, BorderColour.Error);
                Notify("Nombre no v�lido", BorderColour.Error);
                return false;
            }

            Highlight(TxtName, false);
            VariablesList.AddElement(TxtName.value, EFType.value.ToString());
            DisableNotification();

            return true;
        }

        public void RemoveExtraValues()
        {
            while (newVariables.Count > 0)
            {
                target.Remove(newVariables[0].label);
                newVariables.RemoveAt(0);
            }

            ShowElements(false);
        }

        public void ApplyChanges()
        {
            string path = Path.GetDirectoryName(typeof(BasicStats).Assembly.Location);
            VariablesAdder adder = new(path);
            Debug.Log(path);

            List<(VariableType type, string name)> values = new();

            foreach ((Label label, VariableType type) value in newVariables)
            {
                values.Add((value.type, value.label.text));
            }

            if (!adder.Modify(path, values.ToArray(), out string error))
            {
                //notify
            }

            RemoveExtraValues();
            ResetValues();
            ShowElements(false);
        }

        private void OnKeyUp_TxtName(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                if (!AddVariable()) return;

                TxtName.value = "";
                TxtName.Focus();
            }
        }

        private void OnClick_AddButton()
        {
            if (pNotification.ClassListContains("Disable"))
            {
                ButtonAdd.text = "Apply changes";
                ShowElements(true);
                TxtName.Focus();
            }
            else
            {
                ApplyChanges();
                ResetValues();

                ShowElements(false);
            }
        }

        private void ResetValues()
        {
            DDFHeader.SetValueWithoutNotify(_headers.Count > 0 ? _headers[0] : "None");
            TxtHeader.SetValueWithoutNotify("New header value");
            ButtonAdd.text = "Add basic stat";
            TxtName.value = "Name";
            EFType.value = VariableType.Int;
        }

        private void ShowElements(bool shouldShow = true)
        {
            ButtonAdd.SetEnabled(!shouldShow);
            EnableContainer(PMoreOptions, shouldShow);
            //EnableContainer(EFType, shouldShow);
            //EnableContainer(TxtName, shouldShow);
            EnableContainer(ButtonCancel, shouldShow);
        }
    }

    public class ElementCreation<T> : ElementCreationUI where T : Enum
    {
        private T _type;

        public override Enum Type { get; set; }

        public override void SetType(string value)
        {
            _type = (T)Enum.Parse(typeof(T), value);
        }
    }
}
