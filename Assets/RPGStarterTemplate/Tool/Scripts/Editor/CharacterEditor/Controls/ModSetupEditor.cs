using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ModSetupEditor
    {
        private const string MethodName = "SetUpMods";

        public static List<ModEntry> ExtractAllMods(string scriptText)
        {
            var mods = new List<ModEntry>();

            var methodMatch = Regex.Match(scriptText, $@"(?<=void\s+{MethodName}\s*\(\)\s*\{{)(.*?)(?=\}}[^\)])", RegexOptions.Singleline);
            if (!methodMatch.Success) return mods;

            var methodBody = methodMatch.Groups[1].Value;

            var matches = Regex.Matches(methodBody,
                @"ModsList\.AddVariable\(\(Character\)this,\s*ModifiableStat\.([a-zA-Z0-9_]+),\s*\(\)\s*=>\s*(.*?),\s*\(value",
                RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count >= 3)
                {
                    mods.Add(new ModEntry
                    {
                        ModifiableStat = match.Groups[1].Value.Trim(),
                        VariableName = match.Groups[2].Value.Trim().Split('.').Last().Replace(";", "")
                    });
                }
            }

            return mods;
        }

        public static string AddMods(string scriptText, List<ModEntry> newMods)
        {
            if (newMods.Count == 0) return scriptText;

            var methodMatch = Regex.Match(scriptText, $@"(void\s+{MethodName}\s*\(\)\s*\{{)(.*?)(?=\}}[^\)])", RegexOptions.Singleline);
            if (!methodMatch.Success) return scriptText;

            var methodStart = methodMatch.Groups[1].Value;
            var methodBody = methodMatch.Groups[2].Value.TrimEnd();
            var methodEnd = methodMatch.Groups[3].Value;

            foreach (var entry in newMods)
            {
                var newLine = $"    ModsList.AddVariable((Character)this, ModifiableStat.{entry.ModifiableStat}, () => {entry.VariableName}, value => {{ {entry.VariableName} = value; }});";
                if (!methodBody.Contains(entry.VariableName))
                    methodBody += "\n" + newLine;
            }

            return scriptText.Replace(methodMatch.Value, methodStart + "\n" + methodBody + "\n" + methodEnd);
        }

        public static string RemoveMods(string scriptText, List<string> variableNames)
        {
            if (variableNames.Count == 0) return scriptText;

            var methodMatch = Regex.Match(scriptText, $@"(void\s+{MethodName}\s*\(\)\s*\{{)(.*?)(?=\}}[^\)])", RegexOptions.Singleline);
            if (!methodMatch.Success) return scriptText;

            var methodStart = methodMatch.Groups[1].Value;
            var methodBody = methodMatch.Groups[2].Value;
            var methodEnd = methodMatch.Groups[3].Value;

            var lines = methodBody.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    filteredLines.Add(line);
                    continue;
                }

                bool containsAny = false;
                foreach (var variable in variableNames)
                {
                    if (Regex.IsMatch(line, $@"\b{Regex.Escape(variable)}\b"))
                    {
                        containsAny = true;
                        break;
                    }
                }

                if (!containsAny)
                    filteredLines.Add(line);
            }

            return scriptText.Replace(methodMatch.Value, methodStart + "\n" + string.Join("\n", filteredLines) + "\n" + methodEnd);
        }

        public static string ApplyModChanges(string scriptText, List<ModChange> changes)
        {
            if (changes.Count == 0) return scriptText;

            var methodMatch = Regex.Match(scriptText, $@"(void\s+{MethodName}\s*\(\)\s*\{{)(.*?)(?=\}}[^\)])", RegexOptions.Singleline);
            if (!methodMatch.Success) return scriptText;

            var methodStart = methodMatch.Groups[1].Value;
            var methodBody = methodMatch.Groups[2].Value;
            var methodEnd = methodMatch.Groups[3].Value;

            var lines = methodBody.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
            var updatedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("//"))
                {
                    updatedLines.Add(line);
                    continue;
                }

                bool modified = false;
                foreach (var change in changes)
                {
                    if (change.Type.ToString() == "None")
                    {
                        if (Regex.IsMatch(line, $@"\b{Regex.Escape(change.OldName)}\b"))
                        {
                            modified = true; // remove line
                            break;
                        }
                    }
                    else if (Regex.IsMatch(line, $@"\b{Regex.Escape(change.OldName)}\b"))
                    {
                        var updatedLine = line;
                        if (!string.IsNullOrEmpty(change.NewName))
                        {
                            updatedLine = Regex.Replace(updatedLine, $@"\b{Regex.Escape(change.OldName)}\b", change.NewName);
                        }
                        updatedLine = Regex.Replace(updatedLine, @"ModifiableStat\.[a-zA-Z0-9_]+", $"ModifiableStat.{change.Type}");
                        updatedLines.Add(updatedLine);
                        modified = true;
                        break;
                    }
                }

                if (!modified)
                    updatedLines.Add(line);
            }

            var updatedBody = string.Join("\n", updatedLines);
            return scriptText.Replace(methodMatch.Value, methodStart + "\n" + updatedBody + "\n" + methodEnd);
        }
    }

    public struct ModEntry
    {
        public string VariableName;
        public string ModifiableStat;

        public override string ToString() => $"{VariableName} => {ModifiableStat}";
    }

    public struct ModChange
    {
        public string Header;
        public string OldName;
        public string NewName;
        public ModifiableStat Type;
        public VariableType VariableType;
    }
}
