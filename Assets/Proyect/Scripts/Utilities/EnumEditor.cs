using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Burmuruk.Tesis.Utilities
{
    public class EnumEditor
    {
        public bool AddEnumValue(string enumName, string value, string filePath)
        {
            if (Application.isPlaying) return false;

            if (HasSpecialCharacter(enumName) || HasSpecialCharacter(value))
                return false;

            var lines = File.ReadAllLines(filePath);

            bool containsEnum = false;
            bool elementAdded = false;

            if (!RewriteFile())
                return false;

            if (containsEnum && elementAdded)
            {
                AssetDatabase.Refresh();
                RecompileScripts();
            }

            return containsEnum && elementAdded;

            bool RewriteFile()
            {
                bool enumStarted = false;
                bool endOfEnum = false;
                string lastLine = "";

                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var line in lines)
                    {
                        if (line.Contains("{"))
                        {
                            if (line.Contains("}") || line.Contains(",")) return false;

                            enumStarted = true;
                        }
                        else if (line.Contains("}"))
                        {
                            endOfEnum = true;
                            elementAdded = true;

                            writer.WriteLine($"     {value},");
                        }
                        else if (enumStarted && !lastLine.Contains(",") && !endOfEnum)
                        {
                            writer.WriteLine(line + ",");
                            lastLine = line;
                            continue;
                        }

                        writer.WriteLine(line);
                        lastLine = line;

                        if (line.Contains($"enum {enumName}"))
                            containsEnum = true;
                    }
                }

                return true;
            }
        }

        public bool Modify(string enumName, string[] values, string filePath)
        {
            if (Application.isPlaying) return false;

            if (HasSpecialCharacter(enumName) || values.Any(chr => HasSpecialCharacter(chr)))
                return false;

            var lines = File.ReadAllLines(filePath);

            bool containsEnum = false;
            bool elementAdded = false;

            if (!RewriteFile())
                return false;

            if (containsEnum && elementAdded)
            {
                AssetDatabase.Refresh();
                RecompileScripts();
            }

            return containsEnum && elementAdded;

            bool RewriteFile()
            {
                bool inValues = false;

                using (var writer = new StreamWriter(filePath))
                {
                    int i = 0;

                    foreach (var line in lines)
                    {
                        if (line.Contains("{"))
                        {
                            if (line.Contains("}") || line.Contains(",")) return false;

                            inValues = true;
                        }
                        else if (line.Contains("}"))
                        {
                            inValues = false;
                        }
                        else if (inValues)
                        {
                            writer.WriteLine(values[i] + ",");
                            ++i;
                            continue;
                        }

                        writer.WriteLine(line);

                        if (line.Contains($"enum {enumName}"))
                            containsEnum = true;
                    }
                }

                return true;
            }
        }

        private void RecompileScripts()
        {
            CompilationPipeline.RequestScriptCompilation();
        }

        private bool HasSpecialCharacter(string value)
        {
            return value.Any(chr => !char.IsLetterOrDigit(chr));
        }
    }
}
