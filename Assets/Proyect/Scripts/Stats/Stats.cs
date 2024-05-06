using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/CharacterStats", order = 1)]
    public class Stats : ScriptableObject
    {
        [Header("Status")]
        [SerializeField] bool initialized = false;
        [Space(20), Header("Basic stats")]
        [SerializeField] public float speed;
        [SerializeField] public int hp;
        [SerializeField] public int maxHp;
        [SerializeField] public float damage;
        [SerializeField] public Color color;
        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
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
