using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class HealthSettings : BaseItemSetting, ISubWindow
    {
        private float _health;

        public event Action GoBack;

        public FloatField FFHealth { get; private set; }
        public Button btnBackHealthSettings { get; private set; }

        public override void Initialize(VisualElement container, NameSettings nameControl)
        {
            FFHealth = container.Q<FloatField>("health");
            FFHealth.value = 0;

            FFHealth.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);

            btnBackHealthSettings = container.Q<Button>();

            btnBackHealthSettings.clicked += GoBack;
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

            characterData.components.TryAdd(type, item);
        }
    }
}
