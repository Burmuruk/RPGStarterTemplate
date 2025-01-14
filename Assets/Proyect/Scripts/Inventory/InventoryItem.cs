using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Inventory/IntemsList", order = 1)]
    public class InventoryItem : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Information")]
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [SerializeField] ItemType _type;
        [SerializeField] Sprite m_sprite;
        [SerializeField] Pickup pickup;
        [SerializeField] int m_capacity;
        [SerializeField] private int _id = 0;

        private static List<int> m_itemsIds;

        public int ID
        {
            get
            {
                if (_id == 0)
                {
                    _id = GetHashCode();
                    m_itemsIds ??= new();
                    if (!m_itemsIds.Contains(_id))
                    {
                        m_itemsIds.Add(_id);
                    }
                    else
                    {
                        while (!m_itemsIds.Contains(_id))
                            _id += 1;

                        m_itemsIds.Add(_id);
                    }
                }

#if UNITY_EDITOR
                SerializedObject serializedObject = new(this);
                SerializedProperty property = serializedObject.FindProperty("_id");
                serializedObject.ApplyModifiedProperties(); 
#endif

                return _id;
            }
        }
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

        public virtual void Populate(string name, string description, ItemType type, 
            Sprite sprite, Pickup pickup, int capacity, object args)
        {
            m_name = name;
            m_description = description;
            _type = type;
            m_sprite = sprite;
            this.pickup = pickup;
            m_capacity = capacity;
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_id == 0)
            {
                _id = GetHashCode();
                m_itemsIds ??= new();
                if (!m_itemsIds.Contains(_id))
                {
                    m_itemsIds.Add(_id);
                }
                else
                {
                    while (!m_itemsIds.Contains(_id))
                        _id += 1;

                    m_itemsIds.Add(_id);
                }
            }

            //EditorUtility.SetDirty(this);
#endif
        }
    }
}