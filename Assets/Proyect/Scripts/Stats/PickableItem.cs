using Burmuruk.Tesis.Fighting;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class PickableItem : MonoBehaviour
    {
        [SerializeField] ItemsList list;
        public ItemType type;
        [SerializeField] int itemIdx;

        public ISaveableItem Item
        {
            get => type switch
            {
                ItemType.Hability => list.Habilities[itemIdx],
                ItemType.Modification => list.Modifiers[itemIdx],
                ItemType.Consumable => list.Items[itemIdx],
                _ => null
            };
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
