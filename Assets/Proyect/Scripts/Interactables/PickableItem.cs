using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Interaction
{
    public class PickableItem : MonoBehaviour
    {
        [SerializeField] InventoryItem item;

        public event Action<GameObject> OnPickedUp;

        public InventoryItem PickUp()
        {
            OnPickedUp?.Invoke(gameObject);
            return item;
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
