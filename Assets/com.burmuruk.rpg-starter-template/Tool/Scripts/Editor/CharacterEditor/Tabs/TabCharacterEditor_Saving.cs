using Burmuruk.RPGStarterTemplate.Editor.Utilities;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        private bool Save_Creation()
        {
            UtilitiesUI.DisableNotification();
            try
            {
                var type = currentSettingTag.type;
                return CreationControls[type].Save();
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        private void Load_CreationData(ElementCreationPinnable element, ElementType type)
        {
            UtilitiesUI.DisableNotification();
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
        }
    }
}
