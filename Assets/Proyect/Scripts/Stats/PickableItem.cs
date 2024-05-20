using Burmuruk.Tesis.Saving;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class PickableItem : MonoBehaviour, ISaveableItem, ISaveable
    {
        [SerializeField] ItemsList list;
        public ItemType itemType;
        [SerializeField] int itemIdx;
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        int id;

        public ItemType Type => ItemType.Consumable;
        public bool IsPersistentData => false;

        public int ID => id;

        private void Awake()
        {
            id = GetHashCode();
        }

        public int GetSubType()
        {
            return itemIdx;
        }

        public string GetName()
        {
            return m_name;
        }

        public string GetDescription()
        {
            return m_description;
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
