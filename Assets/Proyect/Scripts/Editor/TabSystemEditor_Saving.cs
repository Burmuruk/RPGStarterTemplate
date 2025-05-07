using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabSystemEditor : BaseLevelEditor
    {
        TextField[] savingTxtFields;
        string[] enumValues;
        const int systemEnumValues = 3;

        private void InitializeSaving()
        {
            var savingButtons = container.Q<VisualElement>("SavingButtons");

            GetAceptButton(savingButtons).clicked += OnAccept_SavingBtn;
            GetCancelButton(savingButtons).clicked += OnCanceled_SavingBtn;
        }

        private void Show_Saving()
        {
            if (changesInTab) ;
            //Display warning

            DisableNotification();
            ChangeTab(infoSavingName);
            SelectTabBtn(btnSavingName);
            CreateSavingTextFields();
            EnableSavingButtons(CheckCanges().changes);
        }

        private void CreateSavingTextFields()
        {
            var infoContainer = container.Q<VisualElement>("savingInfoCont");
            infoContainer.Clear();
            int enumCount = Enum.GetValues(typeof(SavingExecution)).Length;
            enumValues = new string[enumCount];
            savingTxtFields = new TextField[15];

            for (int i = 0; i < savingTxtFields.Length; i++)
            {
                TextField textField = new TextField();

                if (i < enumCount)
                {
                    textField.value = ((SavingExecution)i).ToString();
                    enumValues[i] = textField.value;

                    if (i >= systemEnumValues)
                    {
                        textField.RegisterCallback<KeyUpEvent>(AddEnumSavingStep);
                    }
                    else
                    {
                        textField.isReadOnly = true;
                        textField.SetEnabled(false);
                    }
                }
                else
                {
                    textField.value = "";
                    textField.RegisterCallback<KeyUpEvent>(AddEnumSavingStep);
                }

                infoContainer.Add(textField);
                savingTxtFields[i] = textField;
            }
        }

        private void AddEnumSavingStep(KeyUpEvent e)
        {
            bool error;
            (changesInTab, error) = CheckCanges();

            if (!error)
                EnableSavingButtons(changesInTab);
        }

        private (bool changes, bool error) CheckCanges()
        {
            bool hasChanges = false;
            bool hasErrors = false;

            for (int i = 0; i < savingTxtFields.Length; i++)
            {
                if (i < enumValues.Length)
                {
                    if (enumValues[i].ToLower() != savingTxtFields[i].value.ToLower())
                    {
                        hasChanges = true;
                        Highlight(savingTxtFields[i], true);
                    }
                    else
                        Highlight(savingTxtFields[i], false);
                }
                else if (!string.IsNullOrEmpty(savingTxtFields[i].value))
                {
                    hasChanges = true;
                    Highlight(savingTxtFields[i], true);
                }
                else if (HasSpecialCharacter(savingTxtFields[i].value))
                {
                    Highlight(savingTxtFields[i], false, BorderColour.Error);
                    hasErrors = true;
                    Notify("Special characteres are not allowed.", BorderColour.Error);
                }
                else
                {
                    Highlight(savingTxtFields[i], false);
                }
            }

            return (hasChanges, hasErrors);
        }

        private void EnableSavingButtons(bool shouldEnable)
        {
            var savingButtons = container.Q<VisualElement>("SavingButtons");
            var buttonA = GetAceptButton(savingButtons);


            if (shouldEnable)
            {
                if (buttonA.ClassListContains("Invisible"))
                {
                    GetAceptButton(savingButtons).RemoveFromClassList("Invisible");
                    GetCancelButton(savingButtons).RemoveFromClassList("Invisible");
                }
            }
            else if (!buttonA.ClassListContains("Invisible"))
            {
                GetAceptButton(savingButtons).AddToClassList("Invisible");
                GetCancelButton(savingButtons).AddToClassList("Invisible");
            }
        }

        private void OnAccept_SavingBtn()
        {
            var enumEditor = new EnumEditor();
            List<string> newValues = new();

            foreach (var textFiel in savingTxtFields)
            {
                if (string.IsNullOrEmpty(textFiel.value))
                    continue;

                newValues.Add(textFiel.value.Trim());
            }

            if (newValues.Count > 0)
            {
                Notify("No changes found.", BorderColour.Approved);
                return;
            }

            if (enumEditor.Modify("SavingExecution", newValues.ToArray(), "", out string error))
            {
                ClearSavingValues();
                EnableSavingButtons(false);
                Notify("Changes applied.", BorderColour.Approved);
            }
            else if (!string.IsNullOrEmpty(error))
            {
                Notify(error, BorderColour.Error);
            }
            else
            {
                Notify("Error ocurred when trying to apply changes.", BorderColour.Error);
            }
        }

        private void OnCanceled_SavingBtn()
        {
            ClearSavingValues();
            EnableSavingButtons(CheckCanges().changes);
        }

        private void ClearSavingValues()
        {
            for (int i = 0; i < savingTxtFields.Length; i++)
            {
                if (i < enumValues.Length)
                {
                    savingTxtFields[i].value = enumValues[i];
                }
                else
                {
                    savingTxtFields[i].value = "";
                }
            }
        }
    }
}
