using Burmuruk.Tesis.Fighting;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Hability", order = 2)]
    public class Ability : ScriptableObject, ISaveableItem, IUsable, IEquipable
    {
        [SerializeField] float speed;
        [SerializeField] AbilityType type;
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [SerializeField] Sprite sprite;
        [SerializeField] int mode;
        [SerializeField] bool isHuman = true;

        public int Mode { get => mode; }
        public bool IsHuman { get => isHuman; }
        public Sprite Sprite { get => sprite; }

        public ItemType Type => ItemType.Ability;

        public BodyManager.BodyPart BodyPart => BodyManager.BodyPart.None;

        public GameObject Prefab => null;

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

    static class ModsController
    {
        public static void CalculateStats(List<AbilityType> stats)
        {

        }
    }
}
