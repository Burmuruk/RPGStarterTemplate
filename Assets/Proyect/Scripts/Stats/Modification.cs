using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Modifier", order = 3)]
    public class Modification : EquipeableItem
    {
        [SerializeField] int amount;
        [SerializeField] float amount2;
        [SerializeField] ModsType subType;
        [SerializeField] BodyPart equipmentPlace;
        [SerializeField] string m_name;
        [SerializeField] string m_description;

        public BodyPart Location { get => equipmentPlace; }

        public void Equip(Character character, float value)
        {
            ModsList.Add(character, subType, value);
        }

        public void Remove(Character character, float value)
        {
            ModsList.Remove(character, subType, value);
        }

        public override object GetSubType()
        {
            return subType;
        }

        public override object GetEquipLocation()
        {
            return equipmentPlace;
        }
    }
}
