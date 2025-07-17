using Burmuruk.RPGStarterTemplate.Utilities;
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Utilities
{
    public static class UtilitiesUI
    {
        private const string BORDER_COLOURS_STYLESHEET_NAME = "BorderColours";
        static Regex regVariableName = new Regex(@"(?m)^[a-zA-Z](?!.*\W)+\w*", RegexOptions.Compiled);
        static Regex regName = new Regex(@"(?m)^[a-zA-Z](?!.*\W)+\w*", RegexOptions.Compiled);
        private static StyleSheet _hightlight_Colours;
        public static VisualElement pNotification = null;
        public static Label lblNotification;
        static IVisualElementScheduledItem pNotificationTimeout = null;
        public static readonly string[] Keywords = new[]
        {
            // base
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
            "namespace", "new", "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly", "ref", "return",
            "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
            "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile", "while",

            // C# 7+
            "add", "alias", "async", "await", "dynamic", "from", "get", "global",
            "group", "into", "join", "let", "orderby", "partial", "remove", "select",
            "set", "value", "var", "where", "yield",

            // C# 9
            "record", "init", "with", "not"
        };

        public static void Notify(string message, BorderColour colour)
        {
            pNotificationTimeout?.Pause();
            pNotificationTimeout = null;

            string colourText = colour.ToString();
            pNotification.RemoveFromClassList("Disable");
            RomveTags(colourText);

            if (!pNotification.ClassListContains(colourText))
                pNotification.AddToClassList(colourText);

            lblNotification.text = message;
            pNotificationTimeout = pNotification.schedule.Execute(() =>
            {
                DisableNotification();
            });

            pNotificationTimeout.ExecuteLater(5000);

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

        public static void Highlight(VisualElement element, bool shouldHighlight, BorderColour colour = BorderColour.HighlightBorder)
        {
            string colourText = colour.ToString();
            RemoveStyleClass(element, colourText);

            if (shouldHighlight)
            {
                if (!element.ClassListContains(colourText))
                {
                    element.AddToClassList(colourText);
                }
            }
            else
            {
                if (element.ClassListContains(colourText))
                {
                    element.RemoveFromClassList(colourText);
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

        public static bool VerifyVariableName(string name)
        {
            string lower = name.ToLowerInvariant();

            for (int i = 0; i < Keywords.Length; i++)
            {
                if (lower == Keywords[i])
                    return false;
            }

            return regVariableName.IsMatch(name);
        }

        public static bool VerifyName(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Notify("Name can't be empty.", BorderColour.Error);
                return false;
            }

            if (!regName.IsMatch(name))
            {
                Notify("Invalid name", BorderColour.Error);
                return false;
            }

            return true;
        }

        public static bool IsDisabled(VisualElement element)
        {
            return element.ClassListContains("Disable");
        }

        public static VisualElement CreateDefaultTab(string fileName)
        {
            VisualElement newContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/Tabs/{fileName}.uxml").Instantiate();
            return newContainer;
        }
    }
}
