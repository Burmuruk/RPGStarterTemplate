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
        const string infoMainContainerName = "infoMainContainer";
        const string infoClassName = "infoClassContainer";
        const string infoCharacterName = "CharacterSettings";
        const string infoItemSettingsName = "ItemSettings";
        const string infoWeaponSettingsName = "WeaponSettings";
        const string infoBuffSettingsName = "BuffSettings";
        const string infoConsumableSettingsName = "ConsumableSettings";
        const string infoArmourSettingsName = "ArmorSettings";
        const string infoHealthSettingsName = "HealthSettings";
        const string infoEquipmentSettingsName = "EquipmentSettings";
        const string infoInventorySettingsName = "InventorySettings";
        const string infoGeneralSettinsCharacterName = "GeneralSettingsCharacter";

        const string infoDialoguesName = "infoDialoguesContainer";
        const string infoSetupName = "InfoBase";

        CharacterTag charactersLists;
        TextField txtSearch_Right;
        TextField txtSearch_Left;
        Button btnClearSearch;
        List<TagData> btnsLeft_Tag;
        List<TagData> btnsRight_Tag;
        List<ElementData> Left_Elements;

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

        public struct ElementData
        {
            public VisualElement element;
            public ElementType type;
            public int? valueIdx;
            public bool pinned;
            private Button button;
            private Button pin;

            public ElementData(VisualElement element)
            {
                this.element = element;
                type = ElementType.None;
                valueIdx = 0;
                pinned = false;
                button = null;
                pin = null;
            }

            public Button Button
            {
                get
                {
                    if (button == null)
                        button = element.Q<Button>("txtElement");

                    return button;
                }
            }

            public Button Pin
            {
                get
                {
                    if (pin == null)
                        pin = element.Q<Button>("btnPin");

                    return pin;
                }
            }

            public void ResetExtraValues()
            {
                button = null;
                pin = null;
            }
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
            ChangeTab(infoGeneralSettinsCharacterName);
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
            //container.Q<VisualElement>("Tabs").AddToClassList("Disable");
        }

        protected override void GetInfoContainers()
        {
            infoMainContainer = container.Q<VisualElement>(infoMainContainerName);

            AddContainer(new string[]
            {
                infoCharacterName,
                infoItemSettingsName,
                infoWeaponSettingsName,
                infoBuffSettingsName,
                infoArmourSettingsName,
                infoConsumableSettingsName,
                infoHealthSettingsName,
                infoEquipmentSettingsName,
                infoInventorySettingsName,
            }, infoSetup);

            AddContainer(new string[] { infoGeneralSettinsCharacterName }, infoRight, false);
            infoRight.Add(infoContainers[infoGeneralSettinsCharacterName]);

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
            var infoContainer = container.Q<VisualElement>(infoMainContainerName);

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

            infoSetup = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/{infoSetupName}.uxml").Instantiate();
            txtNameCreation = infoSetup.Q<TextField>(txtCreationName);
            CFCreationColor = infoSetup.Q<ColorField>(creationColorName);

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
            Left_Elements = new();

            for (int i = 0; i < amount; i++)
            {
                var instance = ElementTag.Instantiate();
                EnableContainer(instance, false);

                container.Add(instance);
                Left_Elements.Add(new ElementData(instance));
                int idx = i;

                instance.Q<Button>("txtElement").clicked += () =>
                {
                    DisplayElementPanel(idx);
                };
                instance.Q<Button>("btnPin").clicked += () =>
                {
                    PinElement(idx);
                };
                instance.Q<Button>("btnDelete").clicked += () =>
                {
                    RemoveCreation(idx);
                };
            }
        }

        private void RemoveCreation(int idx)
        {
            var type = Left_Elements[idx].type;
            var name = Left_Elements[idx].Button.text;
            charactersLists.elements[type].Remove(name);
            charactersLists.creations[type].Remove(name);
            SearchAllElements();
            Notify("Element deleted.", BorderColour.Approved);
        }

        private void DisplayElementPanel(int idx)
        {
            switch (Left_Elements[idx].type)
            {
                case ElementType.Item:
                    ChangeTab(infoItemSettingsName);

                    Load_ElementBaseData<InventoryItem>(ElementType.Item, creations[idx].BtnEditComponent.text);
                    break;

                case ElementType.Character:
                    LoadChanges_Character(Left_Elements[idx].Button.text);
                    ChangeTab(infoCharacterName);
                    break;

                case ElementType.Buff:
                    ChangeTab(infoBuffSettingsName);

                    break;

                //case ElementType.Mod:
                //    break;

                //case ElementType.State:
                //    break;

                case ElementType.Ability:
                    var ability = Load_ElementBaseData<Ability>(ElementType.Ability, creations[idx].BtnEditComponent.text);
                    Load_AbilityData(ability);
                    break;

                case ElementType.Creation:
                    break;

                case ElementType.Weapon:
                    ChangeTab(infoWeaponSettingsName);

                    var weapon = Load_ElementBaseData<Weapon>(ElementType.Weapon, creations[idx].BtnEditComponent.text);
                    Load_WeaponData(weapon);
                    break;

                case ElementType.Armour:
                    ChangeTab(infoArmourSettingsName);

                    var armour = Load_ElementBaseData<ArmourElement>(ElementType.Armour, creations[idx].BtnEditComponent.text);
                    Load_ArmourData(armour);
                    break;

                case ElementType.Consumable:
                    ChangeTab(infoConsumableSettingsName);

                    var consumable = Load_ElementBaseData<ConsumableItem>(ElementType.Consumable, creations[idx].BtnEditComponent.text);
                    Load_ConsumableData(consumable);
                    break;

                default:
                    return;
            }

            btnsRight_Tag.ForEach(t => Highlight(t.element, false));
            var type = Left_Elements[idx].type.ToString();

            int tagIdx = 0;
            foreach (var tag in btnsRight_Tag)
            {
                if (tag.Text == type)
                {
                    Highlight(tag.element, true);
                    currentSettingTag = (Left_Elements[idx].type, tagIdx);
                    break;
                }

                ++tagIdx;
            }

            EnableContainer(infoSetup, true);
            infoSetup.Q<Label>("txtState").text = "Editing";
            settingsState = SettingsState.Editing;
            btnSettingAccept.text = "Apply";
            editingElement = (Left_Elements[idx].type, Left_Elements[idx].Button.text, idx);
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

        private bool EnableCharacterElements(out List<(int, ElementType, int)> enabledButtonsIdx, params (ElementType type, int idx)[] names)
        {
            return EnableCharacterElements(out enabledButtonsIdx, names.ToList());
        }

        private bool EnableCharacterElements(out List<(int elementIdx, ElementType type, int valueIdx)> enabledButtonsIdx, List<(ElementType type, int idx)> namesList)
        {
            bool enabled = false;
            enabledButtonsIdx = null;

            if (namesList == null || namesList.Count == 0) return false;

            for (int i = 0; i < Left_Elements.Count; i++)
            {
            nextTurn:

                if (Left_Elements[i].pinned)
                {
                    if (namesList.Count <= 0) continue;

                    for (int k = 0; k < namesList.Count; k++)
                    {
                        if (Left_Elements[i].Button.text.Contains(GetName(k)))
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
                    Left_Elements[i].Button.text = GetName(0);
                    EnableContainer(Left_Elements[i].element, true);

                    if (tglShowElementColour.value)
                        Highlight(Left_Elements[i].element, true, GetElementColour(namesList[0].type));

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
                    Left_Elements[i].Button.text = "";
                    EnableContainer(Left_Elements[i].element, false);
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
                EnableCharacterElements(out List<(int, ElementType, int)> idxs, idxFound);
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
            foreach (var element in Left_Elements)
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
                ElementData newElementData = new()
                {
                    element = Left_Elements[idxs[i].elementIdx].element,
                    type = idxs[i].type,
                    valueIdx = idxs[i].valueIdx,
                };

                Left_Elements[idxs[i].elementIdx] = newElementData;
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
                EnableCharacterElements(out List<(int, ElementType, int)> idxs, values);
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
                ChangeTab(infoGeneralSettinsCharacterName);
                currentSettingTag = (ElementType.None, -1);
                return;
            }

            //if (settingsState == SettingsState.Editing)
            //    OnCancel_BtnSetting();

            switch (type)
            {
                case ElementType.Character:
                    ChangeTab(infoCharacterName);
                    break;
                case ElementType.Item:
                    ChangeTab(infoItemSettingsName);
                    break;
                case ElementType.Weapon:
                    ChangeTab(infoWeaponSettingsName);
                    break;
                case ElementType.Buff:
                    ChangeTab(infoBuffSettingsName);
                    break;
                case ElementType.Consumable:
                    ChangeTab(infoConsumableSettingsName);
                    break;
                case ElementType.Armour:
                    ChangeTab(infoArmourSettingsName);
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
            if (Left_Elements[elementIdx].pinned)
            {
                RemovePin(elementIdx);
                return;
            }

            for (int i = 0; i < Left_Elements.Count; i++)
            {
                if (!Left_Elements[i].pinned)
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

            for (int i = elementIdx + 1; i < Left_Elements.Count; i++)
            {
                if (Left_Elements[i].pinned)
                {
                    SwapPin(i, i - 1);
                    moved = true;
                }
                else
                    break;
            }

            if (!moved)
            {
                var data = Left_Elements[elementIdx];
                data.pinned = false;
                Left_Elements[elementIdx] = data;
                Highlight(Left_Elements[elementIdx].Pin, false);
            }
        }

        private void SwapPin(int lastTarget, int target)
        {
            var lastData = Left_Elements[target];
            lastData.element = Left_Elements[lastTarget].element;
            lastData.pinned = false;
            var newData = Left_Elements[lastTarget];
            newData.element = Left_Elements[target].element;
            newData.pinned = true;

            Left_Elements[lastTarget] = lastData;
            Left_Elements[lastTarget].ResetExtraValues();
            Left_Elements[target] = newData;
            Left_Elements[target].ResetExtraValues();

            var lastText = Left_Elements[lastTarget].Button.text;
            Left_Elements[lastTarget].Button.text = Left_Elements[target].Button.text;
            Left_Elements[target].Button.text = lastText;

            Highlight(Left_Elements[lastTarget].Pin, false);
            Highlight(Left_Elements[target].Pin, true);
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
