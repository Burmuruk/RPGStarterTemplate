using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    }

    //public static class BasicStatsEditor
    //{
        
    //}
}
