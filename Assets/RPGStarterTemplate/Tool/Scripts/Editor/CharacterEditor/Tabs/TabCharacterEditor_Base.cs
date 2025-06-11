using Burmuruk.Tesis.Editor.Controls;
using System.Collections.Generic;
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

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/Tabs/CharacterTab.uxml");
            rootTab = visualTree.Instantiate();
            container.Add(rootTab);
            //rootTab.style.height = new StyleLength(StyleKeyword.;

            var styleSheets = new List<StyleSheet>()
            {
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/BasicSS.uss"),
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/TagSystem.uss"),
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/BorderColours.uss"),
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/LineTags.uss"),
            };

            styleSheets.ForEach(styleSheet => { container.styleSheets.Add(styleSheet); });

            GetTabButtons();
            GetNotificationSection();

            SavingSystem.Initialize();
            CreateTagsContainer();
            GetInfoContainers();
            CreateSettingTabs();

            searchBar.SearchAllElements();
            ChangeTab(INFO_GENERAL_SETTINGS_CHARACTER_NAME);
        }

        private void CreateSettingTabs()
        {
            Setup_Coponents();
            Create_BaseSettingsTab();
            Create_CharacterTab();
            Create_WeaponSettings();
            Create_ItemTab();
            Create_BuffSettings();
            //Create_HealthSettings();
            Create_ConsumableSettings();
            Create_ArmourSettings();
            Create_GeneralCharacterSettings();
        }

        protected override void GetTabButtons()
        {
            //Parent.Q<VisualElement>("Tabs").AddToClassList("Disable");
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
                    VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/Tabs/{containerData.name}.uxml").Instantiate();
                    StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/LineTags.uss");
                    StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/BorderColours.uss");
                    StyleSheet styleSheetBasic = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/BasicSS.uss");
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
            VisualTreeAsset tagsContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/TagsContainer.uxml");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/LineTags.uss");
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGStarterTemplate/Tool/UIToolkit/Styles/BorderColours.uss");

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
            searchBar.Creations.OnElementClicked += DisplayElementPanel;

            var rightSearchContainer = rightPanel.Q<VisualElement>("SearchContainer");
            rightSearchContainer.AddToClassList("Disable");
            txtSearch_Right = rightPanel.Q<TextField>("txtSearch");
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

            infoSetup = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/{INFO_SETUP_NAME}.uxml").Instantiate();
            nameSettings = CreateInstance<CreationsBaseInfo>();
            nameSettings.Initialize(infoSetup);

            EnableContainer(infoSetup, false);
            infoRight.Add(infoSetup);

            return splitView;
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
            (CreationControls[type] as BaseInfoTracker).Set_CreationState(CreationsState.Editing);
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
            DisableNotification();
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
