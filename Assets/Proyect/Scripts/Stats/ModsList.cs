using Burmuruk.Tesis.Control;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public static class ModsList
    {
        static Dictionary<Character, BuffData> buffs = new();

        private struct BuffData
        {
            public Character Character;
            ModsType modeType;
            public Dictionary<ModsType, (float result, List<float> modifications)> mods;

            public List<float> this[ModsType type] => mods[type].modifications;

            public BuffData(Character character, ModsType modsType, float variable)
            {
                this.Character = character;
                this.modeType = modsType;
                mods = new()
                {
                    { modsType, (variable,  new List<float>()) }
                };
            }

            public void RemoveModification(ModsType type, float modification)
            {
                for (int i = 0; i < mods[type].modifications.Count; i++)
                {
                    if (mods[type].modifications[i] == modification)
                    {
                        mods[type].modifications.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static void Add(Character character, ModsType modType, float value)
        {
            if (buffs.ContainsKey(character)) return;

            buffs[character] = new BuffData(character, modType, value);
        }

        public static void Remove(Character character, ModsType modType, float value)
        {
            if (!buffs.ContainsKey(character)) return;

            buffs[character].RemoveModification(modType, value);
        }

        public static void ChangeValue(Character character, ModsType modType, float modification)
        {
            buffs[character][modType].Add(modification);
        }
    }

    public enum ModsType
    {
        None,
        HP,
        Speed,
        BaseDamage,
        GunDamage,
        GunFireRate
    }
}
