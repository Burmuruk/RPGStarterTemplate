using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Burmuruk.Tesis.Editor.Utilities
{
    public static class UtilitiesUI
    {
        public static VisualElement pNotification = null;
        public static Label lblNotification;
        static Regex regName = new Regex(@"(?m)^[a-zA-Z](?!.*\W)+\w*");

        public static void Notify(string message, BorderColour colour)
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

        public static void DisableNotification()
        {
            if (!pNotification.ClassListContains("Disable"))
                pNotification.AddToClassList("Disable");
        }

        public static void Highlight(VisualElement textField, bool shouldHighlight, BorderColour colour = BorderColour.HighlightBorder)
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

        public static bool IsHighlighted(VisualElement element, out BorderColour colour)
        {
            colour = BorderColour.None;

            foreach (BorderColour value in Enum.GetValues(typeof(BorderColour)))
            {
                if (element.ClassListContains(value.ToString()))
                {
                    colour = value;
                    return true;
                }
            }

            return false;
        }

        public static bool IsHighlighted(VisualElement element)
        {
            return element.ClassListContains(BorderColour.HighlightBorder.ToString());
        }

        public static void EnableContainer(VisualElement container, bool shouldEnable)
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

        public static bool VerifyVariableName(string name) => regName.IsMatch(name);

        public static bool VerifyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Notify("Name can't be empty.", BorderColour.Error);
                return false;
            }

            //if (IsTheNameUsed(name))
            //{
            //    Notify("The name it's already been used.", BorderColour.Error);
            //    return false;
            //}

            if (!VerifyVariableName(name))
            {
                Notify("Invalid name", BorderColour.Error);
                return false;
            }

            return true;
        }

        public static void HighlightChanges<T>()
        {
            //if (a != b)
            //{

            //}
        }

        public static VisualElement CreateDefaultTab(string fileName)
        {
            VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/{fileName}.uxml").Instantiate();
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
            StyleSheet styleSheetColour = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/BorderColours.uss");
            newContainer.styleSheets.Add(styleSheet);
            newContainer.styleSheets.Add(styleSheetColour);

            return newContainer;
        }
    }
}
