using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Modifier", order = 3)]
    public class Modification : ScriptableObject, ISaveableItem, IEquipable
    {
        [SerializeField] int amount;
        [SerializeField] float amount2;
        [SerializeField] ModificationType type;

        StatsManager stats;

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
