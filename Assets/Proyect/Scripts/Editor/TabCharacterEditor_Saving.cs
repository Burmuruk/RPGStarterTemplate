using Burmuruk.Tesis.Inventory;
using System;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        public event Action<ModificationType, ElementType, string, CreationData> OnCreationModified;

        #region Saving
        private bool Save_Creation()
        {
            var type = currentSettingTag.type;
            string errorMessage = settingsElements[type].Save();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Notify("Error al guardar", BorderColour.Error);
                return false;
            }

            Notify("Elemento guardado", BorderColour.Approved);
            
            //switch (type)
            //{
            //    case ElementType.Character:
            //        result = SaveChanges_Character(creationName, id, creationName);
            //        break;

            //    case ElementType.Buff:

            //        newData = CurBuffData.visualizer.buff;
            //        result = Save_CreationData(ElementType.Buff, creationName, ref id, newData);
            //        break;

            //    case ElementType.Item:
            //    case ElementType.Weapon:
            //    case ElementType.Armour:
            //    case ElementType.Consumable:
            //        newData = type switch
            //        {
            //            ElementType.Item => settingsElements[ElementType.Item].GetInfo(null).item,
            //            ElementType.Weapon => GetBuffsIds(ElementType.Weapon),
            //            ElementType.Armour => settingsElements[ElementType.Armour].GetInfo(null).item,
            //            ElementType.Consumable => GetBuffsIds(ElementType.Consumable),
            //            _ => null
            //        };

            //        settingsElements[type].Save();

            //        result = Save_CreationData(type, creationName, ref id, newData);
            //        break;

            //    default: return false;
            //}

            //if (result)
            //{
            //    OnCreationModified?.Invoke(ModificationType.Add, type, id, new CreationData(creationName, newData));
            //    Notify("Succsessfully created.", BorderColour.Approved);
            //}
            //else
            //{
            //    return false;
            //}

            return true;
        }

        private T Load_ElementBaseData<T>(ElementType type, string id) where T : InventoryItem
        {
            if (!charactersLists.creations[type].TryGetValue(id, out CreationData creationData))
                return null;

            T data = (T)creationData.data;

            return data;
        }

        //private bool SaveChanges_Character(string name, string id, string newName = "")
        //{
        //    characterData.components ??= new();

        //    AddCharacterComponents();
        //    characterData.color = CFCreationColor.value;
        //    characterData.characterType = (CharacterType)emCharacterType.EnumField.value;
        //    characterData.stats = basicStats.stats;

        //    return Save_CreationData(ElementType.Character, name, ref id, characterData, newName);
        //}

        //private void AddCharacterComponents()
        //{
        //    var components = from comp in characterComponents.Components
        //                     where !comp.element.ClassListContains("Disable")
        //                     select comp;

        //    foreach (var component in components)
        //    {
        //        switch ((ComponentType)component.Type)
        //        {
        //            case ComponentType.Health:
        //                var health = infoContainers[infoHealthName].Q<FloatField>().value;
        //                characterData.components[ComponentType.Health] = health;
        //                break;

        //            case ComponentType.Inventory:
        //                AddInventoryComponent();
        //                break;

        //            case ComponentType.Equipment:
        //                AddInventoryComponent();

        //                var inventory = (Inventory)characterData.components[ComponentType.Inventory];
        //                characterData.components[ComponentType.Equipment] = GetEquipment(in inventory);
        //                break;

        //            case ComponentType.None:
        //                break;

        //            case ComponentType.Dialogue:
        //                break;

        //            default:
        //                characterData.components[(ComponentType)component.Type] = null;
        //                break;
        //        }
        //    }
        //}

        //private void AddInventoryComponent()
        //{
        //    if (characterData.components.ContainsKey(ComponentType.Inventory))
        //        return;

        //    var inventory = GetInventory();
        //    characterData.components[ComponentType.Inventory] = inventory;
        //} 
        #endregion

        #region Loading
        private void Load_CreationData(ElementCreationPinable element, ElementType type)
        {
            ChangeTab(type switch
            {
                ElementType.Character => INFO_CHARACTER_NAME,
                ElementType.Armour => INFO_ARMOUR_SETTINGS_NAME,
                ElementType.Buff => INFO_BUFF_SETTINGS_NAME,
                ElementType.Weapon => INFO_WEAPON_SETTINGS_NAME,
                ElementType.Consumable => INFO_CONSUMABLE_SETTINGS_NAME,
                _ => INFO_ITEM_SETTINGS_NAME,
            });

            settingsElements[type].Load(type, element.Id);

            //switch (type)
            //{
            //    case ElementType.Character:
            //        LoadChanges_Character(element.Id);
            //        ChangeTab(INFO_CHARACTER_NAME);
            //        break;

            //    case ElementType.Armour:
            //    case ElementType.Item:
            //        ChangeTab(type switch
            //        {
            //            ElementType.Item => INFO_ITEM_SETTINGS_NAME,
            //            _ => INFO_ARMOUR_SETTINGS_NAME,
            //        });

            //        var item = Load_ElementBaseData<InventoryItem>(type, element.Id);

            //        if (item is null) break;

            //        settingsElements[type].UpdateInfo(item, null);
            //        break;

            //    case ElementType.Buff:
            //        ChangeTab(INFO_BUFF_SETTINGS_NAME);

            //        Load_BuffData(element.Id);
            //        break;

            //    case ElementType.Ability:
            //        var ability = Load_ElementBaseData<Ability>(ElementType.Ability, element.Id);

            //        if (ability is null) break;

            //        Load_AbilityData(ability);
            //        break;

            //    case ElementType.Weapon:
            //        ChangeTab(INFO_WEAPON_SETTINGS_NAME);

            //        Load_WeaponData(element.Id);
            //        break;

            //    case ElementType.Consumable:
            //        ChangeTab(INFO_CONSUMABLE_SETTINGS_NAME);

            //        Load_ConsumableData(element.Id);
            //        break;

            //    default:
            //        return;
            //}

            //settingsElements[type].Load();
        }


        #endregion
    }

    [Flags]
    public enum ModificationType
    {
        None,
        Add,
        Remove,
        EditData,
        Rename,
        ColourReasigment
    }
}
