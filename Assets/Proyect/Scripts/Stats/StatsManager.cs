using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class StatsManager : MonoBehaviour
    {
        Stats m_stats;

        public float Speed { get => m_stats.speed; }
        public int HP { get; set; }
        public int Damage { get; set; }
    }
}
