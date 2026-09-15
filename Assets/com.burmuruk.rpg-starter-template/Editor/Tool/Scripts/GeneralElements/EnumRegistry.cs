using Burmuruk.RPGStarterTemplate.Editor.Saving;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    [Serializable]
    public record EnumEntry
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public int Order { get; init; }

        public EnumEntry(int id, string name, int order)
        {
            this.Id = id;
            this.Name = name;
            this.Order = order;
        }
    }

    [Serializable]
    public class EnumDefinition
    {
        public string enumType;
        public int nextId;
        public bool hasChanges;
        public List<EnumEntry> entries = new();

        public EnumDefinition(string enumType)
        {
            this.enumType = enumType;
        }
    }

    [Serializable]
    public class EnumRegistry
    {
        private readonly EnumEditor enumEditor = new();
        public const int NoneId = 0;
        public bool waitingForCompilation;
        public List<EnumDefinition> definitions = new();

        public bool HasChanges => definitions.Any(x => x.hasChanges);

        public IReadOnlyList<EnumEntry> GetEntries<T>() where T : Enum
        {
            return GetEntries(typeof(T));
        }

        public IReadOnlyList<EnumEntry> GetEntries(Type enumType)
        {
            EnumDefinition definition = GetDefinition(enumType);

            if (definition == null)
                return GetOriginalEntries(enumType);

            return definition.entries.OrderBy(x => x.Order).ToList();
        }

        public EnumEntry GetEntry(Type enumType, int id)
        {
            return GetEntries(enumType).FirstOrDefault(x => x.Id == id);
        }

        public EnumEntry GetEntry(Type enumType, string name)
        {
            return GetEntries(enumType).FirstOrDefault(x => x.Name == name);
        }

        public string GetName<T>(int id) where T : Enum
        {
            return GetEntry(typeof(T), id)?.Name;
        }

        public int GetId<T>(string name) where T : Enum
        {
            return GetEntries<T>().FirstOrDefault(x => x.Name == name)?.Id ?? NoneId;
        }

        public bool Contains<T>(int id) where T : Enum
        {
            return GetEntries<T>().Any(x => x.Id == id);
        }

        public bool Contains(Type enumType, string name)
        {
            return GetEntries(enumType).Any(x => x.Name == name);
        }

        public EnumEntry Add(Type enumType, string name)
        {
            EnumDefinition definition = GetOrCreateDefinition(enumType);

            ValidateName(definition, name);

            EnumEntry entry = new(definition.nextId++, name, definition.entries.Count);

            definition.entries.Add(entry);
            definition.hasChanges = true;

            Notify(enumType, ModificationTypes.Add);

            return entry;
        }

        public void Rename(Type enumType, int id, string newName)
        {
            EnumDefinition definition = GetOrCreateDefinition(enumType);

            var idx = definition.entries.FindIndex(x => x.Id == id);

            if (idx < 0)
                throw new ArgumentException($"Enum id {id} does not exist.");

            ValidateName(definition, newName, id);

            definition.entries[idx] = definition.entries[idx] with { Name = newName };
            definition.hasChanges = true;

            Notify(enumType, ModificationTypes.Rename);
        }

        public void Remove(Type enumType, int id)
        {
            if (id == NoneId)
                throw new InvalidOperationException("None can't be removed.");

            EnumDefinition definition = GetOrCreateDefinition(enumType);

            EnumEntry entry = definition.entries.FirstOrDefault(x => x.Id == id);

            if (entry == null)
                return;

            definition.entries.Remove(entry);
            definition.hasChanges = true;

            UpdateOrders(definition);

            Notify(enumType, ModificationTypes.Remove);
        }

        public void Move(Type enumType, int id, int newIndex)
        {
            EnumDefinition definition = GetOrCreateDefinition(enumType);

            List<EnumEntry> ordered = definition.entries
                    .OrderBy(x => x.Order)
                    .ToList();

            int oldIndex = ordered.FindIndex(x => x.Id == id);

            if (oldIndex < 0)
                return;

            newIndex = Math.Clamp(newIndex, 0, ordered.Count - 1);

            EnumEntry entry = ordered[oldIndex];

            ordered.RemoveAt(oldIndex);
            ordered.Insert(newIndex, entry);

            definition.entries = ordered;
            definition.hasChanges = true;

            UpdateOrders(definition);

            Notify(enumType, ModificationTypes.EditData);
        }

        public void Reset(Type enumType)
        {
            EnumDefinition definition = GetDefinition(enumType);

            if (definition == null)
                return;

            definitions.Remove(definition);
            definition.hasChanges = true;

            Notify(enumType, ModificationTypes.EditData);
        }

        private EnumDefinition GetDefinition(Type enumType)
        {
            string typeName = enumType.AssemblyQualifiedName;

            return definitions.FirstOrDefault(x => x.enumType == typeName);
        }

        private EnumDefinition GetOrCreateDefinition(Type enumType)
        {
            EnumDefinition definition = GetDefinition(enumType);

            if (definition != null)
                return definition;

            definition = new EnumDefinition(enumType.AssemblyQualifiedName);
            definition.entries = GetOriginalEntries(enumType);
            definition.nextId = definition.entries.Count == 0 ? 0 : definition.entries.Max(x => x.Id) + 1;
            definitions.Add(definition);

            return definition;
        }

        private static List<EnumEntry> GetOriginalEntries(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"{enumType.Name} isn't an enum.");

            List<EnumEntry> entries = new();
            Array values = Enum.GetValues(enumType);
            int order = 0;

            foreach (object value in values)
            {
                entries.Add(new EnumEntry(Convert.ToInt32(value), Enum.GetName(enumType, value), order++));
            }

            return entries;
        }

        private static void ValidateName(EnumDefinition definition, string name, int ignoredId = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Enum name can't be empty.");

            bool duplicated = definition.entries.Any(x =>
                    x.Id != ignoredId &&
                    string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new ArgumentException($"Enum value '{name}' already exists.");
        }

        private static void UpdateOrders(EnumDefinition definition)
        {
            for (int i = 0; i < definition.entries.Count; i++)
            {
                definition.entries[i] = definition.entries[i] with { Order = i };
                //definition.entries[i].Order = i;
            }
        }

        private void Notify(Type enumType, ModificationTypes modificationType)
        {
            EnumScheduler.ChangeData(modificationType, enumType);

            SavingSystem.SaveEnumRegistry(this);
        }

        #region Apply
        public bool ApplyEnums()
        {
            bool result = false;

            foreach (EnumDefinition definition in definitions)
            {
                UnityEngine.Debug.Log(
                    $"Enum: {definition.enumType}, " +
                    $"hasChanges: {definition.hasChanges}, " +
                    $"opciones: {string.Join(", ", definition.entries.Select(e => e.Name))}");

                if (!definition.hasChanges)
                    continue;

                Type type = Type.GetType(definition.enumType);

                if (type == null)
                    throw new InvalidOperationException(
                        $"No se pudo resolver el enum: {definition.enumType}");

                string path = FindEnumPath(type);

                if (string.IsNullOrEmpty(path))
                    throw new InvalidOperationException(
                        $"No se encontró el script del enum: {type.FullName}");

                UnityEngine.Debug.Log($"Escribiendo {type.FullName} en: {path}");

                if (!enumEditor.SetValues(type.Name, path, definition.entries))
                {
                    throw new InvalidOperationException(
                        $"No se pudo actualizar el enum {type.FullName} en {path}");
                }

                result = true;
            }

            return result;
        }

        private string FindEnumPath(Type type)
        {
            string pattern = $@"\benum\s+{Regex.Escape(type.Name)}\b";
            var matches = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Script"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                    || !File.Exists(path))
                    continue;

                string content = File.ReadAllText(path);

                if (Regex.IsMatch(content, pattern))
                    matches.Add(path);
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Hay varias declaraciones candidatas para {type.FullName}:\n" +
                    string.Join("\n", matches));
            }

            return matches.Count == 1 ? matches[0] : null;
        }
        #endregion
    }
}