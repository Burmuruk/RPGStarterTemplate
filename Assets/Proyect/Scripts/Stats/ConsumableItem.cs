using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine;
using Character = Burmuruk.Tesis.Control.Character;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Consumable", order = 3)]
    public class ConsumableItem : EquipeableItem, IUsable
    {
        [Space(), Header("Attributes")]
        [SerializeField] BuffData[] buffs;
        [SerializeField] float consumptionTime;
        [SerializeField] float areaRadious;

        [Serializable]
        private struct BuffData
        {
            public ModifiableStat stat;
            public float value;
            public float duration;
            public float rate;
            public bool percentage;
            public bool affectAll;
        }

        //public int Value { get => value; }
        public float ConsumptionTime { get => consumptionTime; }

        public override object GetEquipLocation()
        {
            return EquipmentLocation.Items;
        }

        public override object GetSubType()
        {
            if (buffs != null && buffs.Length > 0)
                return buffs[0].stat;

            return ConsumableType.None;
        }

        public void Use(object args, Action callback)
        {
            var character = (Character)args;

            foreach (var mod in buffs)
            {
                if (mod.stat == ModifiableStat.HP)
                {
                    BuffsManager.Instance.HealOverTime(character, (int)mod.value, mod.duration, mod.rate);
                }
                else
                {
                    BuffsManager.Instance.AddBuff(character, mod.stat, mod.value, mod.duration);
                }
            }

            callback?.Invoke();
        }
    }

    public enum ConsumableType
    {
        None,
        Hp,
        Speed
    }

    public interface IPickable
    {
        void Use();
    }

    public interface IItem
    {
        void Use();
    }
}
