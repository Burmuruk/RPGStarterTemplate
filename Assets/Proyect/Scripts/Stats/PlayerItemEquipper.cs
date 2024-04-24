using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class PlayerItemEquipper : MonoBehaviour
    {
        StatsManager statsManager;

        public void Equip(ItemType itemType, int subType)
        {
            if (itemType != ItemType.Modification) return;


        }

        public void Unequip(ItemType itemType, int subType)
        {

        }
    }
}
