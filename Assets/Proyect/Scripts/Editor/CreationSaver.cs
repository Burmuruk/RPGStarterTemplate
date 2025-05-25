using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Burmuruk.Tesis.Inventory.InventoryEquipDecorator;

namespace Burmuruk.Tesis.Editor
{
    public class CreationSaver
    {
        const string ITEMSList_NAME = "GeneralItemsList.asset";
        const string ASSET_EXTENSION = ".asset";
        const string RESULT_PATH = "Results";
        const string ITEMS_FOLDER = "Items";
        const string Armour_FOLDER = "Armour";
        const string CHARACTERS_FOLDER = "Characters";
        const string WEAPONS_FOLDER = "Weapons";
        ItemsList _itemsList;
        string resultsPath = null;

        ItemsList ItemsList
        {
            get
            {
                if (_itemsList == null)
                {
                    _itemsList = AssetDatabase.LoadAssetAtPath<ItemsList>(Path + "/" + ITEMSList_NAME);

                    if (_itemsList == null)
                    {
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ItemsList>(), Path + "/" + ITEMSList_NAME);
                        AssetDatabase.Refresh();
                        _itemsList = AssetDatabase.LoadAssetAtPath<ItemsList>(Path + "/" + ITEMSList_NAME);
                    }
                }

                return _itemsList;
            }
        }
        public string Path
        {
            get
            {
                string basePath = SavingSystem.Data.CreationPath;
                if (!AssetDatabase.IsValidFolder(basePath + "/" + RESULT_PATH))
                {
                    AssetDatabase.CreateFolder(basePath, RESULT_PATH);
                    AssetDatabase.Refresh();
                }

                if (!AssetDatabase.IsValidFolder(basePath + "/" + RESULT_PATH))
                    return null;

                return resultsPath = basePath + "/" + RESULT_PATH;
            }
        }

        private bool VerifyFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(Path + "/" + path))
            {
                AssetDatabase.CreateFolder(Path, path);
                AssetDatabase.Refresh();
            }

            return AssetDatabase.IsValidFolder(Path + "/" + path);
        }

        public void SavetItem(InventoryItem item)
        {
            string subFolder = Get_SubFolder(item);
            if (!VerifyFolder(subFolder)) return;

            string itemPath = Path + "/" + subFolder + "/" + item.Name + ASSET_EXTENSION;
            AssetDatabase.CreateAsset(item, itemPath);
            AssetDatabase.Refresh();
            var newItem = AssetDatabase.LoadAssetAtPath<InventoryItem>(itemPath);

            ItemsList.AddItem(newItem);
            EditorUtility.SetDirty(ItemsList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        private string Get_SubFolder(InventoryItem item) =>
            item switch
            {
                Weapon => WEAPONS_FOLDER,
                ArmourElement => Armour_FOLDER,
                _ => ITEMS_FOLDER,
            };

        public void SavePlayer(CharacterData character)
        {
            
            GameObject player = new GameObject("Player", Get_Components(in character));
                
            foreach (var component in character.components)
                Setup_Components(component.Key, player, component.Value, in character);

            GameObject instance = PrefabUtility.SaveAsPrefabAsset(player, Path + "/" + character.characterName + ".prefab");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private Type[] Get_Components(in CharacterData characterData)
        {
            List<Type> components = new()
            {
                typeof(Tesis.Control.Character),
            };
            bool containsEquipment = characterData.components.ContainsKey(ComponentType.Equipment);

            foreach (var component in characterData.components)
            {
                if (component.Key == ComponentType.None) continue;

                if (component.Key == ComponentType.Inventory && containsEquipment) continue;

                //if (component.Key == ComponentType.Equipment && containsEquipment)
                //{
                //    components.Add(Get_ComponentType(ComponentType.Inventory));
                //    Add_CharacterComponent(ComponentType.Inventory, player, character.components[ComponentType.Inventory]);
                //}

                components.Add(Get_ComponentType(component.Key));
            }
            
            return components.ToArray();
        }

        private void Setup_Components(ComponentType type, GameObject player, object data, in CharacterData character)
        {
            switch (type)
            {
                case ComponentType.Health:
                    Setup_Health(player, (float)data);
                    break;

                case ComponentType.Fighter:
                    Setup_Fighter(player);
                    break;

                case ComponentType.Mover:
                    Setup_Movement(player);
                    break;

                case ComponentType.Inventory:
                    Setup_Inventory(player);
                    break;

                case ComponentType.Equipment:
                    var equipment = (Equipment)character.components[ComponentType.Equipment];
                    var inventory = (Inventory)character.components[ComponentType.Inventory];
                    Setup_Equipment(player, equipment);
                    break;

                case ComponentType.Dialogue:
                    break;

                case ComponentType.Patrolling:
                    break;

                case ComponentType.None:
                    break;

                default:
                    break;
            }
        }

        private Type Get_ComponentType(ComponentType type) =>
            type switch
            {
                ComponentType.Mover => typeof(Movement.Movement),
                ComponentType.Fighter => typeof(Fighter),
                ComponentType.Inventory => typeof(Tesis.Inventory.Inventory),
                ComponentType.Equipment => typeof(Tesis.Inventory.InventoryEquipDecorator),
                ComponentType.Health => typeof(Tesis.Stats.Health),
                _ => null
            };

        private void Setup_Health(GameObject instance, float health)
        {
            var inventory = instance.GetComponent<Tesis.Stats.Health>();

            FieldInfo maxHealthF = typeof(Tesis.Stats.Health).GetField("_maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
            maxHealthF.SetValue(inventory, health);
            FieldInfo healthF = typeof(Tesis.Stats.Health).GetField("_hp", BindingFlags.Instance | BindingFlags.NonPublic);
            healthF.SetValue(inventory, health);
            //var character = instance.GetComponents<Tesis.Control.Character>();

            //FieldInfo characterHealthF = typeof(Tesis.Control.Character).GetField("maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
            //characterHealthF.SetValue(character, health);
        }

        private void Setup_Inventory(GameObject instance)
        {

        }

        private void Setup_Equipment(GameObject instance, Equipment equipment)
        {
            var equipper = instance.GetComponent<InventoryEquipDecorator>();

            FieldInfo initialItemsF = typeof(InventoryEquipDecorator).GetField("_initialItems", BindingFlags.Instance | BindingFlags.NonPublic);
            List<InitalEquipedItemData> initialItems = new ();

            FieldInfo itemsF = typeof(ItemsList).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var items = (List<InventoryItem>)itemsF.GetValue(ItemsList);

            foreach (var data in equipment.equipment)
            {
                string name = SavingSystem.Data.creations[data.Value.type][data.Key].Name;
                //var item = AssetDatabase.LoadAssetAtPath<InventoryItem>(Path + "/" + ITEMS_FOLDER + name + ASSET_EXTENSION);
                foreach (var item in items)
                {
                    if (item.name == name)
                    {
                        var initialItem = new InitalEquipedItemData();
                        initialItem.Initilize(item, data.Value.equipped);

                        initialItems.Add(initialItem);
                    }
                }
            }

            initialItemsF.SetValue(equipper, initialItems);
        }

        private void Setup_Movement(GameObject instance)
        {

        }

        private void Setup_Fighter(GameObject instance)
        {

        }
    }
}
