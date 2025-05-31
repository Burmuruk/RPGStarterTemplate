using Burmuruk.Tesis.Inventory;
using System;
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
        [SerializeField] public int damage;
        [SerializeField] public float damageRate;
        [SerializeField] public Color color;
        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
        [SerializeField] public float minDistance;

        [Serializable]
        public struct Slot
        {
            public ItemType type;

            public Slot(ItemType type, int amount)
            {
                this.type = type;
            }
        }

        public bool Initilized { get => initialized; }
        public float Speed { get => speed; set => speed = value; }
        public int Damage { get => damage; set => damage = value; }
        public float DamageRate { get => damageRate; set => damageRate = value; }
        public Color Color { get => color; set => color = value; }
        public float MinDistance { get => minDistance; set => minDistance = value; }
    }
}
