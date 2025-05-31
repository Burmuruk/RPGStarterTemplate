using Burmuruk.Tesis.Inventory;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ArmourSetting : BaseItemSetting
    {
        public EnumModifierUI<EquipmentType> EquipmentPlace { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo nameControl)
        {
            base.Initialize(container, nameControl);

            EquipmentPlace = new EnumModifierUI<EquipmentType>(container);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args, ItemType type = ItemType.Armor)
        {
            _changes = new ArmourElement();
            base.UpdateInfo(data, args, type);
            var armour = data as ArmourElement;

            if (armour == null) return;

            EquipmentPlace.Value = (EquipmentType)armour.GetEquipLocation();
            (_changes as ArmourElement).UpdateInfo(EquipmentPlace.Value);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            ArmourElement armour = new ArmourElement();
            var baseInfo = base.GetInfo(args);
            armour.Copy(baseInfo.Item1);

            armour.UpdateInfo((EquipmentType)EquipmentPlace.EnumField.value);

            return (armour, null);
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return CurModificationType = ModificationTypes.Add;

            base.Check_Changes();
            var location = (EquipmentType)(_changes as ArmourElement).GetEquipLocation();

            if (location != EquipmentPlace.Value)
            {
                CurModificationType = ModificationTypes.EditData;
                Highlight(EquipmentPlace.EnumField, true);
            }

            return CurModificationType;
        }

        public override bool Save()
        {
            var modificationType = Check_Changes();
            if (modificationType == ModificationTypes.None) return false;

            var data = GetInfo(null);
            CreationData creationData = new CreationData(_nameControl.TxtName.value, data);

            return SavingSystem.SaveCreation(ElementType.Armour, in _id, in creationData, modificationType);
        }

        public override void Clear()
        {
            base.Clear();

            EquipmentPlace.Value = EquipmentType.None;
            _changes = null;
        }

        public override void Remove_Changes()
        {
            base.Remove_Changes();

            var changes = _changes as ArmourElement;
            EquipmentPlace.Value = (EquipmentType)changes.GetEquipLocation();
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
