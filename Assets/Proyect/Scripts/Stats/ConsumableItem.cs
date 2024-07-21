using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Consumable", order = 3)]
    public class ConsumableItem : EquipeableItem
    {
        [Space(), Header("Attributes")]
        [SerializeField] ConsumableType consumableType;
        [SerializeField] int value;
        [SerializeField] float consumptionTime;
        [SerializeField] int consumptionRate;
        [SerializeField] float areaRadious;

        public int Value { get => value; }
        public float ConsumptionTime { get => consumptionTime; }
        public int ConsumptionRate { get => consumptionRate; }

        public override object GetEquipLocation()
        {
            return EquipmentLocation.None;
        }

        public override object GetSubType()
        {
            return consumableType;
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
