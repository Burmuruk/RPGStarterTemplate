using Burmuruk.Tesis.Fighting;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Hability", order = 2)]
    public class Hability : ScriptableObject, ISaveableItem, IEquipable, IUsable
    {
        [SerializeField] float speed;
        [SerializeField] HabilityType type;

        public HabilityType Type { get => type; }

        public void Equip(StatsManager stats)
        {
            //ModsController.CalculateStats()
        }

        public void Remove(StatsManager stats)
        {
            throw new System.NotImplementedException();
        }

        public void Use()
        {
            HabilitiesManager.habilitiesList[type].Invoke(null);
        }
    }

    static class ModsController
    {
        public static void CalculateStats(List<HabilityType> stats)
        {

        }
    }
}
