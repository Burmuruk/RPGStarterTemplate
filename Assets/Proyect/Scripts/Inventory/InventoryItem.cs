using System;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public class InventoryItem : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Information")]
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [SerializeField] ItemType _type;
        [SerializeField] Sprite m_sprite;
        [SerializeField] Pickup pickup;
        [SerializeField] int m_capacity;
        private int _id = 0;

        public int ID => _id;
        public string Name { get => m_name; }
        public string Description { get => m_description; }
        public Sprite Sprite { get => m_sprite; }
        public ItemType Type { get => _type; }
        public int Capacity { get => m_capacity; }
        public GameObject Prefab { get => pickup.Prefab; }
        public GameObject PrefabInst { get => Instantiate(Prefab); }
        public Pickup Pickup
        {
            get
            {
                return pickup;
            }
        }

        public virtual object GetSubType()
        {
            throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {
            if (_id == 0)
                _id = GetHashCode();
        }
    }
}