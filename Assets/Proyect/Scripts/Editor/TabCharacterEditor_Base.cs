using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string btnClassName = "btnClass";
        const string btnCharacterName = "btnCharacter";
        const string btnComponentName = "btnComponent";
        const string btnDialogueName = "btnDialogue";

        const string infoClassName = "infoClassContainer";
        const string infoCharacterName = "infoCharacterContainer";
        const string infoComponentsName = "infoComponentsContainer";
        const string infoDialoguesName = "infoDialoguesContainer";

        CharacterTag charactersLists;
        TextField txtSearch_Right;
        TextField txtSearch_Left;
        List<Button> btnsLeft_Tag;
        List<Button> btnsRight_Tag;
        List<ElementData> Left_Elements;

        VisualElement leftPanel;
        VisualElement rightPanel;
        ScrollView infoLeft;
        ScrollView infoRight;

        (ElementType type, string text) lastLeftSearch = default;
        (ElementType type, string text) lastRightSearch = default;
        ElementType leftSearchType = ElementType.None;
        ElementType rightSearchType = ElementType.None;

        public struct ElementData
        {
            public VisualElement element;
            public ElementType type;
            public int? valueIdx;
            public bool pinned;

            public ElementData(VisualElement element)
            {
                this.element = element;
                type = ElementType.None;
                valueIdx = 0;
                pinned = false;
            }
        }

        [MenuItem("LevelEditor/CharacterCreator")]
        public static void ShowWindow()
        {
            TabCharacterEditor window = GetWindow<TabCharacterEditor>();
            window.titleContent = new GUIContent("Character creation");
            window.minSize = new Vector2(400, 300);
        }

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterTab.uxml");
            container.Add(visualTree.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/BasicSS.uss");
            StyleSheet styleSheet2 = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/TagSystem.uss");
            container.styleSheets.Add(styleSheet);
            container.styleSheets.Add(styleSheet2);

            GetTabButtons();
            GetInfoContainers();
            GetNotificationSection();

            CreateTagsContainer();
            LoadTags();

            ChangeTab(infoCharacterName);
        }

        protected override void GetTabButtons()
        {
            tabButtons.Add(btnClassName, container.Q<Button>(btnClassName));
            tabButtons[btnClassName].clicked += Show_Class;

            tabButtons.Add(btnCharacterName, container.Q<Button>(btnCharacterName));
            tabButtons[btnCharacterName].clicked += Show_Character;

            tabButtons.Add(btnComponentName, container.Q<Button>(btnComponentName));
            tabButtons[btnComponentName].clicked += Show_Components;

            tabButtons.Add(btnDialogueName, container.Q<Button>(btnDialogueName));
            tabButtons[btnDialogueName].clicked += Show_Dialogue;
        }

        private void Show_Dialogue()
        {
            throw new NotImplementedException();
        }

        private void Show_Components()
        {
            throw new NotImplementedException();
        }

        protected override void GetInfoContainers()
        {
            infoContainers.Add(infoCharacterName, container.Q<VisualElement>(infoCharacterName));
            infoContainers.Add(infoClassName, container.Q<VisualElement>(infoClassName));
            infoContainers.Add(infoComponentsName, container.Q<VisualElement>(infoComponentsName));
            infoContainers.Add(infoDialoguesName, container.Q<VisualElement>(infoDialoguesName));
        }

        private void Show_Class()
        {

        }

        private void Show_Character()
        {

        }

        private void CreateTagsContainer()
        {
            VisualTreeAsset tagsContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/TagsContainer.uxml");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/LineTags.uss");

            container.styleSheets.Add(styleSheet);
            var infoContainer = container.Q<VisualElement>(infoCharacterName);

            TwoPaneSplitView splitView = CreateSplitView(tagsContainer);
            infoContainer.Add(splitView);

            GetSearchBars();

            CreateTags(leftPanel, rightPanel);
            Add_SearchElements(infoLeft, 16);

            void GetSearchBars()
            {
                txtSearch_Left = leftPanel.Q<TextField>("txtSearch");
                txtSearch_Left.RegisterCallback<KeyDownEvent>(KeyDown_SearchElementCharacter);

                txtSearch_Right = rightPanel.Q<TextField>("txtSearch");
                txtSearch_Right.RegisterCallback<KeyDownEvent>(SearchFilterCharacter);
            }
        }

        TwoPaneSplitView CreateSplitView(VisualTreeAsset tagsContainer)
        {
            TwoPaneSplitView splitView = new TwoPaneSplitView();
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.fixedPaneInitialDimension = 150;

            leftPanel = tagsContainer.Instantiate();
            rightPanel = tagsContainer.Instantiate();
            splitView.Insert(0, leftPanel);
            splitView.Insert(1, rightPanel);

            infoLeft = leftPanel.Q<ScrollView>("elementsContainer");
            infoRight = rightPanel.Q<ScrollView>("elementsContainer");

            return splitView;
        }

        private void CreateTags(VisualElement leftContainer, VisualElement rightContainer)
        {
            AddTags(leftContainer, btnsLeft_Tag);
            AddTags(rightContainer, btnsRight_Tag);

            void AddTags(VisualElement container, List<Button> buttons)
            {
                var filters = container.Q<VisualElement>("TagsContainer").Query<Button>(className: "FilterTag");
                buttons = filters.ToList();
                buttons.ForEach(b => b.clicked += OnClicked_ElementCharacter);
            }
        }

        private void Add_SearchElements(ScrollView container, int amount)
        {
            VisualTreeAsset ElementTag = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/ElementTag.uxml");
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
                    PinElement(idx);
                };
            }
        }

        private void DisplayElementPanel(int idx)
        {
            throw new NotImplementedException();
        }

        private void LoadTags()
        {
            charactersLists = (AssetDatabase.FindAssets(typeof(CharacterTag).ToString(), new[] { "Assets/Proyect/Game/ScriptableObjects/Tool/CharacterTag.asset" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterTag>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList()).FirstOrDefault();

            if (charactersLists == null)
            {
                Notify("No labels found.", BorderColour.Warning);

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

        private bool EnableCharacterElements(out List<int> enabledButtonsIdx, params string[] names)
        {
            bool enabled = false;
            enabledButtonsIdx = null;

            for (int i = 0, j = 0; i < Left_Elements.Count; i++)
            {
                if (Left_Elements[i].pinned)
                {
                    if (j >= names.Length) continue;

                    if (Left_Elements[i].element.Q<Button>("txtElement").text.Contains(names[j]))
                    {
                        enabled = true;
                        ++j;
                    }

                    continue;
                }

                if (j < names.Length)
                {
                    Left_Elements[i].element.Q<Button>("txtElement").text = names[j];
                    EnableContainer(Left_Elements[i].element, true);

                    (enabledButtonsIdx ??= new()).Add(i);
                    enabled = true;
                    ++j;
                }
                else
                {
                    Left_Elements[i].element.Q<Button>("txtElement").text = "";
                    EnableContainer(Left_Elements[i].element, false);
                }
            }

            return enabled;
        }

        private void EnableContainer(VisualElement container, bool shouldEnable)
        {
            if (shouldEnable)
            {
                if (container.ClassListContains("Disable"))
                {
                    container.RemoveFromClassList("Disable");
                }
            }
            else if (!container.ClassListContains("Disable"))
            {
                container.AddToClassList("Disable");
            }
        }

        private void SearchElementCharacter(string text, ElementType searchType)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (searchType == ElementType.None)
            {
                int count = Enum.GetValues(typeof(ElementType)).Length;

                for (int i = 0; i < count; i++)
                {
                    if (!charactersLists.elements.ContainsKey((ElementType)i))
                        continue;

                    FindType(text, (ElementType)i);
                }
            }
            else
            {
                FindType(text, searchType);
            }

            void UpdateElementsData(List<int> idxs)
            {
                if (idxs == null) return;

                for (int i = 0; i < idxs.Count; i++)
                {
                    ElementData newElementData = new()
                    {
                        element = Left_Elements[idxs[i]].element,
                        type = searchType,
                        valueIdx = idxs[i],
                    };

                    Left_Elements[idxs[i]] = newElementData;
                }
            }

            void FindType(string text, ElementType type)
            {
                foreach (var element in charactersLists.elements[type])
                {
                    if (element.ToLower().Contains(text.ToLower()))
                    {
                        EnableCharacterElements(out List<int> idxs, element);
                        UpdateElementsData(idxs);

                        lastLeftSearch = (searchType, text);
                    }
                }
            }
        }

        private void Displace_Pins(int elementIdx)
        {
            for (int i = elementIdx + 1; i < Left_Elements.Count; i++)
            {
                if (Left_Elements[i].pinned)
                {
                    SetPin(i, i - 1);

                    var lastText = Left_Elements[i].element.Q<Button>("txtElement").text;
                    Left_Elements[i].element.Q<Button>("txtElement").text = Left_Elements[i - 1].element.Q<Button>("txtElement").text;
                    Left_Elements[i -1].element.Q<Button>("txtElement").text = lastText;
                }
                else
                    break;
            }
        }

        #region Events
        private void KeyDown_SearchElementCharacter(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return) return;

            string text = txtSearch_Left.value.Trim();

            SearchElementCharacter(text, leftSearchType);
        }

        private void OnClicked_ElementCharacter()
        {

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
                    SetPin(elementIdx, i);
                    break;
                }
            }
        }

        private void SetPin(int elementIdx, int target)
        {
            var lastData = Left_Elements[target];
            lastData.element = Left_Elements[elementIdx].element;
            lastData.pinned = false;
            var newData = Left_Elements[elementIdx];
            newData.element = Left_Elements[target].element;
            newData.pinned = true;

            Left_Elements[elementIdx] = lastData;
            Left_Elements[target] = newData;

            Highlight(Left_Elements[elementIdx].element.Q<Button>("txtElement"), false);
            Highlight(Left_Elements[target].element.Q<Button>("txtElement"), true);
        }

        private void RemovePin(int elementIdx)
        {
            Displace_Pins(elementIdx);

            SearchElementCharacter(lastLeftSearch.text, lastLeftSearch.type);
            return;
        }

        private void RemoveElement(VisualElement element)
        {
            string text = element.Q<Button>("txtElement").text;

            //foreach (string value in charactersLists.elements.Values)
            //{
            //    if (value == text)
            //    {

            //    }
            //}
        }
        #endregion
    }
}
