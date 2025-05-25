using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class CreationsBaseInfo : UnityEditor.Editor, IChangesObserver, IClearable
    {
        const string TXT_CREATION_NAME = "txtName";
        const string CREATION_COLOUR_NAME = "cfSettingColour";
        string _lastName;
        Color _lastColour;
        CreationsState _creationsState = CreationsState.Creating;

        public event Action<CreationsState> CreationsStateChanged;

        public Button BtnState { get; private set; }
        public TextField TxtName { get; private set; }
        public ColorField Colour { get; private set; }

        public CreationsBaseInfo(VisualElement container)
        {
            BtnState = container.Q<Button>("btnState");
            TxtName = container.Q<TextField>(TXT_CREATION_NAME);
            Colour = container.Q<ColorField>(CREATION_COLOUR_NAME);

            //TxtName.RegisterCallback<KeyUpEvent>(OnKeyUp_txtNameCreation);
            TxtName.RegisterValueChangedCallback(OnValueChanged_TxtName);
            Colour.RegisterValueChangedCallback(OnValueChanged_CFCreationColour);
            BtnState.clicked += Change_State;
            TxtName.value = name;
            BtnState.text = "Creating";
        }

        private void Change_State()
        {
            if (_creationsState == CreationsState.Creating) return;

            SetState(CreationsState.Creating);
            CreationsStateChanged?.Invoke(_creationsState);
        }

        public void SetState(CreationsState state)
        {
            _creationsState = state;
            BtnState.text = state.ToString();
            BtnState.SetEnabled(state == CreationsState.Editing);
        }

        private void OnValueChanged_TxtName(ChangeEvent<string> evt)
        {
            if (!evt.newValue.VerifyName())
            {
                Highlight(TxtName, true, BorderColour.Error);
            }
            else if (_creationsState != CreationsState.Editing && IsTheNameUsed(evt.newValue))
            {
                Highlight(TxtName, true, BorderColour.Error);
                Notify("Name in use", BorderColour.Error);
            }
            else
            {
                Highlight(TxtName, false);
                DisableNotification();
            }
        }

        private bool ValidateName()
        {
            if (!TxtName.value.VerifyName())
            {
                Highlight(TxtName, true, BorderColour.Error);
                throw new InvalidNameExeption();
            }
            else if (_creationsState == CreationsState.Editing)
            {
                if (IsTheNameUsed(TxtName.value))
                {
                    Highlight(TxtName, true, BorderColour.Error);
                    throw new InvalidNameExeption("Name in use");
                }
            }

            Highlight(TxtName, false);

            return true;
        }

        public CreationsBaseInfo(VisualElement container, Vector4 colour) : this(container)
        {
            Colour.value = colour;
        }

        private void OnValueChanged_CFCreationColour(ChangeEvent<Color> evt)
        {
            //characterData.color = evt.newValue;
            //((CharacterData)editingData[ElementType.Character].data).color = evt.newValue;
        }

        public bool IsTheNameUsed(string name)
        {
            int idx = 0;
            foreach (var creationType in SavingSystem.Data.creations.Keys)
            {
                foreach (var creation in SavingSystem.Data.creations[creationType].Values)
                {
                    if (creation.Name == name)
                    {
                        Notify("There's already an element with the same name.", BorderColour.Error);
                        Debug.Log("Save canceled");
                        return true;
                    }

                    ++idx;
                }
            }

            return false;
        }

        public ModificationTypes Check_Changes()
        {
            ValidateName();

            ModificationTypes modification = ModificationTypes.None;

            if (_lastName != TxtName.value)
                modification = ModificationTypes.Rename;

            if (_lastColour != Colour.value)
                modification |= ModificationTypes.Rename;

            return modification;
        }

        public void Remove_Changes()
        {
            TxtName.value = _lastName;
        }

        public void Clear()
        {
            TxtName.value = "";
            _lastName = "";
        }
    }
}
