using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    class Inventary : MonoBehaviour
    {
        [SerializeField] List<Weapon> weapons;
        [SerializeField] List<Hability> habilites;
        [SerializeField] List<Modifiers> modifiers;

        static List<Weapon> m_weapons;
        static List<Hability> m_habilites;
        static List<Modifiers> m_modifiers;
        ArrayList m_owned;
        int? m_equipedWeapon = 1;

        public event Action OnWeaponChanged;

        public Weapon EquipedWeapon
        {
            get
            {
                if (m_equipedWeapon.HasValue)
                {
                    return m_weapons[m_equipedWeapon.Value];
                }

                return null;
            }
        }

        private void Awake()
        {
            m_weapons ??= weapons;
            m_habilites ??= habilites;
            m_modifiers ??= modifiers;
        }

        private void Start()
        {
            
        }

        public void ChangeEquipedWeapon(int idx)
        {
            if (m_weapons != null && m_weapons[idx] != null)
            {
                m_equipedWeapon = idx;
                OnWeaponChanged?.Invoke();
            }
        }
    }
}
