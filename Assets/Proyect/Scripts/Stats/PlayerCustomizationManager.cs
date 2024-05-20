using Burmuruk.Tesis.Control;
using System.Collections.Generic;
using UnityEngine;

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

                EquipModification(character, (IEquipable)mod);
            }
        }

        public void EquipModification(Character character, IEquipable modification)
        {
            if (modification == null) return;

            var parent = character.BodyManager.GetPart(modification.BodyPart);

            var inst = Instantiate(modification.Prefab, parent.transform);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.Euler(Vector3.zero);

            equipedObjects.Add((inst, (ModificationType)(modification as ISaveableItem).GetSubType()));
        }

        public void UnequipModification(Character character, IEquipable equipable)
        {
            var bodyPart = character.BodyManager.GetPart(equipable.BodyPart);

            for (int i = 0; i < bodyPart.transform.childCount; i++)
            {
                Destroy(bodyPart.transform.GetChild(i).gameObject);
            }

            //var type = (ModificationType)modificationType;

            //for (var i = 0; i < equipedObjects.Count; i++)
            //{
            //    if (equipedObjects[i].type == type)
            //    {
            //        Destroy(equipedObjects[i].item);
            //        equipedObjects.RemoveAt(i);
            //        break;
            //    }
            //}
        }
    }
}