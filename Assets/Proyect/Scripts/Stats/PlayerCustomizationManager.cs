using Burmuruk.Tesis.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Burmuruk.Tesis.Stats
{
    public class PlayerCustomizationManager : MonoBehaviour
    {
        [SerializeField] GameObject spwnPoint;
        InventaryEquipDecorator inventary;
        List<(GameObject item, ModificationType type)> equipedObjects = new();

        private void Start()
        {
            inventary = GetComponent<InventaryEquipDecorator>();
        }

        public void EquipModifications(Character character)
        {
            var equipedItems = inventary.GetEquipedItems(ItemType.Modification, character);

            foreach (var item in equipedItems)
            {
                var mod = inventary.GetItem(ItemType.Modification, item.GetSubType());
                
                if (mod == null) continue;

                var inst = Instantiate(((Modification)mod).Prefab, character.transform);
                equipedObjects.Add((inst, (ModificationType)mod.GetSubType()));
            }
        }

        public void EquipModification(Character character, Modification modification)
        {
            if (modification == null) return;

            var inst = Instantiate(modification.Prefab, character.transform);
            equipedObjects.Add((inst, (ModificationType)modification.GetSubType()));
        }

        public void UnequipModification(Character character, int modificationType)
        {
            var type = (ModificationType)modificationType;
           
            for (var i = 0; i < equipedObjects.Count; i++)
            {
                if (equipedObjects[i].type == type)
                {
                    Destroy(equipedObjects[i].item);
                    equipedObjects.RemoveAt(i);
                    break;
                }
            }
        }
    }
}