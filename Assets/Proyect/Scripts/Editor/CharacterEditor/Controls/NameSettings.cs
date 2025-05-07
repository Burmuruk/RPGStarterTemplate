using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class NameSettings : UnityEditor.Editor, IChangesObserver
    {
        string _lastName;
        Color _lastColour;

        public TextField TxtName { get; private set; }
        public ColorField TxtColour { get; private set; }

        public NameSettings(string name)
        {
            TxtName.value = name;
        }

        public NameSettings(string name, Vector4 colour) : this(name)
        {
            TxtColour.value = colour;
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

        public bool Check_Changes()
        {
            return _lastName != TxtName.value && _lastColour != TxtColour.value;
        }

        public void Remove_Changes()
        {
            throw new System.NotImplementedException();
        }
    }
}
