using Burmuruk.RPGStarterTemplate.Inventory;
using Burmuruk.RPGStarterTemplate.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public class GeneralCharacterSettings : SubWindow, ISaveable
    {
        public event Action OnCustomColoursEnabled;
        public event Action OnTypeColoursEnabled;
        Dictionary<ElementType, ISaveable> _creationControls;
        CreationSaver _creationSaver;

        public TextField TxtLocation { get; private set; }
        public Button BtnRest { get; private set; }
        public Button BtnGenerate { get; private set; }
        public Toggle TglTypeColour { get; private set; }
        public Toggle TglCustomColour { get; private set; }

        public void Initialize(VisualElement container, Dictionary<ElementType, ISaveable> CreationControls)
        {
            _creationControls = CreationControls;
            _container = container;
            TxtLocation = container.Q<TextField>("txtLocation");
            BtnRest = container.Q<Button>("btnReset");
            BtnGenerate = container.Q<Button>("btnGenerateElements");
            TglTypeColour = container.Q<Toggle>("tglShowTypeColour");
            TglCustomColour = container.Q<Toggle>("ShowCustomColours");

            TxtLocation.RegisterValueChangedCallback(OnTxtLocation_Changed);
            BtnRest.clicked += ResetPath;
            BtnGenerate.clicked += CreatePrefabs;
            _creationSaver = new CreationSaver();
        }

        private void ResetPath()
        {
            TxtLocation.value = "Assets/RPG-Results";
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
            if (TxtLocation.value == "Assets/RPG-Results")
            {
                if (!AssetDatabase.IsValidFolder("Assets/RPG-Results"))
                {
                    AssetDatabase.CreateFolder("Assets", "RPG-Results");
                    AssetDatabase.Refresh();
                    if (!VerifyPath(TxtLocation.value))
                        return;
                }
            }
            else if (string.IsNullOrEmpty(SavingSystem.Data.CreationPath))
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
            bool elementCreated = false;

            foreach (var creationType in types)
            {
                foreach (var creation in SavingSystem.Data.creations[creationType])
                {
                    switch (creationType)
                    {
                        case ElementType.Item:
                        case ElementType.Armour:
                            var item = (creation.Value as ItemCreationData).Data;
                            _creationSaver.SavetItem(item);
                            elementCreated = true;
                            break;

                        case ElementType.Weapon:
                        case ElementType.Consumable:
                            var buffUserData = creation.Value as BuffUserCreationData;
                            var (buffUser, cArgs) = (buffUserData.Data, buffUserData.Names);
                            InventoryItem newBuff = ScriptableObject.Instantiate(buffUser);
                            ItemDataConverter.Update_BuffsInfo(buffUser as IBuffUser, cArgs);
                            
                            _creationSaver.SavetItem(buffUser);
                            elementCreated = true;
                            break;

                        case ElementType.Character:
                            _creationSaver.SavePlayer((creation.Value as CharacterCreationData).Data);
                            elementCreated = true;
                            break;

                        default:
                            break;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (elementCreated)
                Notify("Elements created", BorderColour.Success);
            else
                Notify("There were no elements to create", BorderColour.HighlightBorder);
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
