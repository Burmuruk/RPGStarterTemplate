using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public static class SavingSystem
    {
        const string DATA_PATH = "Assets/RPGStarterTemplate/Tool/Data/CharacterTag.asset";
        public static CharacterTag Data { get; private set; } = null;
        public static event Action<ModificationTypes, ElementType, string, CreationData> OnCreationModified;

        public static void Initialize()
        {
            Data = (AssetDatabase.FindAssets(typeof(CharacterTag).ToString(), new[] { DATA_PATH })
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterTag>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList()).FirstOrDefault();

            if (Data == null)
            {
                Notify("No labels found.", BorderColour.Error);

                Data = ScriptableObject.CreateInstance<CharacterTag>();
                AssetDatabase.CreateAsset(Data, DATA_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Notify("Labels found.", BorderColour.Approved);
            }

            OnCreationModified += (modification, type, id, data) =>
            {
                var info = new BaseCreationInfo(id, data.Name, data);
                CreationScheduler.ChangeData(modification, type, id, info);
            };
        }

        public static CreationData GetCreation(ElementType type, string id)
        {
            return Data.creations[type][id];
        }

        public static bool SaveCreation(ElementType type, in string id, in CreationData data, ModificationTypes modificationType)
        {
            string newId = id;
            bool saved = Save_CreationData(type, ref newId, data);

            OnCreationModified?.Invoke(modificationType, type, newId, data);
            return saved;
        }

        public static CreationData? Load(ElementType type, string id)
        {
            if (!Data.creations.ContainsKey(type))
                return null;

            return Data.creations[type][id];
        }

        static bool Save_CreationData(ElementType type, ref string id, CreationData creationData, string newName = "")
        {
            string name = creationData.Name;

            if (string.IsNullOrEmpty(newName))
            {
                if (!name.VerifyName()) return false;

                newName = name;
            }
            else if (newName != name)
            {
                if (!newName.VerifyName())
                    return false;
            }

            if (!Data.creations.ContainsKey(type))
                Data.creations.Add(type, new Dictionary<string, CreationData>());

            if (!string.IsNullOrEmpty(id))
            {
                if (Data.creations[type].ContainsKey(id))
                {
                    Data.creations[type].Remove(id);
                }
            }
            else
                id = Guid.NewGuid().ToString();

            var creation = new CreationData(newName, creationData.data);
            Data.creations[type].TryAdd(id, creation);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return true;
        }

        public static bool Remove(ElementType type, string id)
        {
            var lastData = Data.creations[type][id];
            Data.creations[type].Remove(id);

            OnCreationModified?.Invoke(ModificationTypes.Remove, type, id, lastData);

            return true;
        }
    }

    public interface ISaveable
    {
        public bool Save();
        public CreationData Load(ElementType type, string id);
    }

    [Flags]
    public enum ModificationTypes
    {
        None = 0,
        Add = 1,
        Remove = 1 << 1,
        EditData = 1 << 2,
        Rename = 1 << 3,
        ColourReasigment = 1 << 4
    }

    public interface IChangesObserver
    {
        public ModificationTypes Check_Changes();
        public void Remove_Changes();
    }

    public enum CreationsState
    {
        None,
        Creating,
        Editing,
    }
}
