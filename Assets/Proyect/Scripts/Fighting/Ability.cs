using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Ability", order = 2)]
    public class Ability : EquipeableItem
    {
        [SerializeField] float speed;
        [SerializeField] AbilityType type;
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [SerializeField] Sprite sprite;
        [SerializeField] int mode;
        [SerializeField] bool isHuman = true;
        [SerializeField] float coolDown;

        public int Mode { get => mode; }
        public bool IsHuman { get => isHuman; }
        public BodyPart BodyPart => BodyPart.None;
        public float CoolDown { get => coolDown; }

        public void Remove(Character stats)
        {
            throw new System.NotImplementedException();
        }

        public void Use(object args, Action callback)
        {
            AbilitiesManager.habilitiesList[type].Invoke(args, callback);
        }

        public override object GetSubType()
        {
            return Convert.ChangeType(type, typeof(AbilityType));
        }

        public string GetName()
        {
            return m_name;
        }

        public string GetDescription()
        {
            return m_description;
        }

        public override object GetEquipLocation()
        {
            return EquipmentLocation.None;
        }
    }

    static class ModsController
    {
        public static void CalculateStats(List<AbilityType> stats)
        {

        }
    }
}
