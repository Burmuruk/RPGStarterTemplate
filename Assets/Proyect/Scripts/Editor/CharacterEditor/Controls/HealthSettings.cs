using Burmuruk.Tesis.Editor.Utilities;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class HealthSettings : SubWindow
    {
        const string INFO_HEALTH_SETTINGS_NAME = "HealthSettings";
        private float _health;

        public FloatField FFHealth { get; private set; }
        public Button btnBackHealthSettings { get; private set; }

        public override void Initialize(VisualElement container, NameSettings nameControl)
        {
            base.Initialize(container, nameControl);

            var control = UtilitiesUI.CreateDefaultTab(INFO_HEALTH_SETTINGS_NAME);
            container.hierarchy.Add(control);
            _instance = control;
            FFHealth = container.Q<FloatField>();
            FFHealth.value = 0;
            btnBackHealthSettings = container.Q<Button>();

            FFHealth.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);
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

            //characterData.components.TryAdd(type, item);
        }

        public override void Clear()
        {
            _health = 0;
            FFHealth.value = 0;
        }

        public override ModificationType Check_Changes()
        {
            var modificationType = ModificationType.None;

            if (FFHealth.value != _health)
                modificationType = ModificationType.EditData;

            return modificationType;
        }

        public override void Remove_Changes()
        {
            FFHealth.value = _health;
        }
    }
}
