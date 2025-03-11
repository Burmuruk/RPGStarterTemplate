using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        VisualElement infoMainContainer;
        const string INFO_MAIN_CONTAINER_NAME = "infoMainContainer";
        const string INFO_CLASS_NAME = "infoClassContainer";
        const string INFO_CHARACTER_NAME = "CharacterSettings";
        const string INFO_ITEM_SETTINGS_NAE = "ItemSettings";
        const string INFO_WEAPON_SETTINGS_NAME = "WeaponSettings";
        const string INFO_BUFF_SETTINGS_NAME = "BuffSettings";
        const string INFO_CONSUMABLE_SETTINGS_NAME = "ConsumableSettings";
        const string INFO_ARMOUR_SETTINGS_NAME = "ArmorSettings";
        const string INFO_HEALTH_SETTINGS_NAME = "HealthSettings";
        const string INFO_EQUIPMENT_SETTINGS_NAME = "EquipmentSettings";
        const string INFO_INVENTORY_SETTINGS_NAME = "InventorySettings";
        const string INFO_GENERAL_SETTINGS_CHARACTER_NAME = "GeneralSettingsCharacter";

        const string INFO_DIALOGUES_NAME = "infoDialoguesContainer";
        const string INFO_SETUP_NAME = "InfoBase";

        CharacterTag charactersLists;
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
        (ElementType type, string text) lastRightSearch = default;
        ElementType currentFilter = ElementType.None;
        ElementType rightSearchType = ElementType.None;

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
            Create_BaseSettingsTab();
            Create_CharacterTab();
            Create_WeaponSettings();
            Create_ItemTab();
            Create_BuffSettings();
            Create_HealthSettings();
            Create_EquipmentSettings();
            Create_ConsumableSettings();
            Create_ArmourSettings();
            Create_InventorySettings();
            Create_GeneralCharacterSettings();
        }

        protected override void GetTabButtons()
        {
            //Parent.Q<VisualElement>("Tabs").AddToClassList("Disable");
        }

        protected override void GetInfoContainers()
        {
            infoMainContainer = container.Q<VisualElement>(INFO_MAIN_CONTAINER_NAME);

            AddContainer(new string[]
            {
                INFO_CHARACTER_NAME,
                INFO_ITEM_SETTINGS_NAE,
                INFO_WEAPON_SETTINGS_NAME,
                INFO_BUFF_SETTINGS_NAME,
                INFO_ARMOUR_SETTINGS_NAME,
                INFO_CONSUMABLE_SETTINGS_NAME,
                INFO_HEALTH_SETTINGS_NAME,
                INFO_EQUIPMENT_SETTINGS_NAME,
                INFO_INVENTORY_SETTINGS_NAME,
            }, infoSetup);

            AddContainer(new string[] { INFO_GENERAL_SETTINGS_CHARACTER_NAME }, infoRight, false);
            infoRight.Add(infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME]);

            void AddContainer(string[] names, VisualElement container, bool shouldAdd = true)
            {
                foreach (var containerName in names)
                {
                    VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/{containerName}.uxml").Instantiate();

                    infoContainers.Add(containerName, newContainer);
                    tabNames[containerName] = "";

                    if (shouldAdd)
                        container.Q<ScrollView>("infoContainer").Add(newContainer);

                    EnableContainer(newContainer, false);
                }
            }
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
            txtNameCreation = infoSetup.Q<TextField>(TXT_CREATION_NAME);
            CFCreationColor = infoSetup.Q<ColorField>(CREATION_COLOUR_NAME);

            txtNameCreation.RegisterCallback<KeyUpEvent>(OnKeyUp_txtNameCreation);
            CFCreationColor.RegisterValueChangedCallback(OnValueChanged_CFCreationColour);

            EnableContainer(infoSetup, false);
            infoRight.Add(infoSetup);

            return splitView;
        }

        private void OnValueChanged_CFCreationColour(ChangeEvent<Color> evt)
        {
            characterData.color = evt.newValue;
        }

        private void OnKeyUp_txtNameCreation(KeyUpEvent evt)
        {
            Verify_TxtCreationName();

            tabNames[curTab] = txtNameCreation.value;
        }

        private void Verify_TxtCreationName()
        {
            if (regName.IsMatch(txtNameCreation.value))
            {
                characterData.characterName = txtNameCreation.value;
                Highlight(txtNameCreation, false);
                DisableNotification();
            }
            else
            {
                Highlight(txtNameCreation, true, BorderColour.Error);
                Notify("The name is not allowed.", BorderColour.Error);
            }
        }

        private void CreateTags(VisualElement leftContainer, VisualElement rightContainer)
        {
            var max = charactersLists.defaultElements.Count;
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
                        var element = charactersLists.defaultElements[i];
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
            creations = new(container, Notify);
            creations.bindElementBtn += DisplayElementPanel;
            creations.OnElementCreated += (ElementCreationPinable element) =>
            {
                element.RemoveButton.clicked += () => RemoveCreation(element._idx);
                element.Pin.clicked += () => PinElement(element._idx);
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

        private void RemoveCreation(int idx)
        {
            var type = (ElementType)creations[idx].Type;
            var name = creations[idx].NameButton.text;

            charactersLists.elements[type].Remove(name);
            charactersLists.GetCreation(name, type, out string id);
            charactersLists.creations[type].Remove(id);
            SearchAllElements();
            Notify("Element deleted.", BorderColour.Approved);
        }

        private void DisplayElementPanel(int idx)
        {
            var type = (ElementType)creations[idx].Type;
            Load_CreationData(idx, type);

            btnsRight_Tag.ForEach(t => Highlight(t.element, false));
            txtNameCreation.value = creations[idx].NameButton.text;
            CFCreationColor.value = Color.black;

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
            infoSetup.Q<Label>("txtState").text = "Editing";
            settingsState = SettingsState.Editing;
            btnSettingAccept.text = "Apply";
            editingElement = (type, creations[idx].NameButton.text, idx);
        }

        private void LoadTagsDatabase()
        {
            charactersLists = (AssetDatabase.FindAssets(typeof(CharacterTag).ToString(), new[] { "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterTag>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList()).FirstOrDefault();

            if (charactersLists == null)
            {
                Notify("No labels found.", BorderColour.Error);

                charactersLists = ScriptableObject.CreateInstance<CharacterTag>();
                AssetDatabase.CreateAsset(charactersLists, "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Notify("Labels found.", BorderColour.Approved);
            }
        }

        private void SearchFilterCharacter(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                string text = txtSearch_Right.value.Trim();

                if (string.IsNullOrEmpty(text))
                    return;

                foreach (var character in charactersLists.characters)
                {
                    if (character.name == text)
                    {
                        //Enable_CharacterFilterTags(text);
                        break;
                    }
                }
            }
        }

        private bool TryEnableCharacterElements(out List<(int, ElementType, int)> enabledButtonsIdx, params (ElementType type, int idx)[] names)
        {
            return TryEnableCharacterElements(out enabledButtonsIdx, names.ToList());
        }

        private bool TryEnableCharacterElements(out List<(int elementIdx, ElementType type, int valueIdx)> enabledButtonsIdx, List<(ElementType type, int idx)> namesList)
        {
            bool enabled = false;
            enabledButtonsIdx = null;

            if (namesList == null || namesList.Count == 0) return false;

            for (int i = 0; i < creations.Components.Count; i++)
            {
            nextTurn:

                if (creations.Components[i].pinned)
                {
                    if (namesList.Count <= 0) continue;

                    for (int k = 0; k < namesList.Count; k++)
                    {
                        if (creations.Components[i].NameButton.text.Contains(GetName(k)))
                        {
                            enabled = true;
                            namesList.RemoveAt(k);
                            ++i;
                            goto nextTurn;
                        }
                    }

                    continue;
                }

                if (namesList.Count > 0)
                {
                    creations.Components[i].NameButton.text = GetName(0);
                    EnableContainer(creations.Components[i].element, true);

                    if (tglShowElementColour.value)
                        Highlight(creations.Components[i].element, true, GetElementColour(namesList[0].type));

                    if (tglShowCustomColour.value)
                    {
                        //Show custom colour in element
                    }

                    (enabledButtonsIdx ??= new()).Add((i, namesList[0].type, namesList[0].idx));
                    enabled = true;
                    namesList.RemoveAt(0);
                }
                else
                {
                    creations.Components[i].NameButton.text = "";
                    EnableContainer(creations.Components[i].element, false);
                }
            }

            return enabled;

            string GetName(int idx)
            {
                return charactersLists.elements[namesList[idx].type][namesList[idx].idx];
            }
        }

        private void SearchElementTag(string text, ElementType searchType)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (currentFilter != ElementType.None) searchType = currentFilter;

            List<(ElementType type, int idx)> idxFound = null;

            if (searchType == ElementType.None)
            {
                int count = Enum.GetValues(typeof(ElementType)).Length;

                for (int i = 0; i < count; i++)
                {
                    if (!charactersLists.elements.ContainsKey((ElementType)i))
                        continue;

                    if (FindValues(text, (ElementType)i, out List<int> found))
                    {
                        Debug.Log("Found in None");
                        idxFound ??= new();
                        found.ForEach(value => idxFound.Add(((ElementType)i, value)));
                    }
                }
            }
            else if (FindValues(text, searchType, out List<int> found))
            {
                Debug.Log("found int filter");
                idxFound ??= new();
                found.ForEach(value => idxFound.Add((searchType, value)));
            }

            if (idxFound != null && idxFound.Count > 0)
            {
                Debug.Log("somenting found");
                TryEnableCharacterElements(out List<(int, ElementType, int)> idxs, idxFound);
                UpdateElementsData(idxs);

                lastLeftSearch = (searchType, text);
            }
            else
            {
                Debug.Log("no items found");
                DisableElements();
                lastLeftSearch = (ElementType.None, "");
            }

            bool FindValues(string text, ElementType type, out List<int> valuesIdx)
            {
                valuesIdx = new List<int>();
                int i = 0;

                foreach (var element in charactersLists.elements[type])
                {
                    if (element.ToLower().Contains(text.ToLower()))
                    {
                        valuesIdx.Add(i);
                    }

                    ++i;
                }

                return valuesIdx.Count > 0;
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

        void UpdateElementsData(List<(int elementIdx, ElementType type, int valueIdx)> idxs)
        {
            if (idxs == null) return;

            for (int i = 0; i < idxs.Count; i++)
            {
                var cur = creations[idxs[i].elementIdx];

                cur.element = creations[idxs[i].elementIdx].element;
                cur.Type = idxs[i].type;
                cur._idx = idxs[i].valueIdx;
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
            List<(ElementType type, int idx)> values = new();

            if (type == ElementType.None)
            {
                int i = 0;
                foreach (var elementData in charactersLists.elements)
                {
                    for (int j = 0; j < elementData.Value.Count; j++)
                    {
                        values.Add((elementData.Key, j));
                    }

                    ++i;
                }
            }
            else if (charactersLists.elements.ContainsKey(type))
            {
                for (int i = 0; i < charactersLists.elements[type].Count; i++)
                {
                    values.Add((type, i));
                }
            }
            else
            {
                DisableElements();
                return;
            }

            if (values.Count > 0)
            {
                TryEnableCharacterElements(out List<(int, ElementType, int)> idxs, values);
                UpdateElementsData(idxs);
            }
            else
                DisableElements();
        }

        protected override void ChangeTab(string tab)
        {
            if (IsHighlighted(txtNameCreation, out _))
            {
                Highlight(txtNameCreation, false);
                DisableNotification();
            }

            base.ChangeTab(tab);
            txtNameCreation.SetValueWithoutNotify(tabNames[tab]);

            if (!string.IsNullOrEmpty(txtNameCreation.value))
                Verify_TxtCreationName();
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
                    ChangeTab(INFO_ITEM_SETTINGS_NAE);
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
            infoSetup.Q<Label>("txtState").text = "Creating";
            btnSettingAccept.text = "Create";
            settingsState = SettingsState.Creating;

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
                if (!creations[i].pinned)
                {
                    SwapPin(elementIdx, i);
                    break;
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
            bool moved = false;

            for (int i = elementIdx + 1; i < creations.Components.Count; i++)
            {
                if (creations[i].pinned)
                {
                    SwapPin(i, i - 1);
                    moved = true;
                }
                else
                    break;
            }

            if (!moved)
            {
                var data = creations[elementIdx];
                data.pinned = false;
                creations[elementIdx] = data;
                Highlight(creations[elementIdx].Pin, false);
            }
        }

        private void SwapPin(int lastTarget, int target)
        {
            var lastData = creations[target];
            lastData.element = creations[lastTarget].element;
            lastData.pinned = false;
            var newData = creations[lastTarget];
            newData.element = creations[target].element;
            newData.pinned = true;

            creations[lastTarget] = lastData;
            //creations[lastTarget].ResetExtraValues();
            creations[target] = newData;
            //creations[target].ResetExtraValues();

            var lastText = creations[lastTarget].NameButton.text;
            creations[lastTarget].NameButton.text = creations[target].NameButton.text;
            creations[target].NameButton.text = lastText;

            Highlight(creations[lastTarget].Pin, false);
            Highlight(creations[target].Pin, true);
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
