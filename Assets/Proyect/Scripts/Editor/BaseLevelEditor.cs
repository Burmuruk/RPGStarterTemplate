using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public enum BorderColour
    {
        None,
        Approved,
        Error,
        HighlightBorder,
        StateBorder,
        BuffBorder,
        CharacterBorder,
        ArmorBorder,
        WeaponBorder,
        ConsumableBorder,
        ItemBorder,
    }

    public class BaseLevelEditor : EditorWindow
    {
        protected VisualElement container;

        protected Dictionary<string, Button> tabButtons = new();
        protected Dictionary<string, VisualElement> infoContainers = new();
        protected string curTab = "";
        protected string lastTab = "";

        Button selectedButton;
        protected const string acceptButtonName = "AceptButton";
        protected const string cancelButtonName = "CancelButton";

        protected bool changesInTab = false;
        protected BorderColour borderColor = BorderColour.None;

        protected virtual void GetTabButtons() { }
        protected virtual void GetInfoContainers() { }

        protected void GetNotificationSection()
        {
            pNotification = container.Q<VisualElement>("notifications");
            lblNotification = container.Q<Label>("lblNotifications");
            pNotification.AddToClassList("Disable");
        }

        protected virtual void ChangeTab(string tab)
        {
            lastTab = curTab;

            if (string.IsNullOrEmpty(tab))
            {
                CloseCurrentTab();
                curTab = "";
                return;
            }

            foreach (var curTab in infoContainers.Values)
            {
                EnableContainer(curTab, false);
            }

            EnableContainer(infoContainers[tab], true);
            curTab = tab;
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
            if (string.IsNullOrEmpty(curTab)) return;

            EnableContainer(infoContainers[curTab], false);
            return;
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

        protected Button GetAceptButton(VisualElement container) =>
            container.Q<Button>(acceptButtonName);

        protected Button GetCancelButton(VisualElement container) =>
            container.Q<Button>(cancelButtonName);

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
}
