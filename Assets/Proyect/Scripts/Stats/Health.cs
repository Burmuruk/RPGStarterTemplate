using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Proyect.Scripts.Stats
{
    public class Health : MonoBehaviour
    {
        [SerializeField] public int hp;
        [SerializeField] public int maxHp;

        public event Action onDied;

        public int HP { get => hp; }
        public int MaxHp { get => maxHp; }

        public void ApplyDamage(int damage)
        {
            hp = math.max(hp - damage, 0);

            if (hp <= 0)
            {
                Die();
            }
        }

        public void Heal(int value)
        {
            hp = math.min(hp + value, maxHp);
        }

        private void Die()
        {
            onDied?.Invoke();
        }

        private void GetHealth()
        {

        }
    }
}