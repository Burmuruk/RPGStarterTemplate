using System;
using System.Collections.Generic;
using System.Linq;

namespace Burmuruk.Tesis.Editor
{
    internal static class EnumScheduler
    {
        private static UIListScheduler<Enum, EnumModificationData> scheduler;

        public static void Add(ModificationType modificationType, Enum key, IUIListContainer<EnumModificationData> container)
        {
            scheduler ??= new();
            scheduler.AddContainer(modificationType, key, container);
        }

        public static void ChangeData(ModificationType modificationType, Enum key)
        {
            scheduler.ChangeData(modificationType, key, default);
        }
    }

    internal static class CreationScheduler
    {
        public static Func<ElementType, Dictionary<string, string>> creationsNames;
        private static UIListScheduler<ElementType, BaseCreationInfo> scheduler;

        public static void Add(ModificationType modificationType, ElementType key, IUIListContainer<BaseCreationInfo> container)
        {
            scheduler ??= new();
            scheduler.AddContainer(modificationType, key, container);
        }

        public static void ChangeData(ModificationType modificationType, ElementType key, string id, BaseCreationInfo data)
        {
            var names = GetNames(key);

            scheduler.ChangeData(modificationType, key, in data);
        }

        public static Dictionary<string, string> GetNames(ElementType type)
        {
            return creationsNames(type);
        }
    }

    internal class UIListScheduler<T, U> where U : struct
    {
        Dictionary<ModificationType, Dictionary<T, List<IUIListContainer<U>>>> modifiers = new()
        {
            { ModificationType.Add, new() },
            { ModificationType.Remove, new() },
            { ModificationType.EditData, new() },
            { ModificationType.Rename, new() },
        };

        public void AddContainer(ModificationType modificationType, T key, IUIListContainer<U> container)
        {
            if (!modifiers[modificationType].ContainsKey(key))
                modifiers[modificationType].Add(key, new List<IUIListContainer<U>>());

            if (!(from c in modifiers[modificationType][key]
                  where c == container
                  select c).Any())
            {
                modifiers[modificationType][key].Add(container);
            }
        }

        public void ChangeData(ModificationType modificationType, T key, in U data)
        {
            if (!modifiers[modificationType].ContainsKey(key))
                return;

            if ((modificationType & ModificationType.Add) != 0)
            {
                foreach (var modifier in modifiers[modificationType][key])
                {
                    modifier.AddData(in data);
                }
            }
            if ((modificationType & ModificationType.Remove) != 0)
            {
                foreach (var modifier in modifiers[modificationType][key])
                {
                    modifier.RemoveData(in data);
                }
            }
            if ((modificationType & ModificationType.EditData) != 0)
            {
                foreach (var modifier in modifiers[modificationType][key])
                {
                    modifier.EditData(in data);
                }
            }
            if ((modificationType & ModificationType.Rename) != 0)
            {
                foreach (var modifier in modifiers[modificationType][key])
                {
                    modifier.RenameCreation(in data);
                }
            }
        }
    }

    internal interface IUIListContainer<U> where U : struct
    {
        public virtual void AddData(in U newValue) { }

        public virtual void RenameCreation(in U newValue) { }

        public virtual void RemoveData(in U newValue) { }

        public virtual void EditData(in U newValue) { }
    }
}
