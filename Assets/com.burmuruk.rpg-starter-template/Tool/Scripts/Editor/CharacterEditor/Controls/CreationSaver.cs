using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Burmuruk.Tesis.Inventory.Equipment;
using static Burmuruk.Tesis.Inventory.InventoryEquipDecorator;

namespace Burmuruk.Tesis.Editor
{
    public class CreationSaver
    {
        const string ITEMSList_NAME = "GeneralItemsList.asset";
        const string PROGRESS_NAME = "CharactersProgress.asset";
        const string ASSET_EXTENSION = ".asset";
        const string RESULT_PATH = "Results";
        const string ITEMS_FOLDER = "Items";
        const string Armour_FOLDER = "Armour";
        const string CHARACTERS_FOLDER = "Characters";
        const string WEAPONS_FOLDER = "Weapons";
        ItemsList _itemsList;
        string resultsPath = null;
        List<GameObject> garbage = new();
        CharacterProgress _progress;

        CharacterProgress Progress
        {
            get
            {
                if (_progress == null)
                {
                    _progress = AssetDatabase.LoadAssetAtPath<CharacterProgress>(Path + "/" + PROGRESS_NAME);

                    if (_progress == null)
                    {
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterProgress>(), Path + "/" + PROGRESS_NAME);
                        AssetDatabase.Refresh();
                        _progress = AssetDatabase.LoadAssetAtPath<CharacterProgress>(Path + "/" + PROGRESS_NAME);
                    }
                }

                return _progress;
            }
        }

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
                if (string.IsNullOrEmpty(resultsPath))
                {
                    string basePath = SavingSystem.Data.CreationPath;
                    if (!AssetDatabase.IsValidFolder(basePath))
                    {
                        string defaultPath = "Assets/com.burmuruk.rpg-starter-template";
                        resultsPath = AssetDatabase.CreateFolder(defaultPath, RESULT_PATH);
                        AssetDatabase.Refresh();
                    }
                    else
                        resultsPath = basePath;

                    if (!AssetDatabase.IsValidFolder(resultsPath))
                        return null;
                }

                return resultsPath;
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
            string subFolder = Get_ItemSubFolder(item);
            if (!VerifyFolder(subFolder)) return;

            string itemPath = Path + "/" + subFolder + "/" + item.Name + ASSET_EXTENSION;
            if (AssetDatabase.LoadAssetAtPath<InventoryItem>(itemPath) != null)
                AssetDatabase.DeleteAsset(itemPath);

            var copy = ScriptableObject.Instantiate(item);
            AssetDatabase.CreateAsset(copy, itemPath);
            AssetDatabase.Refresh();
            var newItem = AssetDatabase.LoadAssetAtPath<InventoryItem>(itemPath);

