using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ProgressionUIManager : SubWindow
    {
        public Button BtnBaseInfo { get; private set; }
        public Button BtnGeneralProgression { get; private set; }
        public Button BtnAdd { get; private set; }
        public Button BtnRemove { get; private set; }
        public VisualElement StatsContainer { get; private set; }
        public VisualElement LevelButtonsContainer { get; private set; }

        private List<Button> _levelButtons = new();
        private List<BasicStats> _statsPerLevel = new();
        private BasicStats _baseInfo;
        private BasicStats? _changesBaseInfo = null;

        private Button _selectedButton = null;
        private int _currentLevel = 1;
        private int _totalLevels = 0;
        private bool _applyForAllLevels = false;
        private CharacterProgress _changes = null;
        private CharacterType _changesCharacterType;
        private CharacterType _characterType;
        private BasicStats? CurrentData
        {
            get
            {
                if (_selectedButton == null) return null;

                if (_selectedButton == BtnBaseInfo)
                {
                    return _baseInfo;
                }
                else if (_selectedButton == BtnGeneralProgression)
                {
                    return _statsPerLevel[0];
                }
                else
                {
                    int idx = _levelButtons.IndexOf(_selectedButton);
                    return _statsPerLevel[idx];
                }
            }

            set
            {
                if (_selectedButton == null) return;

                if (_selectedButton == BtnBaseInfo)
                {
                    _baseInfo = value.Value;
                }
                else if (_selectedButton == BtnGeneralProgression)
                {
                    _statsPerLevel[0] = value.Value;
                }
                else
                {
                    int idx = _levelButtons.IndexOf(_selectedButton);
                    _statsPerLevel[idx] = value.Value;
                }
            }
        }

        private Func<BasicStats> _getStats;
        private Action<BasicStats> _setStats;

        public void Initialize(VisualElement container, Func<BasicStats> getStats, Action<BasicStats> setStats)
        {
            _getStats = getStats;
            _setStats = setStats;

            _instance = container.Q<VisualElement>("Progression");
            BtnBaseInfo = _instance.Q<Button>("btnBaseInfo");
            BtnGeneralProgression = _instance.Q<Button>("btnGeneralProgression");
            BtnAdd = _instance.Q<Button>("btnAddLevel");
            BtnRemove = _instance.Q<Button>("btnRemoveLevel");
            StatsContainer = container.Q<VisualElement>("statsContainer");
            LevelButtonsContainer = _instance.Q<VisualElement>("levelButtons");

            BtnBaseInfo.clicked += () => SwitchView(BtnBaseInfo);
            BtnGeneralProgression.clicked += () => SwitchView(BtnGeneralProgression);
            BtnAdd.clicked += AddLevel;
            BtnRemove.clicked += RemoveLevel;

            _levelButtons.Add(BtnGeneralProgression);
            _statsPerLevel.Add(new BasicStats());
            Highlight(BtnBaseInfo, true);
            _selectedButton = BtnBaseInfo;
            EnableContainer(StatsContainer, true);
        }

        public void Set_CharacterType(CharacterType type) => _characterType = type;

        private void SwitchView(Button newButton)
        {
            SaveCurrentStats();

            if (_selectedButton == newButton)
            {
                Highlight(_selectedButton, false);
                EnableContainer(StatsContainer, false);
                _selectedButton = null;
                return;
            }

            if (_selectedButton != null)
            {
                Highlight(_selectedButton, false);
                CurrentData = _getStats();
            }

            Highlight(newButton, true);
            _selectedButton = newButton;

            EnableContainer(StatsContainer, true);

            if (newButton == BtnBaseInfo)
            {
                _setStats(_baseInfo);
                ToggleLevelButtons(true);
            }
            else if (newButton == BtnGeneralProgression)
            {
                _applyForAllLevels = true;
                _setStats(_statsPerLevel[0]);
                ToggleLevelButtons(false);
            }
            else
            {
                int index = _levelButtons.IndexOf(newButton);
                _currentLevel = index;
                _setStats(_statsPerLevel[index]);
                _applyForAllLevels = false;
                ToggleLevelButtons(true);
            }
        }

        private void SaveCurrentStats()
        {
            if (_selectedButton == null) return;

            if (_selectedButton == BtnBaseInfo)
            {
                _baseInfo = _getStats();
            }
            else if (_selectedButton == BtnGeneralProgression || _selectedButton == null)
            {
                _statsPerLevel[0] = _getStats();
            }
            else
            {
                int index = _levelButtons.IndexOf(_selectedButton);
                _statsPerLevel[index] = _getStats();
            }
        }

        private void AddLevel()
        {
            _totalLevels++;
            var levelIndex = _levelButtons.Count;
            Button newButton = null;
            newButton = new Button(() => SwitchView(newButton))
            {
                text = levelIndex.ToString()
            };

            _levelButtons.Add(newButton);
            _statsPerLevel.Add(new BasicStats());
            newButton.AddToClassList("LineExtraButton");
            LevelButtonsContainer.Add(newButton);

            BtnRemove.SetEnabled(true);
        }

        private void RemoveLevel()
        {
            if (_totalLevels <= 0) return;

            int lastIndex = _levelButtons.Count - 1;
            LevelButtonsContainer.Remove(_levelButtons[lastIndex]);
            _levelButtons.RemoveAt(lastIndex);
            _statsPerLevel.RemoveAt(lastIndex);
            _totalLevels--;

            BtnRemove.SetEnabled(_totalLevels > 0);
        }

        private void ToggleLevelButtons(bool enable)
        {
            for (int i = 1; i < _levelButtons.Count; i++)
            {
                EnableContainer(_levelButtons[i], enable);
            }

            ToggleAdditionButtons(enable);
        }

        private void ToggleAdditionButtons(bool enable)
        {
            BtnAdd.SetEnabled(enable);
            BtnRemove.SetEnabled(enable && _totalLevels > 0);
        }

        public void LoadStats(CharacterProgress progress, BasicStats baseInfo, CharacterType type)
        {
            _changes = progress;
            _baseInfo = baseInfo;
            _changesBaseInfo = baseInfo;
            _characterType = type;
            _changesCharacterType = type;
            _selectedButton = null;

            _statsPerLevel.Clear();
            LevelButtonsContainer.Clear();
            LevelButtonsContainer.Add(BtnGeneralProgression);
            _levelButtons.RemoveRange(1, _levelButtons.Count - 1);

            if (progress.ApplyForAll(type))
            {
                _applyForAllLevels = true;
                _statsPerLevel.Add(progress.GetDataByLevel(type, 1).Value);
                SwitchView(BtnGeneralProgression);
            }
            else
            {
                _applyForAllLevels = false;
                _statsPerLevel.Add(new BasicStats());
                int i = 1;
                BasicStats? data;
                do
                {
                    data = progress.GetDataByLevel(type, i);
                    if (data.HasValue)
                    {
                        AddLevel();
                        _statsPerLevel[_statsPerLevel.Count - 1] = data.Value;
                    }
                    i++;
                } while (data.HasValue);

                _currentLevel = 1;
                SwitchView(BtnBaseInfo);
            }
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return ModificationTypes.None;

            ModificationTypes changes = ModificationTypes.None;

            if (_characterType != _changesCharacterType)
                changes |= ModificationTypes.EditData;

            SaveCurrentStats();

            FieldInfo[] fields = typeof(BasicStats).GetFields();

            if (_applyForAllLevels)
            {
                var prev = _changes.GetDataByLevel(_changesCharacterType, 1);
                if (!prev.HasValue || HasChanges(fields, prev.Value, _statsPerLevel[0]))
                    changes |= ModificationTypes.EditData;
            }
            else
            {
                for (int i = 1; i < _statsPerLevel.Count; i++)
                {
                    var prev = _changes.GetDataByLevel(_changesCharacterType, i);
                    if (!prev.HasValue || HasChanges(fields, prev.Value, _statsPerLevel[i]))
                    {
                        changes |= ModificationTypes.EditData;
                        break;
                    }
                }
            }

            if (_changesBaseInfo.HasValue && HasChanges(fields, _changesBaseInfo.Value, _baseInfo))
                changes |= ModificationTypes.EditData;

            return changes;

            bool HasChanges(FieldInfo[] fields, BasicStats a, BasicStats b)
            {
                foreach (var field in fields)
                {
                    if (!Equals(field.GetValue(a), field.GetValue(b)))
                        return true;
                }
                return false;
            }
        }

        public void Get_Info(out CharacterProgress progress, out BasicStats baseInfo)
        {
            baseInfo = _baseInfo;
            progress = new CharacterProgress();

            SaveCurrentStats();

            List<CharacterProgress.LevelData> levels = new();

            if (_applyForAllLevels)
            {
                levels.Add(new CharacterProgress.LevelData
                {
                    level = 1,
                    stats = _statsPerLevel[0]
                });
            }
            else
            {
                for (int i = 1; i < _statsPerLevel.Count; i++)
                {
                    levels.Add(new CharacterProgress.LevelData
                    {
                        level = i,
                        stats = _statsPerLevel[i]
                    });
                }
            }

            progress.SetData(_characterType, _applyForAllLevels, levels);
        }

        public override void Clear()
        {
            Highlight(BtnBaseInfo, false);
            Highlight(BtnGeneralProgression, false);
            if (_selectedButton != null)
                Highlight(_selectedButton, false);

            _selectedButton = null;
            _currentLevel = 1;
            _totalLevels = 0;
            _applyForAllLevels = false;
            _changes = null;
            _changesCharacterType = CharacterType.None;
            _changesBaseInfo = null;

            _statsPerLevel.Clear();
            _levelButtons.RemoveRange(1, _levelButtons.Count - 1);
            BtnRemove.SetEnabled(false);
            EnableContainer(StatsContainer, false);
        }

        public override void Remove_Changes()
        {
            _changes = null;
            _changesCharacterType = CharacterType.None;
            _changesBaseInfo = null;
        }

        public override bool VerifyData() => true;

        public override void Load_Changes()
        {
            LoadStats(_changes, _changesBaseInfo.Value, _characterType);
        }
    }
}
