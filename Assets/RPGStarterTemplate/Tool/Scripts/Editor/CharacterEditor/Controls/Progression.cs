using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class Progression : SubWindow
    {
        private List<Button> _levelButtons = new List<Button>();
        private int _totalLevels = 0;
        private int _currentLevel = 1;
        private CharacterProgress _changes = null;
        Func<BasicStats> Get_BasicStats;
        Action<BasicStats> Set_BasicStats;
        private List<BasicStats> _statsPerLevel = new();

        private bool _applyForAllLevels = false;
        private CharacterType _characterType;

        public Button BtnGeneralProgression { get; private set; }
        public VisualElement LevelButtonsContainer { get; private set; }
        public Button BtnAdd { get; private set; }
        public Button BtnRemove { get; private set; }

        public void Initialize(VisualElement container, Func<BasicStats> GetStats, Action<BasicStats> SetStats)
        {
            Initialize(container);
            Get_BasicStats = GetStats;
            Set_BasicStats = SetStats;
            _instance = container.Q<VisualElement>("Progression");
            BtnGeneralProgression = _instance.Q<Button>("btnGeneralProgression");
            LevelButtonsContainer = _instance.Q<VisualElement>("levelButtons");
            BtnAdd = _instance.Q<Button>("btnAddLevel");
            BtnRemove = _instance.Q<Button>("btnRemoveLevel");
            _modificationType = ModificationTypes.None;

            BtnAdd.clicked += OnClick_BtnAdd;
            BtnRemove.clicked += OnClick_BtnRemove;
            BtnGeneralProgression.clicked += OnClick_BtnGeneralProgression;

            OnClick_BtnAdd();
            _currentLevel = 1;
            Highlight(_levelButtons[0], true);
        }

        private void OnClick_BtnAdd()
        {
            Get_LevelButton(_totalLevels);
            _totalLevels++;
            BtnRemove.SetEnabled(_totalLevels > 0);
        }

        private void OnClick_BtnGeneralProgression()
        {
            if (_applyForAllLevels)
            {
                Highlight(BtnGeneralProgression, false);
                enable_LevelButtons(true);
                Display_LevelInfo(1);
                _applyForAllLevels = false;
            }
            else
            {
                enable_LevelButtons(false);
                Display_LevelInfo(0);
                _applyForAllLevels = true;
            }
        }

        private void OnClick_BtnRemove()
        {
            if (_totalLevels > 0)
            {
                Disable_LevelButton();
                _totalLevels--;
            }

            BtnRemove.SetEnabled(_totalLevels > 0);
        }

        private Button Get_LevelButton(int idx)
        {
            if (idx < _levelButtons.Count)
            {
                EnableContainer(_levelButtons[idx], true);
                return _levelButtons[idx];
            }

            var newButton = new Button(() => Display_LevelInfo(idx + 1))
            {
                text = (_levelButtons.Count + 1).ToString(),
            };
            newButton.AddToClassList("LineExtraButton");

            _levelButtons.Add(newButton);
            LevelButtonsContainer.Add(newButton);
            return newButton;
        }

        private void Disable_LevelButton()
        {
            EnableContainer(_levelButtons[_totalLevels - 1], false);
        }

        private void enable_LevelButtons(bool enable)
        {
            if (!enable)
                _levelButtons.ForEach(button => EnableContainer(button, false));
            else
            {
                for (int i = 0; i < _totalLevels; i++)
                {
                    EnableContainer(_levelButtons[i], true);
                }
            }

            BtnAdd.SetEnabled(enable);
            BtnRemove.SetEnabled(enable ? _totalLevels > 0 : false);
        }

        private void Display_LevelInfo(int idx)
        {
            if (_statsPerLevel.Count <= idx)
            {
                while (_statsPerLevel.Count <= idx)
                {
                    _statsPerLevel.Add(new BasicStats());
                }
            }

            if (idx == _currentLevel) return;

            _statsPerLevel[_currentLevel] = Get_BasicStats();
            Set_BasicStats(_statsPerLevel[idx]);

            if (idx > 0)
            {
                Highlight(_levelButtons[_currentLevel - 1], false);
                _currentLevel = idx;
                Highlight(_levelButtons[_currentLevel - 1], true);
            }
            else
            {
                Highlight(BtnGeneralProgression, true);
            }
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return ModificationTypes.None;

            int i = 0;
            while (_changes.GetDataByLevel(_characterType, i) is var stats && stats.HasValue)
            {
                if (_statsPerLevel[i + 1] != stats.Value)
                    _modificationType = ModificationTypes.EditData;
            }

            return _modificationType;
        }

        public override void Clear()
        {
            Highlight(BtnGeneralProgression, false);
            enable_LevelButtons(true);
            _totalLevels = 1;
            _changes = null;
            _statsPerLevel.Clear();
            Set_BasicStats(default);
        }

        public override void Remove_Changes()
        {
            LoadStats(_changes, _characterType);
        }

        public void LoadStats(CharacterProgress progress, CharacterType type)
        {
            _statsPerLevel.Clear();
            _statsPerLevel.Add(new());
            _characterType = type;
            int i = 0;

            while (progress.GetDataByLevel(type, i++) is var stats && stats.HasValue)
            {
                _statsPerLevel.Add(stats.Value);
            }

            _changes = progress;

            if (_statsPerLevel.Count > 1)
            {
                _totalLevels = 1;
                Display_LevelInfo(1);
            }
            else
            {
                _totalLevels = 0;
                Display_LevelInfo(0);
            }
        }

        public CharacterProgress Get_Info()
        {
            var progress = new CharacterProgress();
            var levels = new List<CharacterProgress.LevelData>();

            if (_applyForAllLevels)
            {
                levels.Add(new CharacterProgress.LevelData
                {
                    level = 1,
                    stats = _statsPerLevel[0],
                });
            }
            else
            {
                int i = 1;
                foreach (var level in _statsPerLevel)
                {
                    levels.Add(new CharacterProgress.LevelData
                    {
                        level = i,
                        stats = level,
                    });
                }
            }

            progress.SetData(_characterType, _applyForAllLevels, levels);

            return progress;
        }
    }
}

