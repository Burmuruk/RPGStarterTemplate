using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    [Serializable]
    public struct Equipment
    {
        Dictionary<int, (Transform spawnPoint, GameObject item, List<EquipeableItem> equipables)> _parts;

        public event Action<int> OnEquipmentChanged;

        public EquipeableItem this[int part]
        {
            get
            {
                if (_parts == null)
                    Initilize();

                return _parts.ContainsKey(part) ? _parts[part].equipables[0] : default;
            }
        }

        public void Initilize()
        {
            _parts = new Dictionary<int, (Transform spawnPoint, GameObject item, List<EquipeableItem> equipeables)>();
        }

        public void Equip(int part, GameObject item, params EquipeableItem[] equipables)
        {
            if (_parts == null)
                Initilize();

            if (!_parts.ContainsKey(part))
                throw new InvalidOperationException();

            _parts[part] = (_parts[part].spawnPoint, item, equipables.ToList());
            //OnEquipmentChanged?.Invoke(equipables.ID);
        }

        public Transform GetSpawnPoint(int part) => _parts[part].spawnPoint;

        public GameObject GetItem(int part) => _parts[part].item;

        public List<EquipeableItem> GetItems(int part) => _parts[part].equipables;
    }
}