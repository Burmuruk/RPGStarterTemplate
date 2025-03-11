using System;
using System.Collections.Generic;
using System.Linq;

namespace Burmuruk.Tesis.Editor
{
    internal class EnumScheduler
    {
        private static UIListScheduler<Enum> scheduler;

        public static void Add(Enum key, IEnumContainer container, Action callback)
        {
            scheduler.AddContainer(key, container, callback);
        }

        public static void ChangeData(Enum key, in string[] names)
        {
            scheduler.ChangeData(key, names);
        }
    }

    internal class UIListScheduler<T>
    {
        IDictionary<T, List<(IEnumContainer container, Action callback)>> modifiers;

        public void AddContainer(T key, IEnumContainer container, Action callback)
        {
            if (!modifiers.ContainsKey(key))
                modifiers.Add(key, new List<(IEnumContainer, Action)>());

            if (!(from m in modifiers[key]
                  where m.container == container
                  select m).Any())
            {
                modifiers[key].Add((container, callback));
            }
        }

        public void ChangeData(T key, in string[] names)
        {
            if (modifiers.ContainsKey(key))
            {
                modifiers[key].ForEach(m => m.callback?.Invoke());
            }
        }
    }

    internal interface IEnumContainer
    {
        public void OnDataChanged(Enum value, in string[] newValues);

    }
}
