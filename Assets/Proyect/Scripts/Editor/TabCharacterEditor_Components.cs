using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
	public class BuffVisulizer : ScriptableObject
	{
		[SerializeField] BuffData buff;
	}

    public partial class TabCharacterEditor : BaseLevelEditor
	{
		private void Create_ItemTab()
		{
            var instance = ScriptableObject.CreateInstance<InventoryItem>();
            infoContainers[infoItemSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_WeaponSettings()
        {
            var instance = ScriptableObject.CreateInstance<Weapon>();
            infoContainers[infoWeaponSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_BuffSettings()
        {
            var instance = ScriptableObject.CreateInstance<BuffVisulizer>();
            infoContainers[infoBuffSettingsName].Add(new InspectorElement(instance));
        }
    } 
}
