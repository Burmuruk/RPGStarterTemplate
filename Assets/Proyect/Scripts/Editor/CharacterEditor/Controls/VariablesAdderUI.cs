using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Utilities.VariablesAdder;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public class VariablesAdderUI
    {
        List<(Label label, VariableType type)> newVariables = new();
        VisualElement target;

        public Button ButtonAdd { get; private set; }
        public Button ButtonCancel { get; private set; }
        public EnumField EFType { get; private set; }
        public TextField TxtName { get; private set; }
        public VisualElement ValuesContainer { get; private set; }

        public VariablesAdderUI(VisualElement container, VisualElement target)
        {
            ButtonAdd = container.Q<Button>("btnAddBasicStats");
            ButtonCancel = container.Q<Button>("btnCancel");
            EFType = container.Q<EnumField>();
            TxtName = container.Q<TextField>("txtName");
            ValuesContainer = container.Q<VisualElement>("newValuesContainer");

            this.target = ValuesContainer;
            ButtonAdd.clicked += OnClick_AddButton;
            EFType.Init(VariableType.Int);

            TxtName.RegisterCallback<KeyUpEvent>(OnKeyUp_TxtName);
            ButtonCancel.clicked += () =>
            {
                ShowElements(false);
                RemoveExtraValues();
                ResetValues();
            };

            ShowElements(false);
        }

        public void AddVariable()
        {
            if (!VerifyVariableName(TxtName.value)) return;

            //Path.GetDirectoryName(typeof(vari).Assembly.Location);

            string path = Path.GetDirectoryName(typeof(BasicStats).Assembly.Location);
            //Debug.Log( AssetDatabase.get("BasicStats") + "  \tAsset database");
            VariablesAdder adder = new(path);

            Debug.Log(path);

            Label newElement = new(TxtName.text);

            newVariables.Add((newElement, (VariableType)EFType.value));
            target.Add(newElement);
        }

        public void RemoveExtraValues()
        {
            while (newVariables.Count > 0)
            {
                target.Remove(newVariables[0].label);
                newVariables.RemoveAt(0);
            }

            ShowElements(false);
        }

        public void ApplyChanges()
        {
            string path = Path.GetDirectoryName(typeof(BasicStats).Assembly.Location);
            VariablesAdder adder = new(path);
            Debug.Log(path);

            List<(VariableType type, string name)> values = new();

            foreach ((Label label, VariableType type) value in newVariables)
            {
                values.Add((value.type, value.label.text));
            }

            if (!adder.Modify(path, values.ToArray(), out string error))
            {
                //notify
            }

            RemoveExtraValues();
            ResetValues();
            ShowElements(false);
        }

        private void OnKeyUp_TxtName(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                AddVariable();
                TxtName.value = "";
                TxtName.Focus();
            }
        }

        private void OnClick_AddButton()
        {
            if (EFType.ClassListContains("Disable"))
            {
                ButtonAdd.text = "Apply changes";
                ShowElements(true);
                TxtName.Focus();
            }
            else
            {
                ApplyChanges();
                ResetValues();

                ShowElements(false);
            }
        }

        private void ResetValues()
        {
            ButtonAdd.text = "Add basic stat";
            TxtName.value = "Name";
            EFType.value = VariableType.Int;
        }

        private void ShowElements(bool shouldShow = true)
        {
            EnableContainer(EFType, shouldShow);
            EnableContainer(TxtName, shouldShow);
            EnableContainer(ButtonCancel, shouldShow);
        }
    }
}
