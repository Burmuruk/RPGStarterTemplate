using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public partial class TabSystemEditor : BaseLevelEditor
{
    TextField[] savingTxtFields;
    string[] enumValues;

    private void InitializeSaving()
    {
        var savingButtons = container.Q<VisualElement>("SavingButtons");

        GetAceptButton(savingButtons).clicked += OnAccept_SavingBtn;
        GetCancelButton(savingButtons).clicked += OnCanceled_SavingBtn;
    }

    private void Show_Saving()
    {
        if (changesInTab)
            //Display warning

        DisableNotification();
        ChangeTab(infoSavingName);
        SelectTabBtn(btnSavingName);
        CreateSavingTextFields();
        EnableSavingButtons(CheckCanges());
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
                textField.isReadOnly = true;
                textField.SetEnabled(false);
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
        changesInTab = CheckCanges();
        EnableSavingButtons(changesInTab);
    }

    private bool CheckCanges()
    {
        bool hasChanges = false;

        for (int i = 0; i < savingTxtFields.Length; i++)
        {
            if (i < enumValues.Length)
            {
                if (enumValues[i].ToLower() != savingTxtFields[i].value.ToLower())
                {
                    hasChanges = true;
                    if (!savingTxtFields[i].ClassListContains("HighlightBorder"))
                        savingTxtFields[i].AddToClassList("HighlightBorder"); 
                }
            }
            else if (!string.IsNullOrEmpty(savingTxtFields[i].value))
            {
                hasChanges = true;
                if (!savingTxtFields[i].ClassListContains("HighlightBorder"))
                {
                    savingTxtFields[i].AddToClassList("HighlightBorder");
                }
            }
            else
            {
                if (savingTxtFields[i].ClassListContains("HighlightBorder"))
                {
                    savingTxtFields[i].RemoveFromClassList("HighlightBorder");
                }
            }
        }
        
        return hasChanges;
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

            newValues.Add(textFiel.value);
        }

        if (newValues.Count > 0)
        {
            Notify("No changes found.", Color.red);
            return;
        }

        if (enumEditor.Modify("SavingExecution", newValues.ToArray(), ""))
        {
            ClearSavingValues();
            EnableSavingButtons(false);
            Notify("Changes applied.", Color.green);
        }
        else
        {
            Notify("Error ocurred when trying to apply changes.", Color.red);
        }
    }

    private void OnCanceled_SavingBtn()
    {
        ClearSavingValues();
        EnableSavingButtons(false);
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
