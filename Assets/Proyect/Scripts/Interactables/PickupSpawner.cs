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
        [SerializeField] List<Pickup> items = new();
        int id;

        struct PickupItemData
        {
            public int id;
            public Vector3 position;
            public Quaternion rotation;
            [HideInInspector] public bool Picked;
        }

        public int ID => id == 0 ? id = GetHashCode() : id;

        private void Awake()
        {
            if (items.Count == 0) return;

            items.ForEach(item => { item.OnPickedUp += RemoveItem; });
        }

        public JToken CaptureAsJToken()
        {
            JObject state = new JObject();
            List<PickupItemData> pickups = new();
            int i = 0;

            foreach (var item in items)
            {
                JObject itemState = new JObject();

                itemState["Id"] = item.ID;
                itemState["Position"] = VectorToJToken.CaptureVector(item.transform.position);
                itemState["Rotation"] = VectorToJToken.CaptureVector(item.transform.rotation.eulerAngles);
                //state["Picked"] = item.pick;
                state[i] = itemState;
            }

            return state;
        }

        public void RestoreFromJToken(JToken jToken)
        {
            if (!(jToken is JObject state)) return;
            items.Clear();
            int i = 0;

            while (state.ContainsKey(i.ToString()))
            {
                var item = state[i];
                var itemConfig = list.Get(item["Id"].ToObject<int>());
                var position = item["Position"].ToObject<Vector3>();
                var rotation = item["Rotation"].ToObject<Vector3>();

                var inst = Instantiate(itemConfig.Pickup, parent: transform, position: position, rotation: Quaternion.Euler(rotation));

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
                    items[i].enabled = false;
                    //Destroy(items[i]);
                    items.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
