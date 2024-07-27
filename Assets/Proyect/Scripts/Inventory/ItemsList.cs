using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/IntemsList", order = 1)]
    public class ItemsList : ScriptableObject
    {
        [Header("General Lists")]
        [SerializeField] List<InventoryItem> items;
        [SerializeField] bool Initialized;

        Dictionary<int, InventoryItem> _mainList;

        public InventoryItem Get(int itemId)
        {
            if (!Initialized) Initialize();

            return _mainList[itemId];
        }

        public List<InventoryItem> GetList(ItemType type)
        {
            if (!Initialized) Initialize();

            return (from item in _mainList.Values
                    where item.Type == type
                    select item
                    ).ToList();
        }

        private void Initialize()
        {
            if (Initialized) return;

            _mainList = new Dictionary<int, InventoryItem>();

            foreach (var item in items)
            {
                _mainList.Add(item.ID, item);
            }

            //Initialized = true;
        }
    }
}
