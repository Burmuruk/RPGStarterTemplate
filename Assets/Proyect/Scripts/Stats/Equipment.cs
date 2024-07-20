using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    [Serializable]
    public struct Equipment
    {
        Dictionary<int, (Transform spawnPoint, GameObject item, EquipeableItem equipable)> _parts;

        public event Action<int> OnEquipmentChanged;

        public EquipeableItem this[int part]
        {
            get
            {
                if (_parts == null)
                    Initilize();

                return _parts.ContainsKey(part) ? _parts[part].equipable : default;
            }
        }

        public void Initilize()
        {
            _parts = new Dictionary<int, (Transform spawnPoint, GameObject item, EquipeableItem id)>();
        }

        public void Equip(int part, GameObject item, EquipeableItem equipable)
        {
            if (_parts == null)
                Initilize();

            if (!_parts.ContainsKey(part))
                throw new InvalidOperationException();

            _parts[part] = (_parts[part].spawnPoint, item, equipable);
            OnEquipmentChanged?.Invoke(equipable.ID);
        }

        public Transform GetSpawnPoint(int id) => _parts[id].spawnPoint;

        public GameObject GetItem(int id) => _parts[id].item;

        public GameObject GetItem() => 
            _parts.ContainsKey(0) ? _parts[0].item : null;
    }
}