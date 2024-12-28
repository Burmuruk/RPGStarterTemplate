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
        bool initialized;
        [Space(), Header("Basic stats")]
        [SerializeField] public float speed;
        [SerializeField] int damage;
        [SerializeField] public float damageRate;
        [SerializeField] public Color color;
        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
        [SerializeField] float minDistance;
        [Space(), Header("Equipment")]
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
        public float Speed { get => speed; set => speed = value; }
        public int Damage { get => damage; set => damage = value; }
        public float DamageRate { get => damageRate; set => damageRate = value; }
        public Color Color { get => color; set => color = value; }
        public float MinDistance { get => minDistance; set => minDistance = value; }

        public void Initialize()
        {
            slots = new Dictionary<ItemType, int>();

            foreach (var slot in m_slots)
            {
                slots.Add(slot.type, slot.maxAmount);
            }

            initialized = true;
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
