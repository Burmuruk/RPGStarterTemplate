using Burmuruk.Tesis.Inventory;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ArmourSetting : BaseItemSetting
    {
        private int _changes;

        public EnumModifierUI<EquipmentType> EquipmentPlace { get; private set; }

        public override void Initialize(VisualElement container, NameSettings nameControl)
        {
            base.Initialize(container, nameControl);

            EquipmentPlace = new EnumModifierUI<EquipmentType>(container);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            ArmourElement armour = new ArmourElement();
            var baseInfo = base.GetInfo(args);
            armour.Copy(baseInfo.Item1);

            armour.Populate((EquipmentType)EquipmentPlace.EnumField.value);

            return (armour, null);
        }

        public override ModificationType Check_Changes()
        {
            if ((_nameControl.Check_Changes() & ModificationType.None) == 0)
                CurModificationType = ModificationType.Rename;

            if (_changes != (int)EquipmentPlace.Value)
            {
                CurModificationType = ModificationType.EditData;
                Highlight(EquipmentPlace.EnumField, true);
            }

            return CurModificationType;
        }
    }

    public record ArmourDataArgs : ItemDataArgs
    {
        public EquipmentType EquipmentPlace { get; private set; }

        public ArmourDataArgs(EquipmentType equipmentPlace)
        {
            EquipmentPlace = equipmentPlace;
        }
    }
}
