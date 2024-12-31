using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        bool SaveElement(ElementType type, string name, object args, string newName = "")
        {
            if (string.IsNullOrEmpty(newName))
            {
                if (!VerifyName(name)) return false;

                newName = name;
            }
            else if (!VerifyName(newName)) return false;

            charactersLists.creations.TryAdd(type, new());

            if (charactersLists.creations[type].ContainsKey(name))
            {
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

        private T Load_ElementBaseData<T>(ElementType type, string name) where T : InventoryItem
        {
            T data = (T)charactersLists.creations[type][name];

            txtNameCreation.value = data.name;
            CFCreationColor.value = Color.black;


            return data;
        }

        bool VerifyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Notify("Name can't be empty.", BorderColour.Error);
                return false;
            }

            if (IsTheNameUsed(name))
            {
                Notify("The name it's already been used.", BorderColour.Error);
                return false;
            }

            if (!VerifyVariableName(name))
            {
                Notify("Invalid name", BorderColour.Error);
                return false;
            }

            return true;
        }

        private bool SaveChanges_Character(string name, string newName = "")
        {
            characterData.components ??= new();

            var inventory = GetInventory();
            characterData.components[ComponentType.Inventory] = inventory;
            characterData.components[ComponentType.Equipment] = GetEquipment(in inventory);
            var health = infoContainers[infoHealthName].Q<FloatField>().value;
            characterData.components[ComponentType.Health] = health;
            characterData.color = CFCreationColor.value;
            characterData.characterType = (CharacterType)emCharacterType.EnumField.value;
            characterData.stats = basicStats.stats;

            return SaveElement(ElementType.Character, name, characterData, newName);
        }

        private void LoadChanges_Character(string elementName)
        {
            if (!charactersLists.creations[ElementType.Character].ContainsKey(elementName))
            {
                Debug.Log("No existe el elemento deseado");
                return;
            }

            CharacterData data = (CharacterData)charactersLists.creations[ElementType.Character][elementName];

            characterData = data;
            txtNameCreation.value = data.characterName;
            CFCreationColor.value = data.color;
            emCharacterType.EnumField.value = data.characterType;
            LoadInventoryElements(in data);
            infoContainers[infoHealthName].Q<FloatField>().value = (float)data.components[ComponentType.Health];

            characterComponents.RestartValues();

            characterData.components = new();
            if (data.components != null)
            {
                foreach (var key in data.components.Keys)
                {
                    characterComponents.AddElement(key.ToString());
                }
            }

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private void LoadInventoryElements(in CharacterData data)
        {
            var lastElementList = mclInventoryElements;
            Inventory inventory = default;
            int i;

            if (data.components.ContainsKey(ComponentType.Equipment))
            {
                var equipment = (Equipment)data.components[ComponentType.Equipment];
                inventory = equipment.inventory;

                mclEquipmentElements.RestartValues();
                i = 0;

                foreach (var item in equipment.equipment)
                {
                    mclEquipmentElements.AddElement(item.Key.ToString());
                    mclEquipmentElements[i].Toggle.value = item.Value.equipped;
                    mclEquipmentElements[i].EnumField.value = item.Value.place;
                    ++i;
                }
            }
            else if (!data.components.ContainsKey(ComponentType.Inventory))
            {
                return;
            }

            i = 0;
            foreach (var item in inventory.items)
            {
                mclInventoryElements.AddElement(item.Key.ToString());
                mclInventoryElements.ChangeAmount(i++, item.Value);
            }
        }

        private void Load_AbilityData(Ability ability)
        {

        }

        private void Load_WeaponData(Weapon weapon)
        {
            curWeaponData = weapon;
        }

        private void Load_ArmourData(ArmourElement armor)
        {

        }

        private void Load_ConsumableData(ConsumableItem consumable)
        {

        }

        private void Load_BuffData()
        {

        }
    }
}
