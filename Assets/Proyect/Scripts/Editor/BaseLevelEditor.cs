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
            else
            {
                button.Value.AddToClassList("Disable");
            }
        }
    }

    protected void ChangeTab(VisualElement visualElement)
    {
        foreach (var button in infoContainers)
        {
            button.Value.AddToClassList("Disable");
        }

        visualElement.RemoveFromClassList("Disable");
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
}
