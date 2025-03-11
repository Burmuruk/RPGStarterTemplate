using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        public event Action<ElementType, object> OnCreationAdded;

        #region Saving
        private bool Create_Settings()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    if (SaveChanges_Character(txtNameCreation.value))
                    {
                        Notify("Character created.", BorderColour.Approved);
                    }
                    else return false;
                    break;

                case ElementType.Buff:
                    ref BuffData buffData = ref curBuffData.buff;
                    SaveElement(ElementType.Buff, txtNameCreation.text, buffData);
                    break;

                case ElementType.Item:
                case ElementType.Weapon:
                case ElementType.Armour:
                case ElementType.Consumable:
                    object creationData = currentSettingTag.type switch
                    {
                        ElementType.Item => settingsElements[ElementType.Item].GetInfo(null).item,
                        ElementType.Weapon => GetBuffsIds(ElementType.Weapon),
                        ElementType.Armour => settingsElements[ElementType.Armour].GetInfo(null).item,
                        ElementType.Consumable => GetBuffsIds(ElementType.Consumable),
                        _ => null
                    };

                    SaveElement(currentSettingTag.type, txtNameCreation.text, creationData);
                    break;

                default: break;
            }

            return true;
        }

        private (InventoryItem, BuffsNamesDataArgs) GetBuffsIds(ElementType type)
        {
            (var item, var args) = settingsElements[type].GetInfo(GetCreatedEnums(ElementType.Buff));
            var buffsNamesArgs = (BuffsNamesDataArgs)args;
            List<string> ids = new();

            foreach (var name in buffsNamesArgs.BuffsNames)
            {
                if (name == BuffAdderUI.INVALIDNAME)
                {
                    ids.Add("");
                }
                else if (charactersLists.GetCreation(base.name, ElementType.Buff, out string id))
                {
                    ids.Add(id);
                }
            }

            return (item, new BuffsNamesDataArgs(ids));
        }

        private CreatedBuffsDataArgs GetCreatedEnums(ElementType type)
        {
            var creations = new List<CreationData>();

            if (!charactersLists.creations.ContainsKey(type)) return new(null);

            foreach (var creation in charactersLists.creations[type])
            {
                creations.Add(creation.Value);
            }

            return new CreatedBuffsDataArgs(creations.ToArray());
        }

        bool SaveElement(ElementType type, string name, object args, string newName = "")
        {
            if (string.IsNullOrEmpty(newName))
            {
                if (!VerifyName(name)) return false;

                newName = name;
            }
            else if (!VerifyName(newName)) return false;

            charactersLists.creations.TryAdd(type, new());
            charactersLists.elements.TryAdd(type, new());

            if (charactersLists.GetCreation(name, type, out string id))
            {
                charactersLists.creations[type].Remove(id);
                charactersLists.elements[type].Remove(name);
            }

            characterData.characterName = newName;
            charactersLists.creations[type].TryAdd(Guid.NewGuid().ToString(), new CreationData(newName, args));
            charactersLists.elements[type].Add(newName);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            OnCreationAdded?.Invoke(type, args);

            SearchAllElements();

            return true;
        }

        private T Load_ElementBaseData<T>(ElementType type, string name) where T : InventoryItem
        {
            if (!charactersLists.GetCreation(name, type, out string id))
                return null;

            T data = (T)charactersLists.creations[type][id].data;

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
        #endregion

        #region Loading
        private void Load_CreationData(int idx, ElementType type)
        {
            string id = "";

            switch (type)
            {
                case ElementType.Character:
                    LoadChanges_Character(creations[idx].NameButton.text);
                    ChangeTab(INFO_CHARACTER_NAME);
                    break;

                case ElementType.Armour:
                case ElementType.Item:
                    ChangeTab(type switch
                    {
                        ElementType.Item => INFO_ITEM_SETTINGS_NAE,
                        _ => INFO_ARMOUR_SETTINGS_NAME,
                    });

                    var item = Load_ElementBaseData<InventoryItem>(type, creations[idx].NameButton.text);

                    if (item is null) break;

                    settingsElements[type].UpdateInfo(item, null);
                    break;

                case ElementType.Buff:
                    ChangeTab(INFO_BUFF_SETTINGS_NAME);

                    Load_BuffData(idx);
                    break;

                case ElementType.Ability:
                    var ability = Load_ElementBaseData<Ability>(ElementType.Ability, creations[idx].NameButton.text);

                    if (ability is null) break;

                    Load_AbilityData(ability);
                    break;

                case ElementType.Weapon:
                    ChangeTab(INFO_WEAPON_SETTINGS_NAME);

                    Load_WeaponData(creations[idx].NameButton.text);
                    break;

                case ElementType.Consumable:
                    ChangeTab(INFO_CONSUMABLE_SETTINGS_NAME);

                    Load_ConsumableData(creations[idx].NameButton.text);
                    break;

                default:
                    return;
            }
        }

        private void LoadChanges_Character(string elementName)
        {
            if (!charactersLists.GetCreation(elementName, ElementType.Character, out string id))
            {
                Debug.Log("No existe el elemento deseado");
                return;
            }

            CharacterData data = (CharacterData)charactersLists.creations[ElementType.Character][id].data;

            characterData = data;
            txtNameCreation.value = data.characterName;
            CFCreationColor.value = data.color;
            basicStats.stats = characterData.stats;
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

        private void Load_BuffData(in int idx)
        {
            const ElementType type = ElementType.Buff;
            Load_ElementBaseData<InventoryItem>(ElementType.Buff, creations[idx].NameButton.text);

            if (!charactersLists.GetCreation(creations[idx].NameButton.text, type, out string id))
                return;

            var bufo = (BuffData)charactersLists.creations[type][id].data;

            curBuffData.buff = bufo;
            EditorUtility.SetDirty(curBuffData);
        }

        private void Load_AbilityData(Ability ability)
        {

        }

        private void Load_WeaponData(string weaponName)
        {
            //CFCreationColor.value = Color.black;
            const ElementType type = ElementType.Weapon;

            if (!charactersLists.GetCreation(weaponName, type, out string id))
                return;

            (var item, var buffsArgs) = ((InventoryItem, BuffsNamesDataArgs))charactersLists.creations[type][id].data;

            SetBuffsNames(item, buffsArgs, type);
        }

        private void Load_ArmourData(ArmourElement armor)
        {

        }

        private void Load_ConsumableData(string consumableName)
        {
            const ElementType type = ElementType.Consumable;

            if (!charactersLists.GetCreation(consumableName, type, out string id))
                return;

            (var item, var buffsArgs) = ((InventoryItem, BuffsNamesDataArgs))charactersLists.creations[type][id].data;

            SetBuffsNames(item, buffsArgs, type);
        }

        private void SetBuffsNames(InventoryItem item, BuffsNamesDataArgs buffsArgs, in ElementType type)
        {
            var buffsNames = GetBuffsIdsNames(buffsArgs.BuffsNames, type);
            var buffsNamesArgs = new BuffsNamesDataArgs(buffsNames);
            settingsElements[type].UpdateInfo(item, buffsNamesArgs);
        }

        private List<string> GetBuffsIdsNames(List<string> ids, ElementType type)
        {
            var list = new List<string>();

            foreach (var id in ids)
            {
                if (id is not null)
                {
                    if (id == "")
                    {
                        list.Add(null);
                    }
                    else if (charactersLists.creations[type].ContainsKey(id))
                    {
                        list.Add(charactersLists.creations[type][id].Name);
                    }
                }
            }

            return list;
        } 
        #endregion
    }
}
