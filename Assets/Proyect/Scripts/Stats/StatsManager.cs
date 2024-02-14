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

        public float Speed
        {
            get
            {
                return m_stats.speed + speed;
            }
            set => speed = value;
        }

        public int HP
        {
            get
            {
                return m_stats.hp + hp;
            }
            set => hp = value;
        }

        public int Damage
        {
            get
            {
                return inventary.EquipedWeapon.Damage + damage;
            }
            set => damage = value;
        }

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
    }
}
