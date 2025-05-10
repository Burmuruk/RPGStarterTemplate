using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class NameSettings : UnityEditor.Editor, IChangesObserver, IClearable
    {
        const string TXT_CREATION_NAME = "txtName";
        const string CREATION_COLOUR_NAME = "cfSettingColour";
        string _lastName;
        Color _lastColour;

        public TextField TxtName { get; private set; }
        public ColorField TxtColour { get; private set; }

        public NameSettings(VisualElement container)
        {
            TxtName = container.Q<TextField>(TXT_CREATION_NAME);
            TxtColour = container.Q<ColorField>(CREATION_COLOUR_NAME);

            TxtName.RegisterCallback<KeyUpEvent>(OnKeyUp_txtNameCreation);
            TxtColour.RegisterValueChangedCallback(OnValueChanged_CFCreationColour);

            TxtName.value = name;
        }

        public NameSettings(VisualElement container, Vector4 colour) : this(container)
        {
            TxtColour.value = colour;

            
        }

        private void OnValueChanged_CFCreationColour(ChangeEvent<Color> evt)
        {
            //characterData.color = evt.newValue;
            //((CharacterData)editingData[ElementType.Character].data).color = evt.newValue;
        }

        private void OnKeyUp_txtNameCreation(KeyUpEvent evt)
        {
            if (!VerifyName(TxtName.value))
            {
                Notify("Nombre no válido", BorderColour.Error);
                return;
            }
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

        public ModificationType Check_Changes()
        {
            ModificationType modification = default;

            if (_lastName != TxtName.value)
                modification = ModificationType.Rename;

            if (_lastColour != TxtColour.value)
                modification |= ModificationType.Rename;

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
