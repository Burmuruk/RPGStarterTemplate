using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        VisualElement infoMainContainer;
        const string INFO_MAIN_CONTAINER_NAME = "infoMainContainer";
        const string INFO_CLASS_NAME = "infoClassContainer";
        const string INFO_CHARACTER_NAME = "CharacterSettings";
        const string INFO_ITEM_SETTINGS_NAME = "ItemSettings";
        const string INFO_WEAPON_SETTINGS_NAME = "WeaponSettings";
        const string INFO_BUFF_SETTINGS_NAME = "BuffSettings";
        const string INFO_CONSUMABLE_SETTINGS_NAME = "ConsumableSettings";
        const string INFO_ARMOUR_SETTINGS_NAME = "ArmorSettings";
        const string INFO_GENERAL_SETTINGS_CHARACTER_NAME = "GeneralSettingsCharacter";
        const string INFO_DIALOGUES_NAME = "infoDialoguesContainer";
        const string INFO_SETUP_NAME = "InfoBase";

        TextField txtSearch_Right;
        List<TagData> btnsRight_Tag;
        VisualElement leftPanel;
        VisualElement rightPanel;
        VisualElement infoRight;
        VisualElement infoSetup;
        VisualElement rootTab;
        static TabCharacterEditor currentWindow;

        SearchBar searchBar;
        (ElementType type, int idx) currentSettingTag = (ElementType.None, -1);

        [MenuItem("LevelEditor/CharacterCreator")]
        public static void ShowWindow()
        {
            TabCharacterEditor window = GetWindow<TabCharacterEditor>();
            window.titleContent = new GUIContent("Character creation");
            window.minSize = new Vector2(400, 400);
            currentWindow = window;
        }

        private void OnDisable()
        {
            //if (currentWindow == this)
            //{
            //    currentWindow = null;
            //}
            SavingSystem.Data.changes.Clear();
            SavingSystem.Data.mainElementChange = ElementType.None;
        }

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/Tabs/CharacterTab.uxml");
            rootTab = visualTree.Instantiate();
            container.Add(rootTab);
            //rootTab.style.height = new StyleLength(StyleKeyword.;

            BaseStyleSheets.ForEach(styleSheet => { container.styleSheets.Add(styleSheet); });

            GetTabButtons();
            GetNotificationSection();

            SavingSystem.Initialize();
            //Load_CreatedAssets();
            CreateTagsContainer();
            GetInfoContainers();
            CreateSettingTabs();

            SavingSystem.LoadCreations();
            searchBar.SearchAllElements();
            Load_UnsavedChanges();
            //ChangeTab(INFO_GENERAL_SETTINGS_CHARACTER_NAME);
        }

        private void Load_UnsavedChanges()
        {
            var changes = SavingSystem.Data.changes;
            foreach (var change in changes)
            {
                var data = change.Value.FirstOrDefault();

                if (string.IsNullOrEmpty(data.Key)) continue;
                
                switch (change.Key)
                {
                    case ElementType.Weapon:
                        var weaponData = data.Value as BuffUserCreationData;
                        var (weapon, weBuffsNames) = (weaponData.Data, weaponData.Names);
                        
                        (CreationControls[change.Key] as WeaponSetting).UpdateInfo(weapon, weBuffsNames); 
                        break;

                    case ElementType.Consumable:
                        var consumableData = data.Value as BuffUserCreationData;
                        var (consumable, consuBuffsNames) = (consumableData.Data, consumableData.Names);

                        (CreationControls[change.Key] as ConsumableSettings).UpdateInfo(consumable, consuBuffsNames);
                        break;

                    case ElementType.Armour:
                        var armour = (data.Value as ItemCreationData).Data;

                        (CreationControls[change.Key] as ArmourSetting).UpdateInfo(armour, null);
                        break;

                    case ElementType.Item:
                        var item = (data.Value as ItemCreationData).Data;

                        (CreationControls[change.Key] as BaseItemSetting).UpdateInfo(item, null);
                        break;

                    case ElementType.Buff:
                        var buff = (data.Value as BuffCreationData).Data;

                        (CreationControls[change.Key] as BuffSettings).UpdateInfo(buff);
                        break;

                    case ElementType.Character:
                        var characterData = (data.Value as CharacterCreationData).Data;

                        (CreationControls[change.Key] as CharacterSettings).LoadInfo(characterData, null);
                        break;
                }
            }

            string tabName = null;
            foreach (var tab in infoContainers)
            {
                if (tab.Value.type == SavingSystem.Data.mainElementChange)
                {
                    tabName = tab.Key;
                    break;
                }
            }
            if (tabName != null && infoContainers.ContainsKey(tabName))
            {
                ChangeTab(tabName);
            }
            else
            {
                ChangeTab(INFO_GENERAL_SETTINGS_CHARACTER_NAME);
            }

            SavingSystem.Data.changes.Clear();
            SavingSystem.Data.mainElementChange = ElementType.None;
        }

        private void Load_CreatedAssets()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/RPG-Results");
            bool noAssets = true;

            //foreach (var asset in assets)
            //{
            //    switch (asset)
            //    {
            //        case Tesis.Combat.Weapon weapon:
            //            noAssets &= !SavingSystem.SaveCreation(ElementType.Weapon, null, new CreationData(weapon.Name, weapon), ModificationTypes.Add);
            //            break;

            //        case Tesis.Stats.ConsumableItem consumable:
            //            noAssets &= !SavingSystem.SaveCreation(ElementType.Consumable, null, new CreationData(consumable.Name, consumable), ModificationTypes.Add);
            //            break;

            //        case Tesis.Inventory.ArmourElement armour:
            //            noAssets &= !SavingSystem.SaveCreation(ElementType.Armour, null, new ItemCreationData(armour.Name, armour), ModificationTypes.Add);
            //            break;

            //        case Tesis.Inventory.InventoryItem item:
            //            noAssets &= !SavingSystem.SaveCreation(ElementType.Item, null, new ItemCreationData(item.Name, item), ModificationTypes.Add);
            //            break;

            //        case Tesis.Control.Character character:
            //            noAssets &= !SavingSystem.SaveCreation(ElementType.Character, null, new CharacterCreationData(asset.name, character), ModificationTypes.Add);
            //            break;

            //        default: break;
            //    }
            //}

            if (!noAssets)
            {
                Notify("Assets loaded successfully", BorderColour.Success);
            }
            else
            {
                Notify("No assets were found", BorderColour.Success);
            }
        }

        private void CreateSettingTabs()
        {
            Setup_Coponents();
            Create_BaseSettingsTab();
            Create_CharacterTab();
            Create_WeaponSettings();
            Create_ItemTab();
            Create_BuffSettings();
            Create_ConsumableSettings();
            Create_ArmourSettings();
            Create_GeneralCharacterSettings();
        }

        protected override void GetInfoContainers()
        {
            infoMainContainer = container.Q<VisualElement>(INFO_MAIN_CONTAINER_NAME);
            infoContainers = new();

            AddContainer(infoSetup, true,
                (INFO_CHARACTER_NAME, ElementType.Character),
                (INFO_ITEM_SETTINGS_NAME, ElementType.Item),
                (INFO_WEAPON_SETTINGS_NAME, ElementType.Weapon),
                (INFO_BUFF_SETTINGS_NAME, ElementType.Buff),
                (INFO_ARMOUR_SETTINGS_NAME, ElementType.Armour),
                (INFO_CONSUMABLE_SETTINGS_NAME, ElementType.Consumable)
                );

            AddContainer(infoRight, false, (INFO_GENERAL_SETTINGS_CHARACTER_NAME, ElementType.None));
            infoRight.Add(infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME].element);

            void AddContainer(VisualElement container, bool shouldAdd, params (string name, ElementType type)[] names)
            {
                foreach (var containerData in names)
                {
                    VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/Tabs/{containerData.name}.uxml").Instantiate();
                    StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/LineTags.uss");
                    StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/BorderColours.uss");
                    StyleSheet styleSheetBasic = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/BasicSS.uss");
                    newContainer.styleSheets.Add(styleSheet);
                    newContainer.styleSheets.Add(styleSheetColour);
                    newContainer.styleSheets.Add(styleSheetBasic);

                    infoContainers.Add(containerData.name, (newContainer, containerData.type));
                    tabNames[containerData.name] = "";

                    if (shouldAdd)
                        container.Q<ScrollView>("infoContainer").Add(newContainer);

                    EnableContainer(newContainer, false);
                }
            }
        }

        private void CreateTagsContainer()
        {
            VisualTreeAsset tagsContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/TagsContainer.uxml");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/LineTags.uss");
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/BorderColours.uss");

            container.styleSheets.Add(styleSheet);
            container.styleSheets.Add(styleSheetColour);
            var infoContainer = container.Q<VisualElement>(INFO_MAIN_CONTAINER_NAME);

            TwoPaneSplitView splitView = CreateSplitView(tagsContainer);
            infoContainer.Add(splitView);

            SetSearchBars();
            CreateTags(rightPanel);
        }

        private void SetSearchBars()
        {
            searchBar = new SearchBar(leftPanel, 16, true);
            searchBar.Creations.OnComponentClicked += DisplayElementPanel;
            searchBar.OnElementDeleted += SearchBar_OnElementDeleted;

            var rightSearchContainer = rightPanel.Q<VisualElement>("SearchContainer");
            rightSearchContainer.AddToClassList("Disable");
            txtSearch_Right = rightPanel.Q<TextField>("txtSearch");
        }

        private void SearchBar_OnElementDeleted(ElementType obj)
        {
            if (CreationControls[obj] is BaseInfoTracker tracker && tracker != null)
                tracker.Set_CreationState(CreationsState.Creating);

            EnableContainer(infoSetup, false);
            ChangeTab(INFO_GENERAL_SETTINGS_CHARACTER_NAME);
            if (currentSettingTag.idx >= 0)
                Highlight(btnsRight_Tag[currentSettingTag.idx].element, false);
            currentSettingTag = (ElementType.None, -1);
            editingElement = (ElementType.None, "", -1);
        }

        TwoPaneSplitView CreateSplitView(VisualTreeAsset tagsContainer)
        {
            TwoPaneSplitView splitView = new TwoPaneSplitView();
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.fixedPaneInitialDimension = 215;
            splitView.AddToClassList("SplitViewStyle");

            leftPanel = tagsContainer.Instantiate();
            rightPanel = tagsContainer.Instantiate();
            splitView.Insert(0, leftPanel);
            splitView.Insert(1, rightPanel);

            splitView.style.flexBasis = 1000;
            splitView.style.flexGrow = 1;
            splitView.style.flexShrink = 1;
            rightPanel.style.flexGrow = 1;
            rightPanel.style.flexShrink = 1;
            rightPanel.style.flexBasis = 1000;

            infoRight = rightPanel.Q<VisualElement>("elementsContainer");

            infoSetup = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/{INFO_SETUP_NAME}.uxml").Instantiate();
            nameSettings = CreateInstance<CreationsBaseInfo>();
            nameSettings.Initialize(infoSetup);
            nameSettings.CreationsStateChanged += NameSettings_CreationsStateChanged;

            EnableContainer(infoSetup, false);
            infoRight.Add(infoSetup);

            return splitView;
        }

        private void NameSettings_CreationsStateChanged(CreationsState obj)
        {
            string text = obj == CreationsState.Editing ? "Apply" : "Create";
            btnSettingAccept.text = text;
        }

        private void CreateTags(VisualElement rightContainer)
        {
            var max = SavingSystem.Data.defaultElements.Count;
            var tags = rightContainer.Q<VisualElement>("TagsContainer").Query<Button>(className: "FilterTag").ToList();
            btnsRight_Tag = new();
            int tagIdx = 0;

            foreach (var item in tags)
            {
                btnsRight_Tag.Add(new TagData(tagIdx++, item, ElementType.None));
            }
            int i = 0;

            btnsRight_Tag.ForEach(b =>
            {
                if (b.element.ClassListContains("Disable"))
                {
                    b.element.RemoveFromClassList("Disable");
                }

                if (i < max)
                {
                    var element = SavingSystem.Data.defaultElements[i];
                    b.element.text = element.ToString();
                    int j = i;

                    b.element.clicked += () => OnClicked_TagComponents(j, element);
                }
                else
                    EnableContainer(b.element, false);

                ++i;
            });
        }

        private void DisplayElementPanel(int idx)
        {
            ElementCreationPinnable element = searchBar[idx];
            var type = (ElementType)element.Type;
            var id = element.Id;

            (CreationControls[type] as BaseInfoTracker).Set_CreationState(CreationsState.Editing);
            Load_CreationData(element, type);

            btnsRight_Tag.ForEach(t => Highlight(t.element, false));

            int tagIdx = 0;
            foreach (var tag in btnsRight_Tag)
            {
                if (tag.Text == type.ToString())
                {
                    Highlight(tag.element, true);
                    currentSettingTag = (type, tagIdx);
                    break;
                }

                ++tagIdx;
            }

            EnableContainer(infoSetup, true);
            btnSettingAccept.text = "Apply";
            editingElement = (type, searchBar[idx].NameButton.text, idx);
        }

        protected override void ChangeTab(string tab)
        {
            base.ChangeTab(tab);

            if (infoContainers[curTab].type != ElementType.None)
            {
                if (CreationControls[infoContainers[curTab].type] is IEnableable e && e != null)
                    e.Enable(true);
            }

            if (infoContainers.ContainsKey(lastTab) && infoContainers[lastTab].type != ElementType.None)
            {
                if (CreationControls[infoContainers[lastTab].type] is IEnableable e && e != null)
                    e.Enable(false);
            }

            nameSettings.TxtName.Focus();
        }

        #region Events
        private void OnClicked_TagComponents(int idx, ElementType type)
        {
            if (IsHighlighted(btnsRight_Tag[idx].element))
            {
                Highlight(btnsRight_Tag[idx].element, false);
                EnableContainer(infoSetup, false);
                //CloseCurrentTab();
                ChangeTab(INFO_GENERAL_SETTINGS_CHARACTER_NAME);
                currentSettingTag = (ElementType.None, -1);
                return;
            }

            switch (type)
            {
                case ElementType.Character:
                    ChangeTab(INFO_CHARACTER_NAME);
                    break;
                case ElementType.Item:
                    ChangeTab(INFO_ITEM_SETTINGS_NAME);
                    break;
                case ElementType.Weapon:
                    ChangeTab(INFO_WEAPON_SETTINGS_NAME);
                    break;
                case ElementType.Buff:
                    ChangeTab(INFO_BUFF_SETTINGS_NAME);
                    break;
                case ElementType.Consumable:
                    ChangeTab(INFO_CONSUMABLE_SETTINGS_NAME);
                    break;
                case ElementType.Armour:
                    ChangeTab(INFO_ARMOUR_SETTINGS_NAME);
                    break;

                default: return;
            }

            EnableContainer(infoSetup, true);
            btnSettingAccept.text = "Create";
            btnsRight_Tag.ForEach(t => Highlight(t.element, false));
            Highlight(btnsRight_Tag[idx].element, true);
            currentSettingTag = (type, idx);
        }
        #endregion
    }

    public struct TagData
    {
        public int idx;
        public Button element;
        public ElementType type;

        public TagData(int idx, Button element, ElementType type)
        {
            this.idx = idx;
            this.element = element;
            this.type = type;
        }

        public string Text { get => element.text; set => element.text = value; }
    }
}
