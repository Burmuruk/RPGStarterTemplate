using Burmuruk.Tesis.Inventory;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BaseItemSetting : UnityEditor.Editor, IClearable
    {
        public TextField TxtName { get; private set; }
        public TextField TxtDescription { get; private set; }
        public ObjectField OfSprite { get; private set; }
        public ObjectField OfPickup { get; private set; }
        public UnsignedIntegerField UfCapacity { get; private set; }

        public virtual void Initialize(VisualElement container, TextField name)
        {
            TxtName = name;
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

        public virtual void Clear()
        {
            TxtName.value = "";
            TxtDescription.value = "";
            OfSprite.value = null;
            OfPickup.value = null;
            UfCapacity.value = 0;
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
