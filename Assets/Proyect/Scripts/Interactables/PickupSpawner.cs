using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Interaction
{
    public class PickupSpawner : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] ItemsList list;
        [SerializeField] Dictionary<int, PickupItemData> items = new();
        int id;

        class PickupItemData
        {
            public Vector3 position;
            public Quaternion rotation;
            [HideInInspector] public bool picked;
            public Pickup pickup;
        }

        public int ID => id == 0 ? id = GetHashCode() : id;

        public void RegisterCurrentItems()
        {
            var pickups = FindObjectsOfType<Pickup>();
            items.Clear();

            foreach (var pickup in pickups)
            {
                PickupItemData data = new PickupItemData()
                {
                    position = pickup.transform.position,
                    rotation = pickup.transform.rotation,
                    picked = false,
                    pickup = pickup
                };

                pickup.OnPickedUp += RemoveItem;

                items[pickup.ID] = data;
            }
        }

        public JToken CaptureAsJToken(out SavingExecution execution)
        {
            execution = SavingExecution.General;
            JObject state = new JObject();
            List<PickupItemData> pickups = new();
            int i = 0;

            foreach (var item in items)
            {
                JObject itemState = new JObject();

                itemState["Id"] = item.Key;
                itemState["Position"] = VectorToJToken.CaptureVector(item.Value.position);
                itemState["Rotation"] = VectorToJToken.CaptureVector(item.Value.rotation.eulerAngles);
                itemState["Picked"] = item.Value.picked;

                state[i.ToString()] = itemState;
                item.Value.pickup.OnPickedUp += RemoveItem;
            }

            return state;
        }

        public void RestoreFromJToken(JToken jToken)
        {
            if (!(jToken is JObject state)) return;
            items.Clear();
            int i = 0;

            InstanciateLastPickups();

            while (state.ContainsKey(i.ToString()))
            {
                if (state[i.ToString()]["Picked"].ToObject<bool>())
                {
                    i++;
                    continue;
                }

                var curItemState = state[i.ToString()];

                var itemData = new PickupItemData()
                {
                    position = curItemState["Position"].ToObject<Vector3>(),
                    rotation = Quaternion.Euler(curItemState["Rotation"].ToObject<Vector3>()),
                    picked = false
                };

                var item = list.Get(curItemState["Id"].ToObject<int>());
                var inst = Instantiate(item.Pickup, itemData.position, itemData.rotation, transform);

                itemData.pickup = inst;
                items[curItemState["Id"].ToObject<int>()] = itemData;

                inst.OnPickedUp += RemoveItem;
                i++;
            }

            //InstanciateLastPickups();
        }

        private bool InstanciateLastPickups()
        {
            var pickups = FindObjectsOfType<Pickup>();

            if (pickups == null || pickups.Length == 0) return false;

            for (int i = 0; i < pickups.Length; i++)
            {
                Destroy(pickups[i].gameObject);
            }

            //foreach (var item in items)
            //{
            //    var prefab = list.Get(item.Key).Prefab;

            //    Instantiate(prefab, item.Value.position, item.Value.rotation);
            //}

            return true;
        }
        public void AddItem(InventoryItem item, Vector3 pos)
        {
            var pickup = Instantiate(item.Pickup, pos, Quaternion.identity, transform);
            var itemData = new PickupItemData()
            {
                position = pos,
                rotation = pickup.transform.rotation,
                picked = false,
                pickup = pickup
            };

            items.Add(item.ID, itemData);
        }

        private void RemoveItem(GameObject itemToRemove)
        {
            if (!itemToRemove.TryGetComponent<Pickup>(out Pickup pickup))
                return;

            var itemData = items[pickup.ID];

            itemData.picked = true;
            items[pickup.ID] = itemData;
        }
    }
}
