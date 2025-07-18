using Burmuruk.RPGStarterTemplate.Inventory;
using System;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Stats
{
    [Serializable]
    public struct BasicStats
    {
        bool initialized;

        [Space(), Header("Basic stats")]
        [SerializeField] public float speed;
        [SerializeField] public int damage;
        [SerializeField] public float damageRate;
        [SerializeField] public Color color;
        [SerializeField] public int Name;

        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
        [SerializeField] public float minDistance;

        
        [Space(), Header("Mis variables")]
        [SerializeField] public float magia;
        [SerializeField] public int poder;

[Serializable]
        public struct Slot
        {
            public ItemType type;

            public Slot(ItemType type, int amount)
            {
                this.type = type;
            }
        }

        public static bool operator == (BasicStats a, BasicStats b)
        {
            return a.Equals(b);
        }

        public static bool operator != (BasicStats a, BasicStats b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is BasicStats other)
            {
                return speed == other.speed &&
                       damage == other.damage &&
                       damageRate == other.damageRate &&
                       color == other.color &&
                       eyesRadious == other.eyesRadious &&
                       earsRadious == other.earsRadious &&
                       minDistance == other.minDistance;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            hashCode = hashCode * 31 + speed.GetHashCode();
            hashCode = hashCode * 31 + damage.GetHashCode();
            hashCode = hashCode * 31 + damageRate.GetHashCode();
            hashCode = hashCode * 31 + color.GetHashCode();
            hashCode = hashCode * 31 + eyesRadious.GetHashCode();
            hashCode = hashCode * 31 + earsRadious.GetHashCode();
            hashCode = hashCode * 31 + minDistance.GetHashCode();
            return hashCode;
        }
    }
}