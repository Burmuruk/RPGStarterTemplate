using Burmuruk.Tesis.Editor.Utilities;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class HealthSettings : SubWindow
    {
        const string INFO_HEALTH_SETTINGS_NAME = "HealthSettings";
        private float? _health = null;

        public FloatField FFHealth { get; private set; }
        public Button btnBackHealthSettings { get; private set; }

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            _instance = UtilitiesUI.CreateDefaultTab(INFO_HEALTH_SETTINGS_NAME);
            container.hierarchy.Add(_instance);
            FFHealth = container.Q<FloatField>();
            FFHealth.value = 0;
            btnBackHealthSettings = _instance.Q<Button>();

            FFHealth.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);
            btnBackHealthSettings.clicked += () => GoBack?.Invoke();
        }

        public void UpdateHealth(float value)
        {
            FFHealth.value = value;
            _health = value;
        }

        private void OnValueChanged_FFHealthValue(ChangeEvent<float> evt)
        {
            AddComponentData(ComponentType.Health);
        }

        private void AddComponentData(ComponentType type)
        {
            object item = null;

            item = type switch
            {
                ComponentType.Health => new Health()
                {
                    HP = (int)FFHealth.value
                },
                _ => null
            };

            //characterData.components.TryAdd(type, item);
        }

        public void LoadInfo(float value)
        {
            _health = value;
            FFHealth.value = _health.Value;
        }

        public override void Clear()
        {
            _health = 0;
            FFHealth.value = 0;
        }

        public override bool VerifyData() => true;

        public override ModificationTypes Check_Changes()
        {
            var modificationType = ModificationTypes.None;

            if (!_health.HasValue) return ModificationTypes.None;

            if (FFHealth.value != _health)
                modificationType = ModificationTypes.EditData;

            return modificationType;
        }

        public override void Load_Changes()
        {
            FFHealth.value = _health.Value;
        }

        public override void Remove_Changes()
        {
            _health = null;
        }
    }
}
