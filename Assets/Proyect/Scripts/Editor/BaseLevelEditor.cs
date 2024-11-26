using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseLevelEditor : EditorWindow
{
    protected VisualElement container;
    Label lblNotification;
    VisualElement pNotification;
    protected Dictionary<string, Button> tabButtons = new();
    protected Dictionary<string, VisualElement> infoContainers = new();

    Button selectedButton;
    protected const string acceptButtonName = "AceptButton";
    protected const string cancelButtonName = "CancelButton";

    protected bool changesInTab = false;

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
        foreach (var button in infoContainers)
        {
            if (button.Key == tab)
            {
                if (button.Value.ClassListContains("Disable"))
                {
                    button.Value.RemoveFromClassList("Disable");
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

    protected void Notify(string message, Color color)
    {
        pNotification.RemoveFromClassList("Disable");
        pNotification.SetEnabled(true);
        lblNotification.text = message;
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
}
