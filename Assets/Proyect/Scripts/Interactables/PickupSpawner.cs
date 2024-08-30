using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Interaction
{
    public class PickupSpawner : MonoBehaviour, ISaveable
    {
        [SerializeField] ItemsList list;
        [SerializeField] List<Pickup> items = new();
        int id;

        struct PickupItemData
        {
            public int id;
            public Vector3 position;
            public Quaternion rotation;
        }

        public int ID => id == 0 ? id = GetHashCode() : id;

        private void Awake()
        {
            if (items.Count == 0) return;

            items.ForEach(item => { item.OnPickedUp += RemoveItem; });
        }

        public object CaptureState()
        {
            List<PickupItemData> pickups = new();

            foreach (var item in items)
            {
                pickups.Add(new PickupItemData()
                {
                    id = item.ID,
                    position = item.transform.position,
                    rotation = item.transform.rotation,
                });
            }

            return pickups.ToArray();
        }

        public void RestoreState(object args)
        {
            var savedItems = (List<PickupItemData>)args;
            items.Clear();

            foreach (var item in savedItems)
            {
                var itemConfig = list.Get(item.id);
                var inst = Instantiate(itemConfig.Pickup, parent: transform, position: item.position, rotation: item.rotation);

                inst.item = itemConfig;
                items.Add(inst);
                inst.OnPickedUp += RemoveItem;
            }
        }

        public void AddItem(InventoryItem item, Vector3 pos)
        {
            var pickup = Instantiate(item.Pickup, pos, Quaternion.identity, transform);
            
            items.Add(pickup);
        }

        private void RemoveItem(GameObject itemToRemove)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].gameObject == itemToRemove)
                {
                    Destroy(items[i]);
                    items.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
