using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
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
        ComponentsList<ElementCreationPinable> creations;
        TextField txtSearch_Right;
        TextField txtSearch_Left;
        Button btnClearSearch;
        List<TagData> btnsLeft_Tag;
        List<TagData> btnsRight_Tag;

        VisualElement leftPanel;
        VisualElement rightPanel;
        VisualElement infoLeft;
        VisualElement infoRight;
        VisualElement infoSetup;
        VisualElement rootTab;
        static TabCharacterEditor currentWindow;

        (ElementType type, int idx) currentSettingTag = (ElementType.None, -1);

        (ElementType type, string text) lastLeftSearch = default;
        ElementType currentFilter = ElementType.None;

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
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/CharacterTab.uxml");
            rootTab = visualTree.Instantiate();
            container.Add(rootTab);
            //rootTab.style.height = new StyleLength(StyleKeyword.;

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BasicSS.uss");
            StyleSheet styleSheet2 = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/TagSystem.uss");
            container.styleSheets.Add(styleSheet);
            container.styleSheets.Add(styleSheet2);

            GetTabButtons();
            GetNotificationSection();

            LoadTagsDatabase();
            CreateTagsContainer();
            GetInfoContainers();
            CreateSettingTabs();

            SearchAllElements();
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
                    VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/{containerData.name}.uxml").Instantiate();
                    StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
                    StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BorderColours.uss");
                    newContainer.styleSheets.Add(styleSheet);
                    newContainer.styleSheets.Add(styleSheetColour);

                    infoContainers.Add(containerData.name, (newContainer, containerData.type));
                    tabNames[containerData.name] = "";
                    
                    if (shouldAdd)
                        container.Q<ScrollView>("infoContainer").Add(newContainer);

                    EnableContainer(newContainer, false);
                }
            }
        }

        private void EnableControl(bool shouldEnable)
        {
            (CreationControls[infoContainers[curTab].type] as IEnableable).Enable(shouldEnable);
        }

        private void CreateTagsContainer()
        {
            VisualTreeAsset tagsContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/TagsContainer.uxml");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BorderColours.uss");

            container.styleSheets.Add(styleSheet);
            container.styleSheets.Add(styleSheetColour);
            var infoContainer = container.Q<VisualElement>(INFO_MAIN_CONTAINER_NAME);

            TwoPaneSplitView splitView = CreateSplitView(tagsContainer);
            infoContainer.Add(splitView);

            SetSearchBars();

            CreateTags(leftPanel, rightPanel);
            Add_SearchElements(infoLeft, 16);
        }

        private void SetSearchBars()
        {
            btnClearSearch = leftPanel.Q<Button>("btnClearSearch");
            btnClearSearch.clicked += SearchAllElements;

            txtSearch_Left = leftPanel.Q<TextField>("txtSearch");
            txtSearch_Left.RegisterCallback<KeyDownEvent>(KeyDown_SearchElementCharacter);

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

            infoLeft = leftPanel.Q<VisualElement>("elementsContainer");
            infoRight = rightPanel.Q<VisualElement>("elementsContainer");

            infoSetup = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/{INFO_SETUP_NAME}.uxml").Instantiate();
            nameSettings = new CreationsBaseInfo(infoSetup);

            EnableContainer(infoSetup, false);
            infoRight.Add(infoSetup);

            return splitView;
        }

        private void CreateTags(VisualElement leftContainer, VisualElement rightContainer)
        {
            var max = SavingSystem.Data.defaultElements.Count;
            AddTags(leftContainer, ref btnsLeft_Tag, true);
            AddTags(rightContainer, ref btnsRight_Tag, false);

            void AddTags(VisualElement container, ref List<TagData> buttons, bool isFilter)
            {
                var tags = container.Q<VisualElement>("TagsContainer").Query<Button>(className: "FilterTag").ToList();
                buttons = new();

                int tagIdx = 0;
                foreach (var item in tags)
                {
                    buttons.Add(new TagData(tagIdx++, item, ElementType.None));
                }
                int i = 0;

                buttons.ForEach(b =>
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

                        if (isFilter)
                        {
                            b.element.clicked += () => OnClicked_FilterTag(j, element);
                        }
                        else
                        {
                            b.element.clicked += () => OnClicked_TagComponents(j, element);
                        }
                    }
                    else
                        EnableContainer(b.element, false);

                    ++i;
                });
            }
        }

        private void Add_SearchElements(VisualElement container, int amount)
        {
            VisualTreeAsset ElementTag = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Elements/ElementTag.uxml");
            creations = new (container);
            creations.OnElementClicked += DisplayElementPanel;
            creations.OnElementCreated += (ElementCreationPinable element) =>
            {
                element.RemoveButton.clicked += () =>
                {
                    RemoveCreation(element.idx);
                    SearchAllElements();
                };
                element.Pin.clicked += () => PinElement(element.idx);
            };
            creations.CreationValidator += delegate { return -1; };

            for (int i = 0; i < amount; ++i)
            {
                creations.AddElement(i.ToString());
            }

            //for (int i = 0; i < amount; i++)
            //{
            //    var instance = ElementTag.Instantiate();
            //    EnableContainer(instance, false);

            //    container.Add(instance);
            //    creations.AddElement()
            //    Left_Elements.Add(new ElementData(instance));
            //    int idx = i;

            //    //instance.Q<Button>("txtElement").clicked += () =>
            //    //{
            //    //    DisplayElementPanel(idx);
            //    //};
            //    //instance.Q<Button>("btnPin").clicked += () =>
            //    //{
            //    //    PinElement(idx);
            //    //};
            //    //instance.Q<Button>("btnDelete").clicked += () =>
            //    //{
            //    //    RemoveCreation(idx);
            //    //};
            //}
        }

        private void RemoveCreation(int elementIdx)
        {
            var type = (ElementType)creations[elementIdx].Type;
            string id = creations[elementIdx].Id;
            SavingSystem.Remove(type, id);
            
            Notify("Element deleted.", BorderColour.Approved);
        }

        private void DisplayElementPanel(int idx)
        {
            ElementCreationPinable element = creations[idx];
            var type = (ElementType)element.Type;
            var id = element.Id;

            Load_CreationData(element, type);

            btnsRight_Tag.ForEach(t => Highlight(t.element, false));
            nameSettings.TxtName.value = creations[idx].NameButton.text;
            nameSettings.Colour.value = Color.black;

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
            //infoSetup.Q<Button>("btnState").text = "Editing";
            //settingsState = CreationsState.Editing;
            (CreationControls[type] as BaseInfoTracker).Set_CreationState(CreationsState.Editing);
            btnSettingAccept.text = "Apply";
            editingElement = (type, creations[idx].NameButton.text, idx);
        }

        private void LoadTagsDatabase()
        {
            SavingSystem.Initialize();
            //charactersLists = (AssetDatabase.FindAssets(typeof(CharacterTag).ToString(), new[] { "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset" })
            //    .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterTag>(AssetDatabase.GUIDToAssetPath(guid)))
            //    .ToList()).FirstOrDefault();

            //if (charactersLists == null)
            //{
            //    Notify("No labels found.", BorderColour.Error);

            //    charactersLists = ScriptableObject.CreateInstance<CharacterTag>();
            //    AssetDatabase.CreateAsset(charactersLists, "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset");
            //    AssetDatabase.SaveAssets();
            //    AssetDatabase.Refresh();
            //}
            //else
            //{
            //    Notify("Labels found.", BorderColour.Approved);
            //}
        }

        private void SearchFilterCharacter(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                string text = txtSearch_Right.value.Trim();

                if (string.IsNullOrEmpty(text))
                    return;

                foreach (var character in SavingSystem.Data.characters)
                {
                    if (character.name == text)
                    {
                        //Enable_CharacterFilterTags(text);
                        break;
                    }
                }
            }
        }

        private bool TryEnableCharacterElements(List<(ElementType type, List<string> ids)> names)
        {
            bool enabled = false;

            if (names == null || names.Count == 0) return false;

            for (int i = 0; i < creations.Components.Count; i++)
            {
            nextTurn:
                if (creations.Components[i].pinned)
                {
                    if (names.Count <= 0) continue;

                    for (int k = 0; k < names[0].ids.Count; k++)
                    {
                        if (creations.Components[i].NameButton.text.Contains(GetName(k)))
                        {
                            enabled = true;
                            RemoveName(k);
                            ++i;
                            goto nextTurn;
                        }
                    }

                    continue;
                }

                if (names.Count > 0)
                {
                    creations.Components[i].SetInfo(false, names[0].type, names[0].ids[0], GetName(0));
                    EnableContainer(creations.Components[i].element, true);

                    if (tglShowElementColour.value)
                    {
                        if (tglShowCustomColour.value)
                        {
                            //Show custom colour on element
                        }
                        else
                            Highlight(creations.Components[i].element, true, GetElementColour(names[0].type));
                    }

                    enabled = true;
                    RemoveName(0);
                }
                else
                {
                    creations.Components[i].NameButton.text = "";
                    EnableContainer(creations.Components[i].element, false);
                }
            }

            return enabled;

            string GetName(int idx) =>
                SavingSystem.Data.creations[names[0].type][names[0].ids[idx]].Name;

            void RemoveName(int idx)
            {
                names[0].ids.RemoveAt(idx);

                if (names[0].ids.Count <= 0)
                {
                    names.RemoveAt(0);
                }
            }
        }

        private void SearchElementTag(string text, ElementType searchType)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (currentFilter != ElementType.None) searchType = currentFilter;

            List<(ElementType, List<string>)> idsFound = null;

            if (searchType == ElementType.None)
            {
                int count = Enum.GetValues(typeof(ElementType)).Length;

                for (int i = 0; i < count; i++)
                {
                    if (!SavingSystem.Data.creations.ContainsKey((ElementType)i))
                        continue;

                    if (FindValues(text, (ElementType)i, out List<string> found))
                    {
                        Debug.Log("Found without filter");
                        idsFound ??= new();

                        idsFound.Add(((ElementType)i, found));
                    }
                }
            }
            else if (FindValues(text, searchType, out List<string> found))
            {
                Debug.Log("found int filter");
                idsFound ??= new();
                idsFound.Add((searchType, found));
            }

            if (idsFound != null && idsFound.Count > 0)
            {
                Debug.Log("somenting found");
                TryEnableCharacterElements(idsFound);
                lastLeftSearch = (searchType, text);
            }
            else
            {
                Debug.Log("no items found");
                DisableElements();
                lastLeftSearch = (ElementType.None, "");
            }

            bool FindValues(string text, ElementType type, out List<string> valuesIds)
            {
                valuesIds = (from c in SavingSystem.Data.creations[type]
                             where c.Value.Name.ToLower().Contains(text.ToLower())
                             select c.Key).ToList();

                return valuesIds.Count > 0;
            }
        }

        private void DisableElements()
        {
            foreach (var element in creations.Components)
            {
                if (element.pinned) continue;

                if (element.element.ClassListContains("Disable"))
                    return;

                EnableContainer(element.element, false);
            }
        }

        private void SearchAllElements()
        {
            SearchAllElements(ElementType.None);
        }

        private void SearchAllElements(ElementType type)
        {
            if (currentFilter != ElementType.None)
                type = currentFilter;

            lastLeftSearch = (ElementType.None, "");
            txtSearch_Left.value = "";
            List<(ElementType type, List<string> ids)> values = new();

            if (type == ElementType.None)
            {
                foreach (var curType in SavingSystem.Data.creations.Keys)
                {
                    var ids = SavingSystem.Data.creations[curType].Select(creation => creation.Key).ToList();

                    if (ids != null && ids.Count > 0)
                        values.Add((curType, ids));
                }
            }
            else if (SavingSystem.Data.creations.ContainsKey(type))
            {
                var ids = SavingSystem.Data.creations[type].Select(creation => creation.Key).ToList();

                if (ids != null && ids.Count > 0)
                    values.Add((type, ids));
            }
            else
            {
                DisableElements();
                return;
            }

            if (values.Count > 0)
            {
                TryEnableCharacterElements(values);
            }
            else
                DisableElements();
        }

        protected override void ChangeTab(string tab)
        {
            base.ChangeTab(tab);

            if (!string.IsNullOrEmpty(nameSettings.TxtName.value))
                VerifyName(nameSettings.TxtName.value);

            if (infoContainers[curTab].type != ElementType.None)
            {
                if (CreationControls[infoContainers[curTab].type] is BaseInfoTracker t && t != null)
                    t.UpdateName();

                if (CreationControls[infoContainers[curTab].type] is IEnableable e && e != null)
                    e.Enable(true);
            }

            if (infoContainers.ContainsKey(lastTab) && infoContainers[lastTab].type != ElementType.None)
            {
                if (CreationControls[infoContainers[lastTab].type] is ISubWindowsContainer c && c != null)
                    c.CloseWindows();

                if (CreationControls[infoContainers[lastTab].type] is IEnableable e && e != null)
                    e.Enable(false);
            }

            nameSettings.TxtName.Focus();
            DisableNotification();
        }

        #region Events
        private void KeyDown_SearchElementCharacter(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return) return;

            string text = txtSearch_Left.value.Trim();

            SearchElementTag(text, currentFilter);
            txtSearch_Left.Focus();
        }

        private void OnClicked_FilterTag(int idx, ElementType type)
        {
            if (IsHighlighted(btnsLeft_Tag[idx].element))
            {
                currentFilter = ElementType.None;
                Highlight(btnsLeft_Tag[idx].element, false);

                if (lastLeftSearch.type == ElementType.None)
                    SearchAllElements();
                else
                    SearchElementTag(txtSearch_Left.value, currentFilter);
            }
            else
            {
                currentFilter = type;

                btnsLeft_Tag.ForEach(b => Highlight(b.element, false));
                Highlight(btnsLeft_Tag[idx].element, true);

                if (lastLeftSearch.type == ElementType.None)
                {
                    Debug.Log("Search filter");
                    SearchAllElements(type);
                }
                else
                {
                    Debug.Log("Search last " + lastLeftSearch.type + " Filter: " + currentFilter);
                    SearchElementTag(txtSearch_Left.value, lastLeftSearch.type);
                }
            }
        }

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

            //if (settingsState == SettingsState.Editing)
            //    OnCancel_BtnSetting();

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
            //infoSetup.Q<Button>("btnState").text = "Creating";
            //settingsState = CreationsState.Creating;
            btnSettingAccept.text = "Create";

            btnsRight_Tag.ForEach(t => Highlight(t.element, false));
            Highlight(btnsRight_Tag[idx].element, true);
            currentSettingTag = (type, idx);
        }

        private void PinElement(int elementIdx)
        {
            if (creations[elementIdx].pinned)
            {
                RemovePin(elementIdx);
                return;
            }

            for (int i = 0; i < creations.Components.Count; i++)
            {
                if (IsDisabled(creations[i].element)) break;

                if (!creations[i].pinned)
                {
                    if (i == elementIdx)
                    {
                        Pin(elementIdx);
                        return;
                    }
                    else
                    {
                        SwapPin(elementIdx, i);
                        Pin(elementIdx, false);
                        Pin(i);
                    }
                    return;
                }
            }
        }

        private void RemovePin(int elementIdx)
        {
            Displace_Pins(elementIdx);

            if (lastLeftSearch.type == ElementType.None)
                SearchAllElements();
            else
                SearchElementTag(lastLeftSearch.text, lastLeftSearch.type);

            return;
        }

        private void Displace_Pins(int elementIdx)
        {
            int last = 0;

            for (int i = elementIdx + 1; i < creations.Components.Count; i++)
            {
                if (IsDisabled(creations[i].element))
                {
                    last = i - 1;
                    break;
                }

                if (!creations[i].pinned)
                { 
                    if (i == elementIdx + 1)
                    {
                        Pin(elementIdx, false);
                    }
                    else
                    {
                        SwapPin(i - 1, elementIdx);
                        Pin(i - 1, false);
                    }
                    return;
                }
            }

            if (last == elementIdx)
            {
                Pin(elementIdx, false);
            }
            else
            {
                SwapPin(elementIdx, last);
                Pin(last, false);
            }
        }

        private void Pin(int elementIdx, bool shouldPin = true)
        {
            if (shouldPin)
            {
                Highlight(creations[elementIdx].Pin, true);
            }
            else
            {
                Highlight(creations[elementIdx].Pin, true, BorderColour.LightBorder);
            }

            creations[elementIdx].pinned = shouldPin;
        }

        private void SwapPin(int lastTarget, int target)
        {
            creations[lastTarget].Swap_BasicInfoWith(creations[target]);
            //var lastData = creations[target];
            //lastData.element = creations[lastTarget].element;
            //lastData.pinned = false;
            //var newData = creations[lastTarget];
            //newData.element = creations[target].element;
            //newData.pinned = true;

            //creations[lastTarget] = lastData;
            ////creations[lastTarget].ResetExtraValues();
            //creations[target] = newData;
            ////creations[target].ResetExtraValues();

            //var lastText = creations[lastTarget].NameButton.text;
            //creations[lastTarget].NameButton.text = creations[target].NameButton.text;
            //creations[target].NameButton.text = lastText;
        }

        private BorderColour GetElementColour(ElementType type) =>
            type switch
            {
                ElementType.Character => BorderColour.CharacterBorder,
                //ElementType.State => BorderColour.StateBorder,
                ElementType.Buff => BorderColour.BuffBorder,
                ElementType.Armour => BorderColour.ArmorBorder,
                ElementType.Weapon => BorderColour.WeaponBorder,
                ElementType.Consumable => BorderColour.ConsumableBorder,
                ElementType.Item => BorderColour.ItemBorder,
                _ => BorderColour.None
            };
        #endregion
    }
}