            ItemsList.AddItem(newItem);
            EditorUtility.SetDirty(ItemsList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        private string Get_ItemSubFolder(InventoryItem item) =>
            item switch
            {
                Weapon => WEAPONS_FOLDER,
                ArmourElement => Armour_FOLDER,
                _ => ITEMS_FOLDER,
            };

        public void SavePlayer(CharacterData data)
        {
            if (!VerifyFolder(CHARACTERS_FOLDER)) return;

            GameObject player = new GameObject("Player", Get_Components(in data));
            garbage.Add(player);

            foreach (var component in data.components)
                Setup_Components(component.Key, player, component.Value, in data);

            player.GetComponent<Rigidbody>().freezeRotation = true;
            Setup_Character(player, in data);
            Set_ProgressionPoints(in data);

            GameObject instance = PrefabUtility.SaveAsPrefabAsset(player, Path + "/" + CHARACTERS_FOLDER + "/" + data.characterName + ".prefab");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RemoveGarbage();
        }

        private void Set_ProgressionPoints(in CharacterData data)
        {
            var levelsF = typeof(CharacterProgress).GetField("statsList", BindingFlags.Instance | BindingFlags.NonPublic);

            if (levelsF == null) return;
            var oldProgress = ((CharacterProgress.CharacterData[])levelsF.GetValue(Progress)).ToList();
            var newProgress = (CharacterProgress.CharacterData[])levelsF.GetValue(data.progress);

            if (newProgress == null) return;

            for (int i = 0; i < newProgress.Length; i++)
            {
                bool hasType = false;

                for (int j = 0; j < oldProgress.Count; j++)
                {
                    if (oldProgress[j].characterType == newProgress[i].characterType)
                    {
                        hasType = true;
                        var oldData = oldProgress[j];
                        oldData.levelData = newProgress[i].levelData;
                        oldProgress[j] = oldData;
                        break;
                    }
                }

                if (!hasType)
                    oldProgress.Add(newProgress[i]);
            }

            levelsF.SetValue(Progress, oldProgress.ToArray());
        }

        private void Setup_Character(GameObject player, in CharacterData characterData)
        {
            player.GetComponent<Tesis.Control.Character>();

            var typeF = typeof(Tesis.Control.Character).GetField("characterType", BindingFlags.Instance | BindingFlags.NonPublic);
            if (typeF != null)
                typeF.SetValue(player.GetComponent<Tesis.Control.Character>(), characterData.characterType);
            
            Set_DetectionPoints(player, characterData);
            Set_Enemy(player, characterData.enemyTag);
            Setup_BasicStats(player, characterData);
        }

        private void Setup_BasicStats(GameObject player, CharacterData characterData)
        {
            var statsF = typeof(Tesis.Stats.BasicStats).GetField("stats", BindingFlags.Static | BindingFlags.NonPublic);

            if (statsF == null) return;
            statsF.SetValue(player, characterData.basicStats);
        }

        private void Set_Enemy(GameObject player, in string enemyTag)
        {
            AddTag(enemyTag);
            player.tag = enemyTag;
        }

        public void AddTag(string newTag)
        {
            if (string.IsNullOrEmpty(newTag)) return;

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // Revisa si el tag ya existe
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(newTag)) return; // Ya existe
            }

            // Añade el nuevo tag
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = newTag;

