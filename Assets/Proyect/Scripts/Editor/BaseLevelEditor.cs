using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BaseLevelEditor;

public class BaseLevelEditor : EditorWindow
{
    protected VisualElement container;
    Label lblNotification;
    VisualElement pNotification;
    protected Dictionary<string, Button> tabButtons = new();
    protected Dictionary<string, VisualElement> infoContainers = new();
    protected string curTab = "";

    Button selectedButton;
    protected const string acceptButtonName = "AceptButton";
    protected const string cancelButtonName = "CancelButton";

    protected bool changesInTab = false;
    protected BorderColour borderColor = BorderColour.None;
    
    public enum BorderColour
    {
        None,
        Approved,
        Warning,
        HighlightBorder,
        StateBorder,
        BuffBorder,
        CharacterBorder,
        ArmorBorder,
        WeaponBorder,
        ConsumableBorder,
        ItemBorder,
    }


    protected virtual void GetTabButtons() { }
    protected virtual void GetInfoContainers() { }

    protected void GetNotificationSection()
    {
        pNotification = container.Q<VisualElement>("notifications");
        lblNotification = container.Q<Label>("lblNotifications");
        pNotification.AddToClassList("Disable");
    }

    protected void ChangeTab(string tab)
    {
        if (string.IsNullOrEmpty(tab))
        {
            CloseCurrentTab();
            return;
        }

        foreach (var button in infoContainers)
        {
            if (button.Key == tab)
            {
                if (button.Value.ClassListContains("Disable"))
                {
                    button.Value.RemoveFromClassList("Disable");
                    curTab = tab;
                    //SelectButton(tab);
                }
            }
            else if (!button.Value.ClassListContains("Disable"))
            {
                button.Value.AddToClassList("Disable");
            }
        }
    }

    protected void ChangeTab(VisualElement visualElement)
    {
        if (visualElement == null)
        {
            CloseCurrentTab();
            return;
        }

        foreach (var container in infoContainers)
        {
            if (!container.Value.ClassListContains("Disable"))
            {
                container.Value.AddToClassList("Disable");
            }
        }

        if (visualElement.ClassListContains("Disable"))
            visualElement.RemoveFromClassList("Disable");
    }

    protected void CloseCurrentTab()
    {
        foreach (var container in infoContainers.Values)
            EnableContainer(container, false);

        curTab = "";
        return;
    }

    protected void EnableContainer(VisualElement container, bool shouldEnable)
    {
        if (shouldEnable)
        {
            if (container.ClassListContains("Disable"))
            {
                container.RemoveFromClassList("Disable");
            }
        }
        else if (!container.ClassListContains("Disable"))
        {
            container.AddToClassList("Disable");
        }
    }

    protected void SelectTabBtn(string tabButtonName)
    {
        foreach (var button in tabButtons)
        {
            if (button.Key == tabButtonName)
            {
                if (!button.Value.ClassListContains("Selected"))
                {
                    button.Value.AddToClassList("Selected");
                }
            }
            else
            {
                if (button.Value.ClassListContains("Selected"))
                {
                    button.Value.RemoveFromClassList("Selected");
                }
            }
        }
    }

    private void SelectButton(string button, bool shouldSelect = true)
    {
        if (selectedButton != null)
            selectedButton.RemoveFromClassList("Selected");

        if (shouldSelect)
        {
            tabButtons[button].AddToClassList("Selected");
        }
        else if (tabButtons[button].ClassListContains("Selected"))
        {
            tabButtons[button].RemoveFromClassList("Selected");
        }

        selectedButton = tabButtons[button];
    }

    protected void Notify(string message, BorderColour colour)
    {
        string colourText = colour.ToString();
        pNotification.RemoveFromClassList("Disable");
        RomveTags(colourText);

        if (!pNotification.ClassListContains(colourText))
            pNotification.AddToClassList(colourText);

        pNotification.SetEnabled(true);
        lblNotification.text = message;

        void RomveTags(string colourText)
        {
            int count = Enum.GetValues(typeof(BorderColour)).Length;

            for (int i = 0; i < count; i++)
            {
                if ((BorderColour)i == colour)
                {
                    continue;
                }
                else if (pNotification.ClassListContains(colourText))
                {
                    pNotification.RemoveFromClassList(colourText);
                }
            }
        }
    }

    protected void DisableNotification()
    {
        if (!pNotification.ClassListContains("Disable"))
            pNotification.AddToClassList("Disable");
    }

    protected Button GetAceptButton(VisualElement container) =>
        container.Q<Button>(acceptButtonName);

    protected Button GetCancelButton(VisualElement container) =>
        container.Q<Button>(cancelButtonName);

    protected void Highlight(VisualElement textField, bool shouldHighlight, BorderColour colour = BorderColour.HighlightBorder)
    {
        string colourText = colour.ToString();
        RemoveStyleClass(textField, colourText);

        if (shouldHighlight)
        {
            if (!textField.ClassListContains(colourText))
            {
                textField.AddToClassList(colourText);
            }
        }
        else
        {
            if (textField.ClassListContains(colourText))
            {
                textField.RemoveFromClassList(colourText);
            }
        }

        void RemoveStyleClass(VisualElement textField, string colourText)
        {
            int count = Enum.GetValues(typeof(BorderColour)).Length;

            for (int i = 0; i < count; i++)
            {
                string colour = ((BorderColour)i).ToString();

                if (colour == colourText)
                {
                    continue;
                }
                else if (textField.ClassListContains(colour))
                {
                    textField.RemoveFromClassList(colour);
                } 
            }
        }
    }

    protected bool IsHighlighted(VisualElement element)
    {
        return element.ClassListContains(BorderColour.HighlightBorder.ToString());
    }

    protected void SaveChanges()
    {

    }

    protected void LoadChanges()
    {

    }

    protected bool IsNameWrittenCorrectly(string name)
    {
        if (HasSpecialCharacter(name) ||
            HasInvalidNumberInName(name))
            return false;

        return true;
    }

    protected bool HasSpecialCharacter(string value)
    {
        return value.Any(chr => !char.IsLetterOrDigit(chr));
    }

    protected bool HasInvalidNumberInName(string name)
    {
        if (Int32.TryParse(name, out _))
            return true;

        bool startsWithNumber = false;

        foreach (var item in name)
        {
            if (char.IsDigit(item) && !startsWithNumber)
                return true;
            else
                return false;
        }

        return false;
    }
}
