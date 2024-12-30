using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BuffsDataUI
    {
        VisualElement dataContainer;

        public VisualElement Element { get; private set; }
        public DropdownField DDBuff { get; private set; }
        public FloatField Value { get; private set; }
        public FloatField Duration { get; private set; }
        public FloatField Rate { get; private set; }
        public Toggle Percentage { get; private set; }
        public Toggle AffectAll { get; private set; }
        public FloatField Probability { get; private set; }

        public BuffsDataUI()
        {
            VisualTreeAsset ElementTag = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Controls/BuffsAdder.uxml");
            StyleSheet basicStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BasicSS.uss");
            StyleSheet lineStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
            Element = ElementTag.Instantiate();
            Element.styleSheets.Add(basicStyleSheet);
            Element.styleSheets.Add(lineStyleSheet);

            DDBuff = Element.Q<DropdownField>("buffType");
            dataContainer = Element.Q<VisualElement>("dataContainer");
            Value = Element.Q<FloatField>("ffValue");
            Duration = Element.Q<FloatField>("ffDuration");
            Rate = Element.Q<FloatField>("ffRate");
            Percentage = Element.Q<Toggle>("tglPercentage");
            AffectAll = Element.Q<Toggle>("tglAffectAll");
            Probability = Element.Q<FloatField>("ffProbability");

            DDBuff.choices.Clear();
            DDBuff.SetValueWithoutNotify("None");
            DDBuff.RegisterValueChangedCallback(OnValueChanged_BuffType);
        }

        private void OnValueChanged_BuffType(ChangeEvent<string> evt)
        {
            bool shouldEnable = string.IsNullOrEmpty(evt.newValue) || evt.newValue == "Custom";

            TabCharacterEditor.EnableContainer(dataContainer, shouldEnable);
        }

        public void SetValues(List<string> values)
        {
            (values ??= new()).Insert(0, "Custom");

            DDBuff.choices = values;
        }
    }
}
