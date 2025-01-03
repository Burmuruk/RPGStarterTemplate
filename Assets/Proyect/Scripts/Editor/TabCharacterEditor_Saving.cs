using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Linq;
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

            AddCharacterComponents();
            characterData.color = CFCreationColor.value;
            characterData.characterType = (CharacterType)emCharacterType.EnumField.value;
            characterData.stats = basicStats.stats;

            return SaveElement(ElementType.Character, name, characterData, newName);
        }

        private void AddCharacterComponents()
        {
            var components = from comp in characterComponents.Components
                             where !comp.element.ClassListContains("Disable")
                             select comp;

            foreach (var component in components)
            {
                switch ((ComponentType)component.Type)
                {
                    case ComponentType.Health:
                        var health = infoContainers[infoHealthName].Q<FloatField>().value;
                        characterData.components[ComponentType.Health] = health;
                        break;

                    case ComponentType.Inventory:
                        AddInventoryComponent();
                        break;

                    case ComponentType.Equipment:
                        AddInventoryComponent();

                        var inventory = (Inventory)characterData.components[ComponentType.Inventory];
                        characterData.components[ComponentType.Equipment] = GetEquipment(in inventory);
                        break;

                    case ComponentType.None:
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        characterData.components[(ComponentType)component.Type] = null;
                        break;
                }
            }
        }

        private void AddInventoryComponent()
        {
            if (characterData.components.ContainsKey(ComponentType.Inventory))
                return;

            var inventory = GetInventory();
            characterData.components[ComponentType.Inventory] = inventory;
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
            LoadCharacterComponents(in data);
            emCharacterType.EnumField.value = data.characterType;

            characterComponents.DDFElement.value = "None";
        }

        private void LoadCharacterComponents(in CharacterData data)
        {
            characterComponents.RestartValues();

            foreach (var component in data.components)
            {
                switch (component.Key)
                {
                    case ComponentType.None:
                        continue;

                    case ComponentType.Health:
                        infoContainers[INFO_HEALTH_SETTINGS_NAME].Q<FloatField>().value = (float)component.Value;
                        break;
                    
                    case ComponentType.Inventory:
                        LoadInventoryItems((Inventory)component.Value);
                        break;
                         

                    case ComponentType.Equipment:
                        var equipment = (Equipment)component.Value;
                        mclEquipmentElements.RestartValues();

                        foreach (var item in equipment.equipment)
                        {
                            Action<ElementCreation> EditData = (e) =>
                            {
                                e.Toggle.value = item.Value.equipped;
                                e.EnumField.value = item.Value.place;
                            };
                            mclEquipmentElements.OnElementCreated += (e) => EditData(e);
                            mclEquipmentElements.OnElementAdded += (e) => EditData(e);

                            mclEquipmentElements.AddElement(item.Key.ToString());

                            mclEquipmentElements.OnElementCreated -= (e) => EditData(e);
                            mclEquipmentElements.OnElementAdded -= (e) => EditData(e);
                        }
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        break;
                }

                characterComponents.AddElement(component.Key.ToString());
            }
        }

        private void LoadInventoryItems(in Inventory inventory)
        {
            characterComponents.AddElement(ComponentType.Inventory.ToString());
            mclInventoryElements.RestartValues();

            foreach (var item in inventory.items)
            {
                int amount = item.Value;
                Action<ElementCreation> ChangeValue = (e) => mclInventoryElements.ChangeAmount(e._idx, amount);
                mclInventoryElements.OnElementAdded += ChangeValue;
                mclInventoryElements.OnElementCreated += ChangeValue;

                mclInventoryElements.AddElement(item.Key.ToString());

                mclInventoryElements.OnElementAdded -= ChangeValue;
                mclInventoryElements.OnElementCreated -= ChangeValue;
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
