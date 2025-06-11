﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Burmuruk.Tesis.Editor.Controls
{
    public static class BasicStatsEditor
    {
        public struct VariableEntry
        {
            public string Name;
            public string Type;
            public string Header;
        }

        public static string AddVariables(string scriptText, List<VariableEntry> newVariables)
        {
            if (newVariables.Count <= 0) return scriptText;

            foreach (var group in newVariables.GroupBy(v => v.Header))
            {
                string headerPattern = $@"(?s)\[.*?Header\s*?\(.*?{Regex.Escape(group.Key)}.*?\).*?\](?-s)(\s*?\[SerializeField\].*?;)+";
                Match headerMatch = Regex.Match(scriptText, headerPattern);
                string val = headerMatch.Groups[1].Value;
                string newFields = string.Join("\r\n        ", group.Select(v => $"[SerializeField] public {v.Type.Replace('_', '.')} {v.Name};"));

                if (headerMatch.Success)
                {
                    scriptText = scriptText.Replace(val, val + "\r\n        " + newFields);
                }
                else
                {
                    string insertion = $"\r\n        [Space(), Header(\"{group.Key}\")]\r\n        {newFields}\r\n\r\n";
                    int insertIndex = scriptText.LastIndexOf("[Serializable]");
                    if (insertIndex == -1)
                        insertIndex = scriptText.LastIndexOf("}");
                    scriptText = scriptText.Insert(insertIndex, insertion);
                }
            }
            return scriptText;
        }

        public static string RenameVariable(string scriptText, string oldName, string newName)
        {
            return Regex.Replace(scriptText, $@"\b{Regex.Escape(oldName)}\b", newName);
        }

        public static string RemoveVariables(string scriptText, List<string> variableNames)
        {
            //if (variableNames.Count <= 0) return scriptText;

            var lines = scriptText.Split("\r\n");
            var result = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith("[SerializeField]") && i + 1 < lines.Length)
                {
                    string nextLine = lines[i + 1].Trim();
                    bool toRemove = variableNames.Any(name => nextLine.Contains($" {name};"));
                    if (toRemove)
                    {
                        i++; // Skip next line too
                        continue;
                    }
                }
                else if (variableNames.Any(name => trimmed.Contains($" {name};")))
                {
                    continue;
                }
                result.Add(lines[i]);
            }
            return string.Join("\r\n", result);
        }
    }
}
