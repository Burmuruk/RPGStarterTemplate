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
            bool enumStarted = false;
            bool endOfEnum = false;
            bool elementAdded = false;
            string lastLine = "";

            if (!RewriteFile(enumName, value, filePath, lines, ref containsEnum, ref enumStarted, ref endOfEnum, ref elementAdded, lastLine))
                return false;

            if (containsEnum && elementAdded)
            {
                AssetDatabase.Refresh();
                RecompileScripts();
            }

            return containsEnum && elementAdded;

            bool RewriteFile(string enumName, string value, string filePath, string[] lines, ref bool containsEnum, ref bool enumStarted, ref bool endOfEnum, ref bool elementAdded, string lastLine)
            {
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
