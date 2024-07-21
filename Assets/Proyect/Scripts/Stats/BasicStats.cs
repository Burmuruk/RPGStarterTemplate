using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [Serializable]
    public struct BasicStats
    {
        [Header("Status")]
        [SerializeField] bool initialized;
        [Space(20), Header("Basic stats")]
        [SerializeField] public float speed;
        [SerializeField] int damage;
        [SerializeField] public float damageRate;
        [SerializeField] public Color color;
        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
        [SerializeField] float minDistance;
        [SerializeField] List<Slot> m_slots;

        [Serializable]
        public struct Slot
        {
            public ItemType type;
            public int maxAmount;

            public Slot(ItemType type, int amount)
            {
                this.type = type;
                this.maxAmount = amount;
            }
        }

        Dictionary<ItemType, int> slots;

        public bool Initilized { get => initialized; }
        public float Speed { get => speed; }
        public int Damage { get => damage; }
        public float DamageRate { get =>  damageRate; }
        public Color Color { get => color; set => color = value; }
        public float MinDistance { get => minDistance; }

        public void Initialize()
        {
            slots = new Dictionary<ItemType, int>();

            foreach (var slot in m_slots)
            {
                slots.Add(slot.type, slot.maxAmount);
            }
        }

        public int GetSlots(ItemType type)
        {
            if (!initialized)
            {
                Initialize();
            }

            return slots[type];
        }

        public Dictionary<ItemType, int> GetAllSlots()
        {
            if (!initialized)
            {
                Initialize();
            }

            return slots;
        }
    }
}
