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
            _nameControl.TxtName.value = data.Name;
            TempName = data.Name;
            TxtDescription.value = data.Description;
            OfSprite.value = data.Sprite;
            OfPickup.value = data.Pickup;
            UfCapacity.value = (uint)data.Capacity;

            _changes ??= new InventoryItem();
            _changes.UpdataInfo(data.Name, data.Description, type, (Sprite)OfSprite.value, (Pickup)OfPickup.value, unchecked((int)UfCapacity.value));
        }

        public virtual (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var data = new InventoryItem();

            data.UpdataInfo(
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
            _changes.UpdataInfo("", "", _changes.Type, null, null, 0);
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return CurModificationType = ModificationTypes.Add;

            CurModificationType = ModificationTypes.None;

            if ((_nameControl.Check_Changes() & ModificationTypes.None) != 0)
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

        public virtual string Save()
        {
            var modificationType = Check_Changes();
            if (modificationType == ModificationTypes.None) return null;

            var data = GetInfo(null);
            CreationData creationData = new CreationData(_nameControl.TxtName.value, data);

            return SavingSystem.SaveCreation(ElementType.Item, in _id, in creationData, modificationType);
        }

        public virtual CreationData Load(ElementType type, string id)
        {
            CreationData? result = SavingSystem.Load(type, id);

            if (result.HasValue)
            {
                _id = id;
                (var item, var args) = ((InventoryItem, ItemDataArgs))result.Value.data;
                UpdateInfo(item, args);
                Set_CreationState(CreationsState.Editing);
            }

            return result.Value;
        }

        public override void Remove_Changes()
        {
            TempName = _changes.name;
            _nameControl.TxtName.value = _changes.Name;
            TxtDescription.value = _changes.Description;
            OfSprite.value = _changes.Sprite;
            OfPickup.value = _changes.Pickup;
            UfCapacity.value = (uint)_changes.Capacity;
            CurModificationType = ModificationTypes.None;
        }

        public override void UpdateName()
        {
            _nameControl.TxtName.value = TempName;
        }
    }

    public interface IClearable
    {
        public abstract void Clear();
    }

    public record ItemDataArgs
    {
        public ItemDataArgs()
        {

        }
    }
}
