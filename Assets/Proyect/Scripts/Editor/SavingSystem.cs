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
        const string DATA_PATH = "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset";

        public static CharacterTag Data { get; private set; } = null;

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
        }

        public static CreationData GetCreation(ElementType type, string id)
        {
            return Data.creations[type][id];
        }

        public static string SaveCreation(ElementType type, in string id, in CreationData data)
        {
            return null;
        }

        public static CreationData? Load(ElementType type, string id)
        {
            if (!Data.creations.ContainsKey(type))
                return null;

            return Data.creations[type][id];
        }

        static bool Save_CreationData(ElementType type, string name, ref string id, object args, string newName = "")
        {
            if (string.IsNullOrEmpty(newName))
            {
                if (!VerifyName(name)) return false;

                newName = name;
            }
            else if (newName != name)
            {
                if (!VerifyName(newName))
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

            var creation = new CreationData(newName, args);
            Data.creations[type].TryAdd(id, creation);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return true;
        }
    }
}
