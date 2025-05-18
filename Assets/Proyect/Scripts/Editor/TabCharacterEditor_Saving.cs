using Burmuruk.Tesis.Inventory;
using System;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        #region Saving
        private bool Save_Creation()
        {
            try
            {
                var type = currentSettingTag.type;
                string errorMessage = CreationControls[type].Save();

                Notify(errorMessage, BorderColour.Approved);

                return true;
            }
            catch (InvalidExeption e)
            {
                throw e;
            }
        }

        private T Load_ElementBaseData<T>(ElementType type, string id) where T : InventoryItem
        {
            if (!SavingSystem.Data.creations[type].TryGetValue(id, out CreationData creationData))
                return null;

            T data = (T)creationData.data;

            return data;
        }
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

            CreationControls[type].Load(type, element.Id);

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
}
