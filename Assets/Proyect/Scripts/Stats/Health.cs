using System;
using Unity.Mathematics;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class Health : MonoBehaviour
    {
        [SerializeField] int hp;
        [SerializeField] int maxHp;

        public event Action OnDied;
        public event Action<int> OnDamaged;

        public int HP { get => hp; }
        public int MaxHp { get => maxHp; }

        public void ApplyDamage(int damage)
        {
            hp = math.max(hp - damage, 0);

            if (hp <= 0)
            {
                Die();
                return;
            }

            OnDamaged?.Invoke(hp);
        }

        public void Heal(int value)
        {
            hp = math.min(hp + value, maxHp);
        }

        private void Die()
        {
            OnDied?.Invoke();
        }

        private void GetHealth()
        {

        }
    }
}