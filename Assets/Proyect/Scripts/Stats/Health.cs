using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class Health : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] int hp;
        [SerializeField] int maxHp;

        public event Action OnDied;
        public event Action<int> OnDamaged;

        public int HP { get => hp; }
        public int MaxHp { get => maxHp; }

        private void Awake()
        {
            ModsList.AddVariable(GetComponent<Control.Character>(), ModifiableStat.HP, () => hp, (value) => { hp = (int)value; });
        }

        public void ApplyDamage(int damage)
        {
            hp = Math.Max(hp - damage, 0);

            if (hp <= 0)
            {
                Die();
                OnDied?.Invoke();
                return;
            }

            OnDamaged?.Invoke(hp);
        }

        public void Heal(int value)
        {
            hp = Math.Min(hp + value, maxHp);
        }

        private void Die()
        {
            OnDied?.Invoke();
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(hp);
        }

        public void RestoreFromJToken(JToken state)
        {
            hp = state.ToObject<int>();
        }
    }
}