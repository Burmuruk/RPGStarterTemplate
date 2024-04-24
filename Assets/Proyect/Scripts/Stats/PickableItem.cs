using Burmuruk.Tesis.Fighting;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class PickableItem : MonoBehaviour, ISaveableItem
    {
        [SerializeField] ItemsList list;
        public ItemType itemType;
        [SerializeField] PickableType itemIdx;

        public int GetSubType()
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
