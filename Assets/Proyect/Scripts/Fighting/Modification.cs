using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/WeaponMod", order = 3)]
    public class Modification : EquipeableItem
    {
        [Header("Equipment")]
        [SerializeField] EquipmentType equipmentPlace;
        [Space(), Header("Attributes")]
        [SerializeField] ModData[] mods;

        [Serializable]
        private struct ModData
        {
            public float amount;
            public ModifiableStat modifiableStat;
        }

        public override object GetEquipLocation()
        {
            throw new System.NotImplementedException();
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
        }

        public override void Unequip(Character character)
        {
            base.Unequip(character);
        }
    }
}
