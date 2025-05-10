using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class EnumModifierUI<T> : IClearable where T : Enum
    {
        public const string ContainerName = "EnumModifier";
        EnumEditor enumEditor = new();
        string path = null;
        State state = State.None;

        enum State
        {
            None,
            Adding,
            Editing,
            Removing
        }

        public VisualElement Container { get; private set; }
        public Label Name { get; private set; }
        public Button BtnAddValue { get; private set; }
        public Button BtnRemoveValue { get; private set; }
        public Button BtnEditValue { get; private set; }
        public EnumField EnumField { get; private set; }
        public TextField TxtNewValue { get; private set; }
        public VisualElement EnumContainer { get; private set; }
        public VisualElement NewValueContainer { get; private set; }
        private State CurrentState 
        { 
            get => state;
            set
            {
                HighlightButton(false);
                state = value;
                HighlightButton(true);
            }
        }

        public EnumModifierUI(VisualElement container)
        {
            this.Container = container;
            BtnEditValue = container.Q<Button>("btnEditValue");
            BtnRemoveValue = container.Q<Button>("btnRemoveValue");
            BtnAddValue = container.Q<Button>("btnAddValue");
            EnumField = container.Q<EnumField>();
            TxtNewValue = container.Q<TextField>();
            EnumContainer = container.Q<VisualElement>("EnumLine");
            Name = EnumContainer.Q<Label>("lblName");
            NewValueContainer = container.Q<VisualElement>("NewElementLine");

            BtnEditValue.clicked += OnClick_EditValue;
            BtnRemoveValue.clicked += OnClick_RemoveValue;
            BtnAddValue.clicked += () => OnClick_AddButton();
            EnumField.Init(default(T));
            TxtNewValue.RegisterCallback<KeyUpEvent>(OnKeyUp_TxtCharacterType);

            //path
            path = "";

            EnableContainer(NewValueContainer, false);
        }

        public T Value => (T)EnumField.value;

        private void OnClick_EditValue()
        {
            if (EnumField.text == "None") return;

            bool shouldShow = CurrentState != State.Editing;

            if (shouldShow)
            {
                BtnEditValue.text = "^";
                ShowElements(true);
                CurrentState = State.Editing; 
            }
            else
            {
                BtnEditValue.text = "Edit";
                ShowElements(false);
                CurrentState = State.None;
            }
        }

        private void OnClick_RemoveValue()
        {
            if (EnumField.text == "None") return;

            ShowElements(false);

            if (!enumEditor.RemoveOption(path, EnumField.text, out string error))
            {
                Notify(error, BorderColour.Error); 
                return;
            }

            CurrentState = State.None;
        }

        private void OnClick_AddButton()
        {
            bool shouldShow = CurrentState != State.Adding;

            if (shouldShow)
            {
                BtnAddValue.text = "^";
                ShowElements(true);
                CurrentState = State.Adding; 
            }
            else
            {
                BtnAddValue.text = "+";
                ShowElements(false);
                CurrentState = State.None;
            }
        }

        private void OnKeyUp_TxtCharacterType(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                if (!VerifyVariableName(TxtNewValue.value))
                    return;

                switch (CurrentState)
                {
                    case State.Adding:
                        if (!Add_EnumValue("CharacterType")) return;

                        break;

                    case State.Editing:
                        if (!enumEditor.Rename(path, EnumField.text, TxtNewValue.text, out string error))
                        {
                            Notify(error, BorderColour.Error);
                            return;
                        }
                        break;

                    default:
                        return;
                }

                ShowElements(false);
                EnumField.SetValueWithoutNotify(CharacterType.None);
                CurrentState = State.None;
            }
        }

        private bool Add_EnumValue(string EnumName)
        {
            string error = "";
            //enumEditor.Modify(EnumName, new string[] { emCharacterType.TxtNewValue.value }, "Path", out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Notify(error, BorderColour.Error);
                return false;
            }

            Notify("Value added", BorderColour.Approved);
            return true;
        }

        private void ShowElements(bool shouldShow = true)
        {
            EnumField.SetEnabled(!shouldShow);
            BtnAddValue.SetEnabled(!shouldShow);
            BtnRemoveValue.SetEnabled(!shouldShow);
            EnableContainer(NewValueContainer, shouldShow);
        }

        private void HighlightButton(bool shouldHighlight)
        {
            Button button = state switch
            {
                State.Adding => BtnAddValue,
                State.Editing => BtnEditValue,
                _ => null
            };

            if (button == null) return;

            Highlight(button, shouldHighlight);
        }

        public virtual void Clear()
        {
            state = State.None;
            ShowElements(false);
            HighlightButton(false);
        }

        public void OnDataChanged(in string[] newValues)
        {
            
        }
    }
}
