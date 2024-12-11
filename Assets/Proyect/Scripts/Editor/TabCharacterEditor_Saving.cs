using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        bool SaveElement(ElementType type, string name, object args, string newName = "") 
        {
            if (!VerifyName(name, ref newName)) return false;

            charactersLists.creations.TryAdd(type, new());

            if (charactersLists.creations[type].ContainsKey(name)) {
                charactersLists.creations[type].Remove(name);
                charactersLists.elements[type].Remove(name);
            }

            characterData.characterName = newName;
            charactersLists.creations[type].TryAdd(newName, args);
            charactersLists.elements[type].Add(newName);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            SearchAllElements();

            return true;
        }

        bool VerifyName(string name, ref string newName)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (IsTheNameUsed(newName)) return false;

            if (string.IsNullOrEmpty(newName)) newName = name;

            return true;
        }

        private bool SaveChanges_Character(string name, string newName = "") 
        {
            characterData.components ??= new();

            var inventory = GetInventory(mclInventoryElements);
            characterData.components[ComponentType.Inventory] = inventory;
            characterData.components[ComponentType.Equipment] = GetEquipment(in inventory, mclEquipmentElements);

            var health = infoContainers[infoHealthName].Q<FloatField>().value;
            characterData.components[ComponentType.Health] = health;

            return true;
        }

        private void LoadChanges_Character(string elementName)
        {
            if (!charactersLists.creations[ElementType.Character].ContainsKey(elementName)) 
            {
                Debug.Log("No existe el elemento deseado");
                return;
            }

            CharacterData data = (CharacterData)charactersLists.creations[ElementType.Character][elementName];

            txtNameCreation.value = data.characterName;
            CFCreationColor.value = data.color;
            emCharacterType.EnumField.value = data.characterType;
            characterData = data;
            infoContainers[infoHealthName].Q<FloatField>().value = (float)data.components[ComponentType.Health];

            Disable_CharacterComponents();

            characterData.components = new();
            if (data.components != null) {
                foreach (var key in data.components.Keys) {
                    Add_CharacterComponent(key.ToString(), key);
                }
            }

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private void SaveChanges_Equipment()
        {

        }

        private void LoadChanges_Equipment()
        {

        }
    } 
}
