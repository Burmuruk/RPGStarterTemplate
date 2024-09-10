using System;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] InventoryItem inventoryItem;
        [SerializeField] public GameObject prefab;

        public event Action<GameObject> OnPickedUp;

        public int ID { get => inventoryItem.ID; }
        public GameObject Prefab { get => prefab; set => prefab = value; }

        public int PickUp()
        {
            OnPickedUp?.Invoke(gameObject);
            return ID;
        }
    }
}