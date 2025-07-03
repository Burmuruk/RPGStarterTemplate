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
            try
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
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override bool Save()
        {
            if (!VerifyData())
            {
                Utilities.UtilitiesUI.Notify("Invalid Data", BorderColour.Error);
                return false;
            }
            if (!VerifyData()) return false;

            try
            {
                if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
                {
                    Notify("No changes were found", BorderColour.HighlightBorder);
                    return false;
                }
                else
                    CurModificationType = ModificationTypes.Add;

                DisableNotification();
                var (data, _) = GetInfo(null);
                var creationData = new ItemCreationData(_nameControl.TxtName.value.Trim(), data as ArmourElement);

                return SavingSystem.SaveCreation(ElementType.Armour, in _id, creationData, CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override void Clear()
        {
            base.Clear();

            EquipmentPlace.Value = EquipmentType.None;
            _changes = null;
        }

        public override void Load_Changes()
        {
            base.Load_Changes();

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
