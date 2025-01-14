using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BaseItemSetting : UnityEditor.Editor
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
            OfPickup.objectType = typeof(GameObject);
        }

        public virtual object GetInfo(object args)
        {
            return null;
        }
    }
}
