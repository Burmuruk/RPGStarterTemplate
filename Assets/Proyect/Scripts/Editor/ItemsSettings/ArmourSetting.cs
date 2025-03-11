using Burmuruk.Tesis.Inventory;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ArmourSetting : BaseItemSetting
    {
        public EnumModifierUI EquipmentPlace { get; private set; }

        public override void Initialize(VisualElement container, TextField name)
        {
            base.Initialize(container, name);

            EquipmentPlace = new EnumModifierUI(container, null, EquipmentType.None);
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
