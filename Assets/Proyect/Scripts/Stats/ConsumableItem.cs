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

            foreach (var buff in buffs)
            {
                if (buff.stat == ModifiableStat.HP)
                {
                    BuffsManager.Instance.AddBuff(character, buff, () => character.Health.ApplyDamage((int)buff.value));
                }
                else
                {
                    BuffsManager.Instance.AddBuff(character, buff);
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
