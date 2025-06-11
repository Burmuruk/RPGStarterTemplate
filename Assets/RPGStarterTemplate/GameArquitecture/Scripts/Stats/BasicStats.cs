using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
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
        [SerializeField] public float ultimoYaBueno;
        [SerializeField] public string hihi;

        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
        [SerializeField] public float minDistance;

        
        [Space(), Header("new variables")]
        [SerializeField] public float magicPowerRando;
        [SerializeField] public bool canFly;
        [SerializeField] public UnityEngine.Color magicColour;

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