using Burmuruk.Tesis.Inventory;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class BaseItemSetting : UnityEditor.Editor, IClearable, IChangesObserver, ISaveable, INameTracker
    {
        protected string _id = null;
        protected string _name = null;
        protected NameSettings _nameControl;
        protected ModificationType _modificationType;

        public event Action<ModificationType, ElementType, string, CreationData> OnCreationModified;

        public TextField TxtName { get; private set; }
        public TextField TxtDescription { get; private set; }
        public ObjectField OfSprite { get; private set; }
        public ObjectField OfPickup { get; private set; }
        public UnsignedIntegerField UfCapacity { get; private set; }
        protected ModificationType CurModificationType
        {
            get => _modificationType;
            set
            {
                if (value == ModificationType.None)
                {
                    _modificationType = value;
                    return;
                }
                else if (value == ModificationType.Rename)
                {
                    _modificationType = ModificationType.Rename;
                    return;
                }
                else if ((_modificationType | ModificationType.Rename) != 0)
                    _modificationType |= value;

                _modificationType = value;
            }
        }

        public virtual void Initialize(VisualElement container, NameSettings nameControl)
        {
            _nameControl = nameControl;
            TxtName = nameControl.TxtName;
            TxtDescription = container.Q<TextField>("txtDescription");
            OfSprite = container.Q<ObjectField>("opSprite");
            OfPickup = container.Q<ObjectField>("opPickup");
            UfCapacity = container.Q<UnsignedIntegerField>("txtCapacity");

            OfSprite.objectType = typeof(Sprite);
            OfPickup.objectType = typeof(Pickup);
        }

        public virtual void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            TxtName.value = data.name;
            _name = data.name;
            TxtDescription.value = data.Description;
            OfSprite.value = data.Sprite;
            OfPickup.value = data.Pickup;
            UfCapacity.value = (uint)data.Capacity;
        }

        public virtual (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var data = new InventoryItem();

            data.Populate(
                TxtName.text,
                TxtDescription.text,
                ItemType.None,
                (Sprite)OfSprite.value,
                (Pickup)OfPickup.value,
                unchecked((int)UfCapacity.value)
                );

            return (data, null);
        }

        public void UpdateDisplayedName()
        {
            TxtName.name = _name;
        }

        public virtual void Clear()
        {
            TxtName.value = "";
            TxtDescription.value = "";
            OfSprite.value = null;
            OfPickup.value = null;
            UfCapacity.value = 0;
        }

        public virtual ModificationType Check_Changes()
        {
            return ModificationType.None;
        }

        public virtual string Save()
        {
            var modificationType = Check_Changes();
            if (modificationType == ModificationType.None) return null;

            var data = GetInfo(null);
            CreationData creationData = new CreationData(TxtName.text, data);

            return SavingSystem.SaveCreation(ElementType.Buff, in _id, in creationData, modificationType);
        }

        public virtual CreationData Load(ElementType type, string id)
        {
            CreationData? result = SavingSystem.Load(type, id);

            if (!result.HasValue)
            {
                _id = id;
                (var item, var args) = ((InventoryItem, ItemDataArgs))result.Value.data;
                UpdateInfo(item, args);
            }

            return result.Value;
        }

        public void Remove_Changes()
        {
            throw new NotImplementedException();
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
