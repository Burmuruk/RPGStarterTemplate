﻿using Burmuruk.Tesis.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.TabCharacterEditor;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class CharacterSettings : BaseInfoTracker, ISaveable, ISubWindowsContainer
    {
        #region Variables
        VisualElement _parent;
        const string STATS_CONTAINER_NAME = "infoStats";
        Dictionary<CharacterTab, SubWindow> subTabs = new();
        CharacterData? _characterData;
        private string _id;
        private Dictionary<int, StatDataUI> _stats = new();
        private Dictionary<int, StatChange> _statsChanges = new();
        private Dictionary<int, PropertyField> _statsTypes = new();
        private int? _selectedStat = null;
        private List<ModEntry> allMods;
        private Dictionary<string, StatNameData> _statNames;
        private Dictionary<string, Type> _selectableClasses;
        private Progression _progression;
        //private BasicStats basicStats;

        CharacterTab _lastTab;
        CharacterTab curTab = CharacterTab.None;
        VisualElement statsContainer;
        StatsVisualizer basicStats = null;

        string[] _defaultVariables = new string[]
        {
            "speed",
            "damage",
            "damageRate",
            "color",
            "eyesRadious",
            "earsRadious",
            "minDistance",
        };
        string[] _defaultHeaders = new string[]
        {
            "Basic stats",
            "Detection",
        };
        private SerializedObject _serializedStats;

        enum CharacterTab
        {
            None,
            Inventory,
            Equipment,
            Health
        }

        private struct ModChanges
        {
            public List<string> remove;
            public List<ModChange> edit;
            public List<ModEntry> add;

            public void Initialize()
            {
                remove = new List<string>();
                edit = new List<ModChange>();
                add = new List<ModEntry>();
            }
        }

        private struct StatChanges
        {
            public List<string> remove;
            public List<ModChange> edit;
            public List<BasicStatsEditor.VariableEntry> add;

            public bool HasChanges()
            {
                return remove.Count > 0 || edit.Count > 0 || add.Count > 0;
            }

            public void Initialize()
            {
                remove = new List<string>();
                edit = new List<ModChange>();
                add = new List<BasicStatsEditor.VariableEntry>();
            }
        }

        class StatDataUI
        {
            public VisualElement extraSpace;
            public Toggle toggle;
            public VisualElement editButtons;
            StatData data = new();

            public string Name { get => data.name; set => data.name = value; }
            public ModifiableStat Type { get => data.type; set => data.type = value; }
            public bool Enabled { get => data.enabled; set => data.enabled = value; }
        }

        class StatChange
        {
            public ModifiableStat? type;
            public string name;
            public bool removed;

            public bool HasChanges()
            {
                return type.HasValue || name != null || removed;
            }

            public StatChange(ModifiableStat? type, string name)
            {
                this.type = type;
                this.name = name;
            }
        }
        #endregion

        #region Properties
        public Toggle TglSave { get; private set; }
        public ComponentsListUI<ElementComponent> ComponentsList { get; private set; }
        public EnumModifierUI<CharacterType> EMCharacterType { get; private set; }
        public PopupField<string> PUBaseClass { get; private set; }
        public VariablesAdderUI Adder { get; private set; }
        public EquipmentSettings EquipmentS { get => (EquipmentSettings)subTabs[CharacterTab.Equipment]; }
        public InventorySettings InventoryS { get => (InventorySettings)subTabs[CharacterTab.Inventory]; }
        private EnumModifierUI<ModifiableStat> EMStatType { get; set; }
        private Button BtnApplyStats { get; set; }
        public VisualElement StatEditorContainer { get; private set; }
        #endregion

        public void Initialize(VisualElement container, CreationsBaseInfo name, VisualElement parent)
        {
            _parent = parent;
            Initialize(container, name);
        }

        #region Initialization
        public override void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            base.Initialize(container, name);
            _instance = container;

            TglSave = container.Q<Toggle>("TglSave");
            VisualElement pBaseClass = container.Q<VisualElement>("PBaseClass");
            BtnApplyStats = container.Q<Button>("btnApplyStats");
            BtnApplyStats.clicked += OnClick_ApplyStats;
            Setup_PUBaseClass(pBaseClass);

            ComponentsList = new ComponentsListUI<ElementComponent>(container);
            Create_StatModifier();
            Create_StatEditor();

            Setup_ComponentsList();
            Setup_EMCharacterType();
            CreateSubTabs();
            Setup_Stats(container);
            _progression = CreateInstance<Progression>();
            _progression.Initialize(container,
                () => basicStats.stats,
                bs =>
                {
                    basicStats.stats = bs;
                    var so = new SerializedObject(basicStats);
                    so.Update();
                });
        }

        private void Create_StatEditor()
        {
            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/Elements/StatEditor.uxml");
            StatEditorContainer = element.Instantiate();
            TextField textField = StatEditorContainer.Q<TextField>("txtStatName");
            textField.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    Rename_StatName(textField.text, textField);
                }
            });
        }

        private void Rename_StatName(string name, TextField textField)
        {
            if (_selectedStat == null) return;

            DisableNotification();
            Highlight(textField, false);
            string lower = name.ToLowerInvariant();

            if (string.IsNullOrEmpty(name) || name.Length < 3)
            {
                Delete_NewStatName("The name must be at least 3 characters long", textField);
                Highlight_StatChange(false);
                return;
            }

            if (lower != _stats[_selectedStat.Value].Name.ToLower())
            {
                if (VerifyVariableName(name))
                {
                    _statsChanges[_selectedStat.Value].name = name;

                    Adder.RequestEnable_ApplyButton(true);
                    Disable_StatEditor(_selectedStat.Value);
                    Highlight_StatChange(true);
                }
                else
                {
                    Notify("Invalid name", BorderColour.Error);
                    Highlight(textField, true, BorderColour.Error);
                    return;
                }
            }
            else
            {
                Delete_NewStatName("The name must be different", textField);
                Highlight(textField, false);
                Highlight_StatChange(false);
            }
        }

        private void Highlight_StatChange(bool highlight)
        {
            var button = _stats[_selectedStat.Value].editButtons.Q<Button>("btnEditStat");
            Highlight(button, highlight);
        }

        private void Delete_NewStatName(string message, TextField textField)
        {
            Notify(message, BorderColour.Error);
            Highlight(textField, true, BorderColour.Error);
            _statsChanges[_selectedStat.Value].name = null;
            Verify_ModChanges();
        }

        private void Disable_StatEditor(int idx)
        {
            if (_stats[idx].extraSpace.Contains(StatEditorContainer))
            {
                _stats[idx].extraSpace.Remove(StatEditorContainer);
            }

            if (_stats[idx].extraSpace.Contains(EMStatType.Container))
                return;

            EnableContainer(_stats[idx].extraSpace, false);
        }

        private void OnClick_ApplyStats()
        {
            var classes = GetClasses();
            var newStats = Adder.GetInfo();
            string updatedText = string.Empty;
            string path = null;

            Get_Changes(newStats, out ModChanges mods, out StatChanges stats);
            DisableNotification();

            foreach (var name in classes)
            {
                path = GetCharacterScriptPath(name);

                if (string.IsNullOrEmpty(path))
                {
                    Notify($"{PUBaseClass.value} class was not found", BorderColour.Error);
                    return;
                }

                var text = File.ReadAllText(path);

                updatedText = ModSetupEditor.RemoveMods(text, mods.remove);
                updatedText = ModSetupEditor.RenameModChanges(updatedText, mods.edit);
                updatedText = ModSetupEditor.AddMods(updatedText, mods.add);
                File.WriteAllText(path, updatedText);
                AssetDatabase.SaveAssets();
            }


            Change_StatsNames(stats);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Get_Changes(List<ModChange> newStats, out ModChanges mods, out StatChanges stats)
        {
            mods = new ModChanges();
            mods.Initialize();
            stats = new StatChanges();
            stats.Initialize();

            foreach (var stat in newStats)
            {
                if (stat.NewName != null)
                {
                    stats.add.Add(new BasicStatsEditor.VariableEntry
                    {
                        Name = stat.NewName,
                        Type = stat.VariableType.ToString(),
                        Header = stat.Header,
                    });

                    if (stat.Type != ModifiableStat.None && stat.VariableType == VariableType.@float)
                    {
                        mods.add.Add(new ModEntry
                        {
                            VariableName = stat.NewName,
                            ModifiableStat = stat.Type.ToString(),
                        });
                    }
                }
            }

            foreach (var item in _statsChanges)
            {
                if (!item.Value.HasChanges()) continue;

                StatChange change = item.Value;

                if (change.type.HasValue)
                {
                    if (_stats[item.Key].Type == ModifiableStat.None)
                    {
                        mods.add.Add(new ModEntry
                        {
                            VariableName = _stats[item.Key].Name,
                            ModifiableStat = change.type.Value.ToString(),
                        });
                    }
                    else if (change.type == ModifiableStat.None)
                    {
                        mods.remove.Add(_stats[item.Key].Name);
                    }
                    else
                    {
                        mods.edit.Add(new ModChange
                        {
                            OldName = _stats[item.Key].Name,
                            NewName = change.name == null ? null : change.name,
                            Type = change.type.Value,
                        });
                    }
                }

                if (change.name != null)
                {
                    mods.edit.Add(new ModChange
                    {
                        OldName = _stats[item.Key].Name,
                        NewName = change.name,
                        Type = _stats[item.Key].Type,
                    });
                    stats.edit.Add(new ModChange
                    {
                        OldName = _stats[item.Key].Name,
                        NewName = change.name,
                        Type = _stats[item.Key].Type,
                    });
                }

                if (change.removed)
                {
                    stats.remove.Add(_stats[item.Key].Name);
                }
            }
        }

        private void Change_StatsNames(StatChanges changes)
        {
            if (!changes.HasChanges()) return;

            string className = typeof(BasicStats).Name;
            string[] guids = AssetDatabase.FindAssets($"t:Script {className}");
            string path = null;

            foreach (string guid in guids)
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                string content = File.ReadAllText(path);

                if (Regex.IsMatch(content, $@"(?m)public\s+struct\s+\b{className}\b"))
                    break;

                path = null;
            }

            if (path == null) return;

            var text = File.ReadAllText(path);

            var updatedText = BasicStatsEditor.RemoveVariables(text, changes.remove);
            updatedText = BasicStatsEditor.AddVariables(updatedText, changes.add);

            foreach (var editedStat in changes.edit)
            {
                updatedText = BasicStatsEditor.RenameVariable(updatedText, editedStat.OldName, editedStat.NewName);
            }

            File.WriteAllText(path, updatedText);
        }

        private void Setup_PUBaseClass(VisualElement pBaseClass)
        {
            var derivedTypes = TypeCache.GetTypesDerivedFrom<Tesis.Control.Character>();
            var baseClasses = derivedTypes.Select(t => t.FullName).ToList();
            var shortNames = baseClasses.Select(name => name.Split(".").Last()).ToList();

            PUBaseClass = new PopupField<string>("Base class", shortNames, 0);
            PUBaseClass.style.flexBasis = new Length(98, LengthUnit.Percent);
            PUBaseClass.style.flexShrink = 1;
            PUBaseClass.RegisterValueChangedCallback(OnValueChanged_BaseClass);

            foreach (var name in shortNames)
            {
                if (name.Contains("GuildMember"))
                {
                    PUBaseClass.SetValueWithoutNotify(name);
                    break;
                }
            }

            _selectableClasses = new Dictionary<string, Type>();
            int i = 0;
            foreach (var name in shortNames)
            {
                _selectableClasses[name] = derivedTypes[i++];
            }
            pBaseClass.Add(PUBaseClass);
        }

        private void OnValueChanged_BaseClass(ChangeEvent<string> evt)
        {
            Get_StatsModifications();

            foreach (var stat in _stats.Values)
            {
                stat.Type = IsStatModified(stat.Name);
            }

            if (_selectedStat.HasValue)
            {
                Disable_Stat(_selectedStat.Value);
            }
        }

        private void Create_StatModifier()
        {
            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RPGStarterTemplate/Tool/UIToolkit/GeneralElements/TypeAdder.uxml");
            EMStatType = new EnumModifierUI<ModifiableStat>(element.Instantiate());
            EMStatType.EnumField.RegisterValueChangedCallback(OnSelected_StatType);
            EMStatType.Name.text = "Modification name";
        }

        private void OnSelected_StatType(ChangeEvent<Enum> evt)
        {
            if (!_selectedStat.HasValue) return;

            int idx = _selectedStat.Value;
            var newType = (ModifiableStat)evt.newValue;

            if (newType != _stats[idx].Type)
            {
                _statsChanges[idx].type = newType;
                Adder.RequestEnable_ApplyButton(true);
            }
            else
            {
                _statsChanges[idx].type = null;
                Verify_ModChanges();

                if (newType == ModifiableStat.None)
                {
                    _stats[idx].toggle.SetValueWithoutNotify(false);
                }
            }
        }

        private void Setup_Stats(VisualElement container)
        {
            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer = container.Q<VisualElement>(STATS_CONTAINER_NAME);
            statsContainer.Clear();
            basicStats = instance;
            Get_StatsModifications();
            AddStats();

            statsContainer.schedule.Execute(() =>
            {
                VisualElement adderUI = container.Q<VisualElement>("VariblesAdder");
                Adder = new(adderUI, Get_Headers(), _statNames);
                Adder.OnChange += value => EnableContainer(BtnApplyStats, value);
            }).ExecuteLater(200);
        }

        private List<string> Get_Headers()
        {
            var properties = statsContainer.Query<PropertyField>().ToList();
            List<string> labels = new();

            foreach (var property in properties)
            {
                var newLabels = property.Query<Label>().ToList();

                if (newLabels.Count > 1)
                {
                    labels.Add(newLabels[0].text);

                    if (_statNames.ContainsKey(newLabels[1].text))
                    {
                        _statNames[newLabels[1].text].header = newLabels[1].text;
                    }
                }
            }

            return labels;
        }

        private void Setup_ComponentsList()
        {
            ComponentsList.DDFElement.RegisterValueChangedCallback(ComponentsList.AddComponent);
            ComponentsList.OnElementClicked += OpenComponentSettings;
            ComponentsList.CreationValidator += ContainsCreation;
            ComponentsList.AddElementExtraData += Setup_ComponentButton;
            ComponentsList.OnElementRemoved += Clear_ComponentData;

            ComponentsList.DDFElement.choices.Clear();

            foreach (var name in Enum.GetNames(typeof(ComponentType)))
            {
                ComponentsList.DDFElement.choices.Add(name);
            }

            ComponentsList.DDFElement.value = "None";
        }

        private void Setup_EMCharacterType()
        {
            EMCharacterType = new EnumModifierUI<CharacterType>(_instance.Q<VisualElement>(EnumModifierUI<CharacterType>.ContainerName));
            EMCharacterType.Name.text = "Character Type";
            EMCharacterType.EnumField.Init(CharacterType.None);
        }

        private void CreateSubTabs()
        {
            subTabs.Add(CharacterTab.None, this);

            subTabs.Add(CharacterTab.Inventory, CreateInstance<InventorySettings>());
            var inventory = (InventorySettings)subTabs[CharacterTab.Inventory];
            inventory.Initialize(_parent);
            inventory.GoBack += () => ChangeWindow(CharacterTab.None);
            inventory.OnElementClicked += ChangeTab;
            EnableContainer(inventory.Instance, false);

            subTabs.Add(CharacterTab.Equipment, CreateInstance<EquipmentSettings>());
            var equipment = subTabs[CharacterTab.Equipment];
            equipment.Initialize(_parent);
            equipment.GoBack += () => ChangeWindow(CharacterTab.None);
            EnableContainer(equipment.Instance, false);

            subTabs.Add(CharacterTab.Health, CreateInstance<HealthSettings>());
            subTabs[CharacterTab.Health].Initialize(_parent);
            subTabs[CharacterTab.Health].GoBack += () => ChangeWindow(CharacterTab.None);
            EnableContainer(subTabs[CharacterTab.Health].Instance, false);
        }
        #endregion

        #region Stats visualization
        private void AddStats()
        {
            _statNames = new Dictionary<string, StatNameData>();
            _stats.Clear();
            var members = typeof(BasicStats).GetFields();
            _serializedStats = new SerializedObject(basicStats);

            foreach (var member in members)
            {
                var prop = _serializedStats.FindProperty("stats");

                _statNames.Add(member.Name, new StatNameData
                {
                    variableType = member.GetType().ToString(),
                });
                Debug.Log($"name: {member.Name} => {member.FieldType}");
                bool modifiable = member.FieldType == typeof(float) || member.FieldType == typeof(int);
                AddStatUI(statsContainer, prop.FindPropertyRelative(member.Name), member.Name, modifiable);
            }

            _serializedStats.ApplyModifiedProperties();
        }

        private void Get_StatsModifications()
        {
            var classes = GetClasses();
            allMods = new();

            foreach (var name in classes)
            {
                string path = GetCharacterScriptPath(name);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError($"{name} class was not found");
                    return;
                }

                var text = File.ReadAllText(path);
                allMods.AddRange(ModSetupEditor.ExtractAllMods(text));
            }

            return;
        }

        private List<string> GetClasses()
        {
            var classes = new List<string>()
            {
                "Character",
            };

            if (PUBaseClass.value != "Character")
            {
                Type curent = _selectableClasses[PUBaseClass.value];
                List<string> baseClasses = new();

                while (curent != null && curent.Name != "Character")
                {
                    baseClasses.Add(PUBaseClass.value);
                    curent = curent.BaseType;
                }

                classes.AddRange(baseClasses);
            }

            return classes;
        }

        public Type GetTypeByName(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    Type[] types = null;
                    try { types = assembly.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                    return types;
                })
                .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
        }

        string GetCharacterScriptPath(string className)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Script {className}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string content = File.ReadAllText(path);

                if (content.Contains($"public class {className}"))
                {
                    return path;
                }
            }

            return null;
        }

        private void OnEnable_StatToggle(ChangeEvent<bool> evt, int idx)
        {
            Verify_LastToggleValue();

            if (!evt.newValue)
            {
                Disable_Stat(idx);
                _selectedStat = null;

                if (_stats[idx].Type == ModifiableStat.None)
                {
                    _statsChanges[idx].type = null;
                    Verify_ModChanges();
                }
                else
                {
                    _statsChanges[idx].type = ModifiableStat.None;
                    Adder.RequestEnable_ApplyButton(true);
                }
                return;
            }

            if (_statsChanges[idx].type.HasValue)
                EMStatType.EnumField.SetValueWithoutNotify(_statsChanges[idx].type.Value);
            else
                EMStatType.EnumField.SetValueWithoutNotify(_stats[idx].Type);
            _selectedStat = idx;

            _stats[idx].extraSpace.Add(EMStatType.Container);
            EnableContainer(_stats[idx].extraSpace, true);
        }

        private void Verify_LastToggleValue()
        {
            if (!_selectedStat.HasValue) return;

            var idx = _selectedStat.Value;
            Disable_Stat(idx);

            if (_statsChanges[idx].type.HasValue && _statsChanges[idx].type != _stats[idx].Type)
                return;

            _stats[idx].toggle.SetValueWithoutNotify(false);
        }

        private void Verify_ModChanges()
        {
            foreach (var chage in _statsChanges.Values)
            {
                if (chage.HasChanges())
                {
                    Adder.RequestEnable_ApplyButton(true);
                    return;
                }
            }

            Adder.RequestEnable_ApplyButton(false);
        }

        private void Disable_Stat(int idx)
        {
            if (_stats[idx].extraSpace.Contains(EMStatType.Container))
            {
                _stats[idx].extraSpace.Remove(EMStatType.Container);
            }

            if (_stats[idx].extraSpace.Contains(StatEditorContainer))
                return;

            EnableContainer(_stats[idx].extraSpace, false);
        }

        void AddStatUI(VisualElement parent, SerializedProperty property, string name, bool modifiable)
        {
            var row = GetRow();
            var field = new PropertyField(property, name);
            field.Bind(property.serializedObject);
            field.style.flexGrow = 1;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.alignItems = Align.FlexStart;
            container.style.flexGrow = 0;

            var extraRow = GetRow();
            extraRow.style.flexGrow = 1;
            extraRow.style.marginBottom = 6;

            int idx = _stats.Count;
            var toggleColumn = Create_Toggle(idx, out Toggle toggle);

            toggle.SetEnabled(modifiable);

            row.Add(field);
            row.Add(toggleColumn);
            container.Add(row);
            container.Add(extraRow);
            parent.Add(container);

            EnableContainer(extraRow, false);
            _stats.Add(idx, new StatDataUI()
            {
                toggle = toggle,
                extraSpace = extraRow,
                Name = name,
                Type = IsStatModified(name),
            });
            _statsTypes[idx] = field;

            _statsChanges ??= new();
            _statsChanges[idx] = new StatChange(null, null);

            toggle.SetValueWithoutNotify(_stats[idx].Type != ModifiableStat.None);
            _statNames[name].type = _stats[idx].Type;

            if (_defaultVariables.Contains(name))
            {
                _statNames[name].editable = false;
                return;
            }

            _statNames[name].editable = true;

            //Add buttons to edit and remove the stat
            Add_EditButtons(row, idx);
        }

        private void Add_EditButtons(VisualElement row, int idx)
        {
            VisualTreeAsset editButtons = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RPGStarterTemplate/Tool/UIToolkit/CharacterEditor/Elements/BtnsEditStat.uxml");
            VisualElement buttonsContainer = editButtons.Instantiate();
            buttonsContainer.style.flexGrow = 1;
            buttonsContainer.style.maxWidth = new Length(45, LengthUnit.Pixel);
            buttonsContainer.style.flexDirection = FlexDirection.Column;

            row.Add(buttonsContainer);
            _stats[idx].editButtons = row.Q<VisualElement>("EditButtons");
            Button edit = _stats[idx].editButtons.Q<Button>("btnEditStat");
            Button remove = _stats[idx].editButtons.Q<Button>("btnRemoveStat");

            edit.clicked += () => OnClick_EditStat(idx);
            remove.clicked += () => OnClick_RemoveStat(idx);
        }

        private void OnClick_RemoveStat(int idx)
        {
            bool removed = !_statsChanges[idx].removed;
            _statsChanges[idx].removed = removed;

            Highlight(_stats[idx].editButtons.Q<Button>("btnRemoveStat"), removed);

            if (removed)
            {
                _selectedStat = idx;
                Highlight_StatChange(false);
                _statsChanges[_selectedStat.Value].name = null;
                Adder.RequestEnable_ApplyButton(true);
            }
            else
            {
                Verify_ModChanges();
            }

            Disable_StatEditor(idx);
            _selectedStat = null;
        }

        private void OnClick_EditStat(int idx)
        {
            if (!_stats[idx].extraSpace.Contains(StatEditorContainer))
            {
                _stats[idx].extraSpace.Add(StatEditorContainer);
                EnableContainer(_stats[idx].extraSpace, true);
                _selectedStat = idx;
                string name = _statsChanges[idx].name == null ? _stats[idx].Name : _statsChanges[idx].name;
                StatEditorContainer.Q<TextField>("txtStatName").SetValueWithoutNotify(name);
            }
            else
            {
                Disable_StatEditor(idx);
                _selectedStat = null;
            }

            if (_statsChanges[idx].removed)
            {
                _statsChanges[idx].removed = false;
                Highlight(_stats[idx].editButtons.Q<Button>("btnRemoveStat"), false);
            }
        }

        private ModifiableStat IsStatModified(string name)
        {
            var nameLower = name.ToLower();

            foreach (var mod in allMods)
            {
                if (mod.VariableName.ToLower() == nameLower)
                {
                    if (Enum.TryParse(mod.ModifiableStat, out ModifiableStat result))
                        return result;
                }
            }

            return ModifiableStat.None;
        }

        private VisualElement Create_Toggle(int idx, out Toggle toggle)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Column;
            row.style.flexGrow = 1;
            row.style.maxWidth = 20;
            var column = new VisualElement();
            column.style.flexDirection = FlexDirection.ColumnReverse;
            column.style.flexGrow = 1;

            toggle = new Toggle()
            {
                text = "",
                tooltip = $"Acción personalizada para {name}"
            };

            toggle.RegisterValueChangedCallback(evt => OnEnable_StatToggle(evt, idx));

            toggle.style.width = 24;
            toggle.style.marginLeft = 4;

            row.Add(column);
            column.Add(toggle);
            return row;
        }

        private VisualElement GetRow()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexGrow = 1,
                    minWidth = new Length(98, LengthUnit.Percent),
                    flexShrink = 0,
                    flexWrap = Wrap.Wrap
                }
            };

            return row;
        }
        #endregion

        #region Public methods
        public override void Clear()
        {
            if (curTab != CharacterTab.None)
            {
                subTabs[curTab].Clear();
                return;
            }

            TxtName.SetValueWithoutNotify("");
            ComponentsList.Clear();
            subTabs[CharacterTab.Inventory].Clear();
            subTabs[CharacterTab.Equipment].Clear();
            subTabs[CharacterTab.Health].Clear();
            EMCharacterType.Value = CharacterType.None;

            _progression.Clear();
            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;
            _characterData = null;
            TempName = "";
            _id = null;
            _lastTab = CharacterTab.None;
        }

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_characterData == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;

                //Name
                if ((_nameControl.Check_Changes() & ModificationTypes.None) == 0)
                {
                    CurModificationType = ModificationTypes.Rename;
                }

                //Components's data
                if ((subTabs[CharacterTab.Health].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                if ((subTabs[CharacterTab.Inventory].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                if ((subTabs[CharacterTab.Equipment].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                //Progresssion
                if ((_progression.Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                //Stats
                if (_characterData.Value.stats.speed != basicStats.stats.speed &&
                    _characterData.Value.stats.damageRate != basicStats.stats.damage &&
                    _characterData.Value.stats.damageRate != basicStats.stats.damageRate &&
                    _characterData.Value.stats.eyesRadious != basicStats.stats.eyesRadious &&
                    _characterData.Value.stats.minDistance != basicStats.stats.minDistance
                    )
                {
                    CurModificationType = ModificationTypes.EditData;
                }

                if (_characterData.Value.characterType != EMCharacterType.Value)
                    CurModificationType = ModificationTypes.EditData;

                //Components
                if (_characterData.Value.components.Count == ComponentsList.Components.Count)
                {
                    foreach (var component in ComponentsList.Components)
                    {
                        if (!_characterData.Value.components.ContainsKey((ComponentType)component.Type))
                        {
                            CurModificationType = ModificationTypes.EditData;
                            break;
                        }
                    }
                }
                else
                    CurModificationType = ModificationTypes.EditData;

                //saving
                if (_characterData.Value.shouldSave != TglSave.value)
                    CurModificationType = ModificationTypes.EditData;

                return CurModificationType;
            }
            catch (InvalidExeption e)
            {
                throw e;
            }
        }

        public bool Save()
        {
            try
            {
                if ((Check_Changes() & ModificationTypes.None) != 0)
                    return false;

                var newData = GetInfo();
                return SavingSystem.SaveCreation(ElementType.Character, _id, new CreationData(newData.characterName, newData), CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public CreationData Load(ElementType type, string id)
        {
            CreationData? data = SavingSystem.Load(ElementType.Character, id);

            if (!data.HasValue)
            {
                return default;
            }

            Set_CreationState(CreationsState.Editing);
            LoadInfo((CharacterData)data.Value.data, id);

            return data.Value;
        }

        public override void Remove_Changes()
        {
            _nameControl.Remove_Changes();

            if (curTab != CharacterTab.None)
            {
                subTabs[curTab].Remove_Changes();
                return;
            }

            _lastTab = CharacterTab.None;
            TempName = _nameControl.name;
            CharacterData newInfo = _characterData.Value;
            LoadInfo(newInfo, _id);
        }

        public void CloseWindows()
        {
            for (int i = 1; i < Enum.GetValues(typeof(CharacterTab)).Length; i++)
            {
                EnableContainer(subTabs[(CharacterTab)i].Instance, false);
            }

            curTab = CharacterTab.None;
        }

        public override void Enable(bool enabled)
        {
            base.Enable(enabled);

            if (enabled)
                ChangeWindow(_lastTab);
            else
                CloseWindows();
        }
        #endregion

        #region Events
        private void OpenComponentSettings(int componentIdx)
        {
            var type = (ComponentType)ComponentsList[componentIdx].Type;

            CharacterTab newTab = Get_TabType(type);

            if (newTab == CharacterTab.None) return;

            switch (type)
            {
                case ComponentType.Equipment:
                    Load_InventoryItemsInEquipment();
                    break;
                case ComponentType.Inventory:
                    break;

                default: break;
            }

            ChangeWindow(newTab);
        }

        private int? ContainsCreation(IList list, string name)
        {
            var components = (List<ElementComponent>)list;
            int i = 0;
            int? emptyIdx = -1;

            foreach (var component in components)
            {
                if (!component.element.ClassListContains("Disable"))
                {
                    if (component.NameButton.text == name)
                    {
                        return null;
                    }
                }
                else if (!emptyIdx.HasValue)
                {
                    emptyIdx = i;
                }

                ++i;
            }

            return emptyIdx;
        }

        /// <summary>
        /// Setup element by setting id, changing colour or enable its button.
        /// </summary>
        /// <param name="element"></param>
        private void Setup_ComponentButton(ElementComponent element)
        {
            switch ((ComponentType)element.Type)
            {
                case ComponentType.Equipment:
                    AddEquipment(element);
                    goto case ComponentType.Health;

                case ComponentType.Inventory:
                case ComponentType.Health:
                    SetClickableButtonColour(element);
                    break;

                default:
                    if (element.NameButton.ClassListContains("ClickableBtn"))
                        element.NameButton.RemoveFromClassList("ClickableBtn");
                    element.NameButton.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
                    break;
            }
        }

        private void Clear_ComponentData(ElementComponent element)
        {
            var tabType = Get_TabType((ComponentType)element.Type);

            if (!subTabs.ContainsKey(tabType)) return;

            subTabs[tabType].Clear();
        }
        #endregion

        /// <summary>
        /// Adds an inventory if the new element is an equipment and the inventory it's not in the list.
        /// </summary>
        /// <param name="element">Equipment component</param>
        private void AddEquipment(ElementComponent element)
        {
            if ((ComponentType)element.Type != ComponentType.Equipment) return;

            var comps = (from c in ComponentsList.Components
                         where (ComponentType)c.Type == ComponentType.Inventory && !c.element.ClassListContains("Disable")
                         select c).ToArray();

            if (comps == null || comps.Length == 0)
            {
                ComponentsList.AddElement(ComponentType.Inventory.ToString());
            }
        }

        private void SetClickableButtonColour(ElementComponent element)
        {
            element.NameButton.AddToClassList("ClickableBtn");
            element.NameButton.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }

        #region Tab control
        private void ChangeTab(ComponentType type)
        {
            if (type == ComponentType.Health)
                ChangeWindow(CharacterTab.Health);
        }

        private void ChangeWindow(CharacterTab newTab)
        {
            if (curTab == newTab) return;

            EnableContainer(subTabs[curTab].Instance, false);
            EnableContainer(subTabs[newTab].Instance, true);
            _lastTab = newTab;
            curTab = newTab;
        }
        #endregion

        private CharacterTab Get_TabType(ComponentType type) =>
            type switch
            {
                ComponentType.Equipment => CharacterTab.Equipment,
                ComponentType.Health => CharacterTab.Health,
                ComponentType.Inventory => CharacterTab.Inventory,
                _ => CharacterTab.None
            };

        private void Load_InventoryItemsInEquipment()
        {
            var items = ((InventorySettings)subTabs[CharacterTab.Inventory]).MClInventoryElements;
            //var inventory = (subTabs[CharacterTab.Inventory] as InventorySettings).GetInventory();
            ((EquipmentSettings)subTabs[CharacterTab.Equipment]).Load_EquipmentFromList(items);
        }

        private CharacterData GetInfo()
        {
            CharacterData newData = new();
            newData.components ??= new();
            AddCharacterComponents(ref newData);
            newData.characterType = (CharacterType)EMCharacterType.EnumField.value;
            newData.stats = basicStats.stats;
            newData.characterName = TxtName.value;
            newData.shouldSave = TglSave.value;
            newData.progress = _progression.Get_Info();

            return newData;
        }

        private void LoadInfo(in CharacterData newData, string id)
        {
            _characterData = newData;
            LoadCharacterComponents(in newData);
            _progression.LoadStats(newData.progress, newData.characterType);
            ComponentsList.DDFElement.value = "None";
            EMCharacterType.Value = _characterData.Value.characterType;
            basicStats.stats = _characterData.Value.stats;
            TxtName.value = _characterData.Value.characterName;
            TempName = _characterData.Value.characterName;
            TglSave.value = _characterData.Value.shouldSave;
            _id = id;
        }

        private void AddCharacterComponents(ref CharacterData characterData)
        {
            var components = from comp in ComponentsList.Components
                             where !comp.element.ClassListContains("Disable")
                             select comp;

            foreach (var component in components)
            {
                switch ((ComponentType)component.Type)
                {
                    case ComponentType.Health:
                        float health = ((HealthSettings)subTabs[CharacterTab.Health]).FFHealth.value;

                        characterData.components[ComponentType.Health] = health;
                        break;

                    case ComponentType.Inventory:
                        AddInventoryComponent(ref characterData);
                        break;

                    case ComponentType.Equipment:
                        AddInventoryComponent(ref characterData);

                        var inventory = (Inventory)characterData.components[ComponentType.Inventory];
                        characterData.components[ComponentType.Equipment] = EquipmentS.GetEquipment(in inventory);
                        break;

                    case ComponentType.None:
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        characterData.components[(ComponentType)component.Type] = null;
                        break;
                }
            }
        }

        private void AddInventoryComponent(ref CharacterData characterData)
        {
            if (characterData.components.ContainsKey(ComponentType.Inventory))
                return;

            var inventory = ((InventorySettings)subTabs[CharacterTab.Inventory]).GetInventory();
            characterData.components[ComponentType.Inventory] = inventory;
        }

        private void LoadCharacterComponents(in CharacterData data)
        {
            ComponentsList.Clear();

            foreach (var component in data.components)
            {
                switch (component.Key)
                {
                    case ComponentType.None:
                        continue;

                    case ComponentType.Health:
                        ((HealthSettings)subTabs[CharacterTab.Health]).FFHealth.value = (float)component.Value;
                        break;

                    case ComponentType.Inventory:
                        ((InventorySettings)subTabs[CharacterTab.Inventory]).LoadInventoryItems((Inventory)component.Value);
                        break;


                    case ComponentType.Equipment:
                        EquipmentS.LoadEquipment((Equipment)component.Value);
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        break;
                }

                ComponentsList.AddElement(component.Key.ToString());
            }
        }
    }

    public interface ISubWindowsContainer
    {
        public void CloseWindows();
    }

    public class StatData
    {
        public string name;
        public ModifiableStat type;
        public bool enabled;
    }

    public class StatNameData
    {
        public string header;
        public ModifiableStat type;
        public bool editable;
        public string variableType;
    }
}
