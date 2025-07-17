using System;
using System.IO;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    public class PrefabReader
    {
        public Span<string> FindBlock(string path, params string[] names)
        {
            string text = File.ReadAllText(path);
            //var result = 


            return null;
        }
    }
}
