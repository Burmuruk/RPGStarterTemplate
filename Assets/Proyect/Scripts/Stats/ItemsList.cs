using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Inventary", order = 1)]
    class ItemsList : ScriptableObject
    {
        [Header("General Lists")]
        [SerializeField] List<Weapon> weapons;
        [SerializeField] List<Modification> modifiers;
        [SerializeField] List<Modification> items;
        [SerializeField] List<Hability> habilites;

        List<Weapon> m_weapons;
        List<ModificationData> m_modifiers;
        [SerializeField] List<Pickable> m_items;

        public List<Pickable> Items { get => m_items; }
        public List<Modification> Modifiers { get => modifiers; }
        public List<Hability> Habilities { get => habilites; }
    }
}
