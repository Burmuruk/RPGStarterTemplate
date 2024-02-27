using System;
using System.Collections.Generic;
using UnityEngine;
using static Burmuruk.Tesis.Stats.Inventary;

namespace Burmuruk.Tesis.Stats
{
    public class StatsManager : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] Stats m_stats;
        [SerializeField] float speed;
        [SerializeField] int hp;
        int maxHp = 100;
        [SerializeField] int damage;
        [Space(), Header("Detection")]
        [SerializeField] float eyesRadious;
        [SerializeField] float earsRadious;

        Inventary inventary;

        public event Action OnDied;

        Dictionary<ItemType, int> slots;

        #region Stats
        public float Speed
        {
            get
            {
                return speed;
            }
            set => speed = value;
        }

        public int Hp
        {
            get
            {
                return hp;
            }
            set => hp = value;
        }

        public int MaxHp { get; set; }
        #endregion

        #region Combat
        public int Damage
        {
            get
            {
                return inventary.EquipedWeapon.Damage + damage;
            }
            set => damage = value;
        } 

        public float DamageRate
        {
            get
            {
                return inventary.EquipedWeapon.DamageRate;
            }
        }
        #endregion

        #region Detection
        public float EyesRadious
        {
            get
            {
                return m_stats.eyesRadious + eyesRadious;
            }
            set => eyesRadious = value;
        }

        public float EarsRadious
        {
            get
            {
                return m_stats.earsRadious + earsRadious;
            }
            set => earsRadious = value;
        } 
        #endregion

        public void ApplyDamage(int amount)
        {
            if (hp - amount < 0)
            {
                Hp = 0;
                OnDied?.Invoke();
            }
            else
            {
                Hp -= amount;
            }

            print(Hp);
        }

        private void Start()
        {
            inventary = GetComponent<Inventary>();
            UpdateStats();
        }

        private void UpdateStats()
        {
            (hp, speed) = (m_stats.hp, m_stats.speed);
            slots = m_stats.GetAllSlots();
        }

        public int GetSlots(ItemType type)
        {
            return slots[type];
        }

        public Dictionary<ItemType, int> GetAllSlots()
        {
            return slots;
        }
    }
}
