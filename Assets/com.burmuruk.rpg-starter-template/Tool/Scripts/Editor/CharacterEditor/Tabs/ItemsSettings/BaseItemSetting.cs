using Burmuruk.Tesis.Inventory;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class BaseItemSetting : BaseInfoTracker, ISaveable
    {
        protected string _id = null;
        protected InventoryItem _changes;

        public TextField TxtDescription { get; private set; }
        public ObjectField OfSprite { get; private set; }
        public ObjectField OfPickup { get; private set; }
        public UnsignedIntegerField UfCapacity { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo nameControl)
        {
            _nameControl = nameControl;

            TxtDescription = container.Q<TextField>("txtDescription");
            OfSprite = container.Q<ObjectField>("opSprite");
            OfPickup = container.Q<ObjectField>("opPickup");
            UfCapacity = container.Q<UnsignedIntegerField>("txtCapacity");

            OfSprite.objectType = typeof(Sprite);
            OfPickup.objectType = typeof(Pickup);
            _nameControl.TxtName.RegisterValueChangedCallback((evt) => 
            {
                if (IsActive)
                    TempName = evt.newValue;
            });
        }

        public virtual void UpdateInfo(InventoryItem data, ItemDataArgs args, ItemType type = ItemType.Consumable)
        {
            TempName = data.Name;
            _originalName = data.Name;
            UpdateName();
            TxtDescription.value = data.Description;
            OfSprite.value = data.Sprite;
            OfPickup.value = data.Pickup;
            UfCapacity.value = (uint)data.Capacity;

            _changes ??= new InventoryItem();
            _changes.UpdateInfo(data.Name, data.Description, type, (Sprite)OfSprite.value, (Pickup)OfPickup.value, unchecked((int)UfCapacity.value));
        }

        public virtual (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var data = new InventoryItem();

            data.UpdateInfo(
                _nameControl.TxtName.value,
                TxtDescription.value,
                ItemType.None,
                (Sprite)OfSprite.value,
                (Pickup)OfPickup.value,
                unchecked((int)UfCapacity.value)
                );

            return (data, null);
        }

        public override void Clear()
        {
            TxtDescription.value = "";
            OfSprite.value = null;
            OfPickup.value = null;
            UfCapacity.value = 0;
            CurModificationType = ModificationTypes.None;
            _changes = null;
            _id = null;
            base.Clear();
        }

        protected void ClearItemInfo()
        {
            _changes.UpdateInfo("", "", _changes.Type, null, null, 0);
        }

        public override bool VerifyData()
        {
            return _nameControl.VerifyData();
        }

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_changes == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;

                if (_nameControl.Check_Changes() != ModificationTypes.None)
                    CurModificationType = ModificationTypes.Rename;

                if (TxtDescription.value != _changes.Description)
                    CurModificationType = ModificationTypes.EditData;

                if (OfSprite.value != _changes.Sprite)
                    CurModificationType = ModificationTypes.EditData;

                if (OfPickup.value != _changes.Pickup)
                    CurModificationType = ModificationTypes.EditData;

                if (UfCapacity.value != _changes.Capacity)
                    CurModificationType = ModificationTypes.EditData;

                return CurModificationType;
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public virtual bool Save()
        {
            if (!VerifyData())
            {
                Utilities.UtilitiesUI.Notify("Invalid Data", BorderColour.Error);
                return false;
            }

            try
            {
                CurModificationType = Check_Changes();
                if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
                {
                    Utilities.UtilitiesUI.Notify("No changes were found", BorderColour.HighlightBorder);
                    return false;
                }
                else
                    CurModificationType = ModificationTypes.Add;

                Utilities.UtilitiesUI.DisableNotification();
                var data = GetInfo(null);
                var creationData = new CreationData(_nameControl.TxtName.value, data);

                return SavingSystem.SaveCreation(ElementType.Item, in _id, in creationData, CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public virtual CreationData Load(ElementType type, string id)
        {
            CreationData? result = SavingSystem.Load(type, id);

            if (result.HasValue)
            {
                _id = id;
                (var item, var args) = ((InventoryItem, ItemDataArgs))result.Value.data;
                Set_CreationState(CreationsState.Editing);
                UpdateInfo(item, args);
            }

            return result.Value;
        }

        public override void Load_Changes()
        {
            TempName = _changes.name;
            UpdateName();
            TxtDescription.value = _changes.Description;
            OfSprite.value = _changes.Sprite;
            OfPickup.value = _changes.Pickup;
            UfCapacity.value = (uint)_changes.Capacity;
            CurModificationType = ModificationTypes.None;
        }

        public override void Remove_Changes()
        {
            _changes = null;
            _id = null;
        }
    }

    public interface IClearable
    {
        public abstract void Clear();
    }

    public record ItemDataArgs { }
}
