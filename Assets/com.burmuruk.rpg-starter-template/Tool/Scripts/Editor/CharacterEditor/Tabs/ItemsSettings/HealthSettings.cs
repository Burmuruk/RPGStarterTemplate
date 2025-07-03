using Burmuruk.Tesis.Editor.Utilities;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class HealthSettings : SubWindow
    {
        const string INFO_HEALTH_SETTINGS_NAME = "HealthSettings";
        private Health? _changes = null;

        public IntegerField IFMaxHealth { get; set; }
        public IntegerField IFHealth { get; private set; }
        public Button btnBackHealthSettings { get; private set; }

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            _instance = UtilitiesUI.CreateDefaultTab(INFO_HEALTH_SETTINGS_NAME);
            container.hierarchy.Add(_instance);
            IFMaxHealth = container.Q<IntegerField>("maxHealth");
            IFHealth = container.Q<IntegerField>("health");
            IFHealth.value = 0;
            btnBackHealthSettings = _instance.Q<Button>();

            IFHealth.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);
            btnBackHealthSettings.clicked += () => GoBack?.Invoke();
        }

        public void UpdateHealth(in Health value)
        {
            IFHealth.value = value.HP;
            IFMaxHealth.value = value.MaxHP;
            _changes = value;
        }

        private void OnValueChanged_FFHealthValue(ChangeEvent<int> evt)
        {
            if (evt.newValue > IFMaxHealth.value)
            {
                IFHealth.SetValueWithoutNotify(IFMaxHealth.value);
            }

            AddComponentData(ComponentType.Health);
        }

        private void AddComponentData(ComponentType type)
        {
            object item = null;

            item = type switch
            {
                ComponentType.Health => new Health()
                {
                    HP = (int)IFHealth.value
                },
                _ => null
            };

            //characterData.components.TryAdd(type, item);
        }

        public void LoadInfo(in Health value)
        {
            UpdateHealth(value);
        }

        public Health GetInfo()
        {
            return new Health
            {
                HP = IFHealth.value,
                MaxHP = IFMaxHealth.value
            };
        }

        public override void Clear()
        {
            _changes = null;
            IFHealth.value = 0;
            IFMaxHealth.value = 100;
        }

        public override bool VerifyData() => true;

        public override ModificationTypes Check_Changes()
        {
            var modificationType = ModificationTypes.None;

            if (!_changes.HasValue) return ModificationTypes.None;

            if (IFHealth.value != _changes.Value.HP)
                modificationType = ModificationTypes.EditData;

            if (IFMaxHealth.value != _changes.Value.MaxHP)
                modificationType = ModificationTypes.EditData;

            return modificationType;
        }

        public override void Load_Changes()
        {
            IFHealth.value = _changes.Value.HP;
            IFMaxHealth.value = _changes.Value.MaxHP;
        }

        public override void Remove_Changes()
        {
            _changes = null;
        }
    }
}
