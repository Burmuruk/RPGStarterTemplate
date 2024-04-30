using Burmuruk.Tesis.Fighting;
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

        public object Save()
        {
            return gameObject.activeSelf;
        }

        public void Load(object args)
        {
            gameObject.SetActive((bool)args);
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