            tagManager.ApplyModifiedProperties();
            Debug.Log($"Tag \"{newTag}\" añadido exitosamente.");
        }

        private void Set_DetectionPoints(GameObject player, CharacterData characterData)
        {
            var (earsDetection, eyesDetection) = (false, false);
            int i = 0;
            while (characterData.progress.GetDataByLevel(characterData.characterType, i++) is var data && data != null)
            {
                if (data.Value.eyesRadious != 0)
                    eyesDetection &= true;
                if (data.Value.earsRadious != 0)
                    earsDetection = true;
            }

            var eyesF = typeof(Tesis.Control.Character).GetField("eyesDetection", BindingFlags.Instance | BindingFlags.NonPublic);
            var earsF = typeof(Tesis.Control.Character).GetField("earsDetection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (eyesF != null)
                eyesF.SetValue(player.GetComponent<Tesis.Control.Character>(), eyesDetection);
            if (earsF != null)
                earsF.SetValue(player.GetComponent<Tesis.Control.Character>(), earsDetection);

            if (eyesDetection)
                Add_DetectionPoint(player, "EyesDetection");
            if (earsDetection)
                Add_DetectionPoint(player, "EarsDetection");
        }

        private void Add_DetectionPoint(GameObject player, string name)
        {
            GameObject detectionPoint = new GameObject(name);
            detectionPoint.transform.parent = player.transform;
            garbage.Add(detectionPoint);
        }

        private void RemoveGarbage() => garbage.ForEach(e => GameObject.DestroyImmediate(e));

        private Type[] Get_Components(in CharacterData characterData)
        {
            List<Type> components = new()
            {
                characterData.className,
                typeof(CapsuleCollider),
                typeof(Rigidbody),
            };

            if (characterData.shouldSave)
                components.Add(typeof(JsonSaveableEntity));

            bool containsEquipment = characterData.components.ContainsKey(ComponentType.Equipment);

            foreach (var component in characterData.components)
            {
                if (component.Key == ComponentType.None) continue;

                if (component.Key == ComponentType.Inventory && containsEquipment) continue;

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
                    var inventory = (Inventory)character.components[ComponentType.Inventory];
                    Setup_Inventory(player, inventory);
                    break;

                case ComponentType.Equipment:
                    var equipment = (Equipment)character.components[ComponentType.Equipment];
                    Setup_Equipment(player, equipment);
                    break;

                //case ComponentType.Dialogue:
                //    break;

                //case ComponentType.Patrolling:
                //    break;

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

        private void Setup_Inventory(GameObject instance, Inventory inventory)
        {
            var inventoryComp = instance.GetComponent<Tesis.Inventory.Inventory>();

            FieldInfo initialItemsF = typeof(Tesis.Inventory.Inventory).GetField("startingItems", BindingFlags.Instance | BindingFlags.NonPublic);
            var initalItems = new List<InventoryItem>();

            FieldInfo itemsF = typeof(ItemsList).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var items = (List<InventoryItem>)itemsF.GetValue(ItemsList);

            foreach (var itemData in inventory.items)
            {
                if (!SavingSystem.Data.TryGetCreation(itemData.Key, out var data, out var type))
                    continue;

                string name = data.Name;

                foreach (var item in items)
                {
                    if (item.name == name)
                    {
                        initalItems.Add(item);

                        break;
                    }
                }
            }

            initialItemsF.SetValue(inventoryComp, initalItems.ToArray());
        }

        private void Setup_Equipment(GameObject instance, Equipment equipment)
        {
            var equipper = instance.GetComponent<InventoryEquipDecorator>();

            FieldInfo initialItemsF = typeof(InventoryEquipDecorator).GetField("_initialItems", BindingFlags.Instance | BindingFlags.NonPublic);
            List<InitalEquipedItemData> initialItems = new();

            FieldInfo itemsF = typeof(ItemsList).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var items = (List<InventoryItem>)itemsF.GetValue(ItemsList);

            foreach (var itemData in equipment.equipment)
            {
                string name = SavingSystem.Data.creations[itemData.Value.type][itemData.Key].Name;

                foreach (var item in items)
                {
                    if (item.name == name)
                    {
                        var initialItem = new InitalEquipedItemData();
                        int amount = equipment.inventory.items[itemData.Key];

                        initialItem.Initilize(item, amount, itemData.Value.equipped);
                        initialItems.Add(initialItem);
                        break;
                    }
                }
            }

            initialItemsF.SetValue(equipper, initialItems);

            SetPlayerModel(instance, equipper, in equipment);
        }

        private void Setup_Movement(GameObject instance)
        {

        }

        private void Setup_Fighter(GameObject instance)
        {

        }


        private void SetPlayerModel(GameObject player, InventoryEquipDecorator inventory, in Equipment equipment)
        {
            var eyes = new GameObject("Eyes");
            eyes.transform.parent = player.transform;
            var ears = new GameObject("Ears");
            eyes.transform.parent = player.transform;

            var body = GameObject.Instantiate(equipment.model, Vector3.zero, Quaternion.identity, player.transform);
            garbage.Add(body);

            Dictionary<(string cur, string parent), EquipmentType> names = new();
            equipment.spawnPoints.ForEach(s =>
            {
                names.TryAdd((s.transform.name, s.transform.parent == null ? null : s.transform.parent.name), s.type);
            });

            var spawnPoints = GetSpawnPoints(body, ref names);

            FieldInfo bodyF = typeof(InventoryEquipDecorator).GetField("body", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo spawnPointsF = typeof(InventoryEquipDecorator).GetField("spawnPoints", BindingFlags.Instance | BindingFlags.NonPublic);

            bodyF.SetValue(inventory, body);
            spawnPointsF.SetValue(inventory, spawnPoints.ToArray());
        }

        private List<SpawnPointData> GetSpawnPoints(GameObject model, ref Dictionary<(string cur, string parent), EquipmentType> names)
        {
            var items = new List<SpawnPointData>();

            for (int i = 0; i < model.transform.childCount; i++)
            {
                var child = model.transform.GetChild(i);
                (string cur, string parent)? key = null;

                foreach (var item in names)
                {
                    if (child.transform.name == item.Key.cur)
                    {
                        if (child.transform.parent.transform.name == item.Key.parent)
                        {
                            key = item.Key;
                            break;
                        }
                    }

                }

                if (key.HasValue)
                {
                    var data = new SpawnPointData()
                    {
                        spawnType = (int)names[key.Value],
                        spawnPoint = child,
                    };
                    items.Add(data);
                    names.Remove(key.Value);
                }

                if (child.transform.childCount > 0)
                    items.AddRange(GetSpawnPoints(child.gameObject, ref names));
            }

            return items;
        }
    }
}
