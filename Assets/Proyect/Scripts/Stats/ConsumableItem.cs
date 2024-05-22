using Burmuruk.Tesis.Control;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Consumable", order = 3)]
    public class ConsumableItem : ScriptableObject, ISaveableItem
    {
        [Header("Information")]
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [Space(), Header("Settings")]
        [SerializeField] ConsumableType consumableType;
        [SerializeField] int value;
        [SerializeField] float consumptionTime;
        [SerializeField] int consumptionRate;
        [SerializeField] float areaRadious;
        [SerializeField] GameObject prefab;

        public ItemType Type => ItemType.Consumable;
        public int Value { get => value; }
        public float ConsumptionTime { get => consumptionTime; }
        public int ConsumptionRate { get => consumptionRate; }
        public GameObject Prefab { get => prefab; }

        public int GetSubType()
        {
            return (int)consumableType;
        }

        public string GetName()
        {
            return m_name;
        }

        public string GetDescription()
        {
            return m_description;
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
