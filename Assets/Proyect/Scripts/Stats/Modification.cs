using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Modifier", order = 3)]
    public class Modification : ScriptableObject, ISaveableItem, IEquipable
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int amount;
        [SerializeField] float amount2;
        [SerializeField] ModificationType type;
        [SerializeField] string m_name;
        [SerializeField] string m_description;

        StatsManager stats;

        public GameObject Prefab { get => prefab; }

        public void Equip(StatsManager stats)
        {
            switch (type)
            {
                case ModificationType.MaxHelth:
                    Debug.Log("HP");
                    stats.MaxHp += amount;
                    break;

                case ModificationType.MaxSpeed:
                    Debug.Log("Speedo");
                    stats.Speed += amount2;
                    break;

                default:
                    break;
            }
        }

        public void Remove(StatsManager stats)
        {
            throw new System.NotImplementedException();
        }

        public int GetSubType()
        {
            return (int)type;
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

    //public struct ModificationData
    //{
    //    public 
    //}

    public enum ModificationType
    {
        None,
        MaxHelth,
        MaxSpeed,
    }
}
