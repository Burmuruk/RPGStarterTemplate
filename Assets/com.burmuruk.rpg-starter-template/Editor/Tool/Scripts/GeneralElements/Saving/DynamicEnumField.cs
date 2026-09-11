using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public class DynamicEnumField : IUIListContainer<EnumModificationData>, IClearable
    {
        private Type enumType;
        private List<EnumEntry> entries;
        EnumRegistry _registry;

        public VisualElement Container { get; private set; }
        public DropdownField DDField { get; private set; }
        public int SelectedId { get; private set; }
        public string Value { get => DDField.value; }

        public void Init(VisualElement container, Type enumType, int selectedId)
        {
            this.Container = container;
            DDField = container.Q<DropdownField>();
            this.enumType = enumType;
            this._registry = SavingSystem.LoadEnumRegistry();

            DDField.RegisterValueChangedCallback(OnValueChanged);
            RefreshChoices();
            SetValueWithoutNotify(selectedId);
            EnumScheduler.Add(ModificationTypes.Add, enumType, this);
            EnumScheduler.Add(ModificationTypes.EditData, enumType, this);
            EnumScheduler.Add(ModificationTypes.Rename, enumType, this);
            EnumScheduler.Add(ModificationTypes.Remove, enumType, this);
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            var entry = _registry.GetEntry(enumType, evt.newValue);

            SelectedId = entry.Id;
        }

        public void SetEnabled(bool enabled)
        {
            DDField.SetEnabled(enabled);
        }

        public void RefreshChoices()
        {
            entries = _registry
                .GetEntries(enumType)
                .OrderBy(x => x.Order)
                .ToList();

            DDField.choices = entries
                .Select(x => x.Name)
                .ToList();

            if (!_registry.Contains(enumType, Value))
            {
                SelectedId = EnumRegistry.NoneId;
            }

            SetValueWithoutNotify(SelectedId);
        }

        public void SetValueWithoutNotify(int id)
        {
            EnumEntry entry = entries.FirstOrDefault(x => x.Id == id);

            if (entry == null)
            {
                SelectedId = EnumRegistry.NoneId;

                entry = entries.FirstOrDefault(
                    x => x.Id == SelectedId);
            }
            else
            {
                SelectedId = id;
            }

            DDField.SetValueWithoutNotify(entry?.Name ?? "None");
        }

        public void SetValue(int id)
        {
            EnumEntry entry = entries.FirstOrDefault(x => x.Id == id);

            if (entry == null)
            {
                SelectedId = EnumRegistry.NoneId;

                entry = entries.FirstOrDefault(
                    x => x.Id == SelectedId);
            }
            else
            {
                SelectedId = id;
            }

            DDField.value = entry?.Name ?? "None";
        }

        public void Clear()
        {
            SetValueWithoutNotify(EnumRegistry.NoneId);
        }

        public virtual void AddData(in EnumModificationData newValue) =>
            RefreshChoices();

        public virtual void EditData(in EnumModificationData newValue) =>
            RefreshChoices();

        public virtual void RenameCreation(in EnumModificationData newValue) =>
            RefreshChoices();

        public virtual void RemoveData(in EnumModificationData newValue) =>
            RefreshChoices();
    }
}