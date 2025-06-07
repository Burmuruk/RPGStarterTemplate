using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;
using System;
using Burmuruk.Tesis.Editor.Controls;


namespace Burmuruk.Tesis.Editor
{
    public class VariablesAdderUI : IClearable
    {
        const string _elementPath = "Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/Elements/StatElement.uxml";
        Dictionary<string, StatNameData> _statsNames;
        List<string> _headers = new();
        List<StatDataUI> newVariables = new();
        VisualElement target;
        bool _enableApplyRequest = false;

        class StatDataUI
        {
            public Label label;
            public Toggle toggle;
            ModChange data = new();

            public string HeaderName { get => data.Header; set => data.Header = value; }
            public string Oldname { get => data.OldName; set => data.OldName = value; }
            public string NewName { get => data.NewName; set => data.NewName = value; }
            public ModifiableStat Type { get => data.Type; set => data.Type = value; }

        }

        public event Action<bool> OnChange;
        
        public Button ButtonAddStat { get; private set; }
        public VisualElement PMoreOptions { get; private set; }
        public TextField TxtHeader { get; private set; }
        public DropdownField DDFHeader { get; private set; }
        public EnumField EFType { get; private set; }
        public TextField TxtName { get; private set; }
        public Toggle TglEditStat { get; private set; }
        public EnumModifierUI<ModifiableStat> EMStatType { get; private set; }
        public VisualElement ValuesContainer { get; private set; }
        public ComponentsList<ElementStatVariable> VariablesList { get; private set; }

        public VariablesAdderUI(VisualElement container, List<string> headers, Dictionary<string, StatNameData> statsNames)
        {
            _headers = headers;
            _statsNames = statsNames;
            ButtonAddStat = container.Q<Button>("btnCancel");
            PMoreOptions = container.Q<VisualElement>("PNewValueControls");
            TxtHeader = container.Q<TextField>("txtHeader");
            DDFHeader = container.Q<DropdownField>("DDFHeader");
            EFType = container.Q<EnumField>();
            TxtName = container.Q<TextField>("txtName");
            TglEditStat = container.Q<Toggle>("tglEditStat");
            ValuesContainer = container.Q<VisualElement>("componentsConatiner");
            EMStatType = new EnumModifierUI<ModifiableStat>(container.Q<VisualElement>("TypeAdder"));
            VariablesList = new ComponentsList<ElementStatVariable>(container, _elementPath);
            
            Setup_VariablesList();
            EFType.Init(VariableType.Int);
            Setup_DDFHeader();

            EnableContainer(PMoreOptions, false);
            EnableContainer(TxtHeader, false);
            
            TxtHeader.RegisterValueChangedCallback(OnTxtHeaderChanged);
            TxtName.RegisterCallback<KeyUpEvent>(OnKeyUp_TxtName);
            ButtonAddStat.clicked += OnClick_CancelButton;
            TglEditStat.RegisterValueChangedCallback(OnClick_ToggleStat);
            EnableContainer(EMStatType.Container, false);

            ShowElements(false);
        }

        private void OnClick_ToggleStat(ChangeEvent<bool> evt)
        {
            EnableContainer(EMStatType.Container, evt.newValue);
            EMStatType.Value = ModifiableStat.None;
        }

        private void Setup_VariablesList()
        {
            VariablesList.OnElementCreated += SetElementStyle;
            VariablesList.OnElementAdded += SetElementExtraData;
            VariablesList.OnElementRemoved += _ => Enable_AplyButton(VariablesList.EnabledCount - 1);
            this.target = ValuesContainer;
        }

        private void SetElementExtraData(ElementStatVariable element)
        {
            element.LblHeader.text = DDFHeader.value == "New" ? TxtHeader.value : DDFHeader.value;
            element.VariableType = (VariableType)EFType.value;
            element.NameButton.text = TxtName.value;
            element.LblOldName.text = "sin valor";
            element.Modification.text = EMStatType.Value.ToString();

            //if ((StatModificationType)element.Type == StatModificationType.Rename)
            //    Highlight(element.NameButton, true);
            //else
            //    Highlight(element.NameButton, false);

            Enable_AplyButton(VariablesList.Components.Count);
        }

        private void OnClick_CancelButton()
        {
            if (IsDisabled(PMoreOptions))
            {
                ButtonAddStat.text = "Cancel";
                ShowElements(true);
                TxtName.Focus();
            }
            else
            {
                Clear();
            }
        }

        private void Enable_AplyButton(int amount)
        {
            if (amount <= 0 && !_enableApplyRequest)
            {
                OnChange?.Invoke(false);
            }
            else if (amount > 0 || _enableApplyRequest)
            {
                OnChange?.Invoke(true);
            }
        }

        private void SetElementStyle(ElementStatVariable creation)
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
            string nameLower = TxtHeader.value.Trim().ToLower();

            foreach (var choice in DDFHeader.choices)
            {
                if (choice.ToLower() == nameLower)
                {
                    DDFHeader.SetValueWithoutNotify(choice);
                    TxtHeader.SetValueWithoutNotify(choice);
                    EnableContainer(TxtHeader, false);
                    return false;
                }
            }

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
            string name = TxtName.text.Trim().ToLower();

            if (!VerifyVariableName(name))
            {
                Highlight(TxtName, true, BorderColour.Error);
                Notify("Not valid name", BorderColour.Error);
                return false;
            }
            else if (!Verify_AddedNames(name.Trim()))
            {
                Highlight(TxtName, true);
                Notify("Name already added", BorderColour.Error);
                return false;
            }

            Highlight(TxtName, false);
            VariablesList.AddElement(name, EMStatType.Value.ToString());
            DisableNotification();
            return true;
        }

        private bool Verify_AddedNames(string current)
        {
            var lower = current.ToLower().Trim();
            foreach (var element in VariablesList.Components)
            {
                if (IsDisabled(element.element)) break;

                if (element.NameButton.text.ToLower() == lower)
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveExtraValues()
        {
            while (newVariables.Count > 0)
            {
                target.Remove(newVariables[0].label);
                newVariables.RemoveAt(0);
            }
        }

        public List<ModChange> GetInfo()
        {
            List<ModChange> values = new();

            foreach (var element in VariablesList.Components)
            {
                values.Add(new ModChange
                {
                    Header = element.LblHeader.text,
                    Type = (ModifiableStat)element.Type,
                    OldName = element.OldName,
                    NewName = element.NewName,
                    VariableType = element.VariableType,
                });
            }

            return values;
        }

        private void OnKeyUp_TxtName(KeyUpEvent evt)
        {
            bool result = Verify_BaseStatsNames();

            if (evt.keyCode == KeyCode.Return)
            {
                if (!result || !AddVariable()) return;

                TxtName.value = "";
                TxtName.Focus();
            }
        }

        private bool Verify_BaseStatsNames()
        {
            var nameLower = TxtName.text.ToLower();

            foreach (var stat in _statsNames)
            {
                if (stat.Key.ToLower() == nameLower)
                {
                    if (stat.Value.editable)
                    {
                        Highlight(TxtName, true);
                        return true;
                    }
                    else
                    {
                        Highlight(TxtName, true, BorderColour.Error);
                        Notify("Variable name can't be modified", BorderColour.Error);

                        return false;
                    }
                }
            }

            Highlight(TxtName, false);
            DisableNotification();
            return true;
        }

        //private BorderColour GetStatColour(StatModificationType type) =>
        //    type switch
        //    {
        //        StatModificationType.Remove => BorderColour.Error,
        //        StatModificationType.Rename => BorderColour.HighlightBorder,
        //        _ => BorderColour.Approved,
        //    };

        private void ResetValues()
        {
            DDFHeader.SetValueWithoutNotify(_headers.Count > 0 ? _headers[0] : "None");
            TxtHeader.SetValueWithoutNotify("New header value");
            ButtonAddStat.text = "Add basic stat";
            TxtName.SetValueWithoutNotify("Name");
            EFType.value = VariableType.Int;
            TglEditStat.value = false;
            EMStatType.Clear();
        }

        private void ShowElements(bool shouldShow = true)
        {
            EnableContainer(PMoreOptions, shouldShow);
        }

        public void RequestEnable_ApplyButton(bool shouldEnable)
        {
            if (shouldEnable || newVariables.Count > 0)
            {
                OnChange?.Invoke(true);
            } 
            else if (!shouldEnable &&  newVariables.Count <= 0)
            {
                OnChange?.Invoke(false);
            }

            _enableApplyRequest = shouldEnable;
        }

        private void Update_BtnApply()
        {
            if (_enableApplyRequest || newVariables.Count > 0)
            {
                OnChange?.Invoke(true);
            }
            else if (!_enableApplyRequest && newVariables.Count <= 0)
            {
                OnChange?.Invoke(false);
            }
        }

        public void Clear()
        {
            RemoveExtraValues();
            ResetValues();
            Update_BtnApply();
        }
    }

    public class ElementCreation<T> : ElementCreationUI where T : Enum
    {
        private T _type;

        public override Enum Type { get => _type; set => _type = (T)value; }

        public override void SetType(string value)
        {
            _type = (T)Enum.Parse(typeof(T), value);
        }
    }

    //public enum StatModificationType
    //{
    //    None,
    //    Add,
    //    Remove,
    //    Rename
    //}
}
