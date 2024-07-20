using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class PickableItem : MonoBehaviour, ISaveable
    {
        public ItemType itemType;
        [SerializeField] int itemId;
        int id;

        public ItemType Type => ItemType.Consumable;
        public bool IsPersistentData => false;

        public int ID => id;

        private void Awake()
        {
            id = GetHashCode();
        }

        public object CaptureState()
        {
            return gameObject.activeSelf;
        }

        public void RestoreState(object args)
        {
            gameObject.SetActive((bool)args);
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
