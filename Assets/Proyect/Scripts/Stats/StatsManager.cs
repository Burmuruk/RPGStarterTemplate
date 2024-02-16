using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class StatsManager : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] Stats m_stats;
        [SerializeField] float speed;
        [SerializeField] int hp;
        [SerializeField] int damage;
        [Space(), Header("Detection")]
        [SerializeField] float eyesRadious;
        [SerializeField] float earsRadious;

        Inventary inventary;

        public event Action OnDied;

        #region Stats
        public float Speed
        {
            get
            {
                return speed;
            }
            set => speed = value;
        }

        public int HP
        {
            get
            {
                return hp;
            }
            set
            {
                if (value < 0)
                {
                    hp = 0;
                    OnDied?.Invoke();
                }
                else
                {
                    hp = value;
                }

                print(hp);
            }
        }
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

        private void Start()
        {
            inventary = GetComponent<Inventary>();
            UpdateStats();
        }

        private void UpdateStats()
        {
            (hp, speed) = (m_stats.hp, m_stats.speed);
        }
    }
}
