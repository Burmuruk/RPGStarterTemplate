using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class GeneralCharacterSettings : SubWindow, ISaveable
    {
        public event Action OnCustomColoursEnabled;
        public event Action OnTypeColoursEnabled;
        Dictionary<ElementType, ISaveable> _creationControls;
        CreationSaver _creationSaver;

        public TextField TxtLocation { get; private set; }
        public Button BtnGenerate { get; private set; }
        public Toggle TglTypeColour { get; private set; }
        public Toggle TglCustomColour { get; private set; }

        public void Initialize(VisualElement container, Dictionary<ElementType, ISaveable> CreationControls)
        {
            _creationControls = CreationControls;
            _container = container;
            TxtLocation = container.Q<TextField>("txtLocation");
            BtnGenerate = container.Q<Button>("btnGenerateElements");
            TglTypeColour = container.Q<Toggle>("tglShowTypeColour");
            TglCustomColour = container.Q<Toggle>("ShowCustomColours");

            TxtLocation.RegisterValueChangedCallback(OnTxtLocation_Changed);
            BtnGenerate.clicked += CreatePrefabs;
            _creationSaver = new CreationSaver();
        }

        private void OnTxtLocation_Changed(ChangeEvent<string> evt)
        {
            VerifyPath(evt.newValue);
        }

        private bool VerifyPath(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                Notify("Not valid directory", BorderColour.Error);
                Highlight(TxtLocation, true, BorderColour.Error);
                return false;
            }

            Highlight(TxtLocation, true, BorderColour.Success);
            DisableNotification();
            SavingSystem.Data.CreationPath = path;
            EditorUtility.SetDirty(SavingSystem.Data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public override ModificationTypes Check_Changes()
        {
            return ModificationTypes.None;
        }

        public override void Clear() { }

        public override void Load_Changes() { }

        private void CreatePrefabs()
        {
            if (string.IsNullOrEmpty(SavingSystem.Data.CreationPath))
            {
                if (string.IsNullOrEmpty(TxtLocation.value))
                {
                    Highlight(TxtLocation, true, BorderColour.Error);
                    Notify("Must enter a location first", BorderColour.Error);
                    return;
                }
                else if (!VerifyPath(TxtLocation.value))
                    return;
            }

            Highlight(TxtLocation, false);

            if (SavingSystem.Data.creations.Count <= 0 ||
                (SavingSystem.Data.creations.Count == 1 && SavingSystem.Data.creations.ContainsKey(ElementType.Buff)))
            {
                Notify("There's no elelemts to create", BorderColour.Error);
                return;
            }

            ElementType[] types = GetTypesInOrder();

            foreach (var creationType in types)
            {
                foreach (var creation in SavingSystem.Data.creations[creationType])
                {
                    switch (creationType)
                    {
                        case ElementType.Item:
                        case ElementType.Armour:
                            var (item, _) = ((InventoryItem, ItemDataArgs))creation.Value.data;
                            _creationSaver.SavetItem(item);
                            break;

                        case ElementType.Weapon:
                        case ElementType.Consumable:
                            var (buffUser, cArgs) = ((InventoryItem, BuffsNamesDataArgs))creation.Value.data;
                            InventoryItem newBuff = ScriptableObject.Instantiate(buffUser);
                            Update_BuffsInfo(buffUser as IBuffUser, cArgs);
                            _creationSaver.SavetItem(buffUser);
                            break;

                        case ElementType.Character:
                            _creationSaver.SavePlayer((CharacterData)creation.Value.data);
                            break;

                        default:
                            break;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Update_BuffsInfo(IBuffUser buffUser, BuffsNamesDataArgs args)
        {
            List<BuffData> newBuffs = new();
            int idx = 0;

            foreach (var name in args.BuffsNames)
            {
                if (name == "")
                {
                    BuffData newBuff = buffUser.Buffs[idx];
                    newBuff.name = "Custom";
                    newBuffs.Add(newBuff);
                    ++idx;
                }
                else
                {
                    newBuffs.Add((BuffData)SavingSystem.Data.creations[ElementType.Buff][name].data);
                }
            }

            buffUser.UpdateBuffData(newBuffs.ToArray());
        }

        private ElementType[] GetTypesInOrder()
        {
            LinkedList<ElementType> types = new();
            bool hasCharacter = false;

            foreach (var type in SavingSystem.Data.creations.Keys)
            {
                if (type == ElementType.Item)
                {
                    types.AddFirst(type);
                }
                else if (type == ElementType.Character)
                    hasCharacter = true;
                else
                    types.AddLast(type);
            }

            if (hasCharacter)
                types.AddLast(ElementType.Character);

            return types.ToArray();
        }

        public override void Enable(bool enabled)
        {
            base.Enable(enabled);
            if (enabled)
            {
                DisableNotification();
                Highlight(TxtLocation, false);
            }
        }

        public override bool VerifyData() => true;

        public bool Save()
        {
            return true;
        }

        public CreationData Load(ElementType type, string id)
        {
            return default;
        }
    }
}
