using Burmuruk.RPGStarterTemplate.Inventory;
using Burmuruk.RPGStarterTemplate.Stats;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public class BaseItemSetting : BaseInfoTracker, ISaveable
    {
        protected string _id = null;
        protected InventoryItem _changes;
        protected ItemDataArgs _args = null;

        public TextField TxtDescription { get; private set; }
        public ObjectField OfSprite { get; private set; }
        public ObjectField OfPickup { get; private set; }
        public UnsignedIntegerField UfCapacity { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo nameControl)
        {
            base.Initialize(container, nameControl);

            TxtDescription = container.Q<TextField>("txtDescription");
            OfSprite = container.Q<ObjectField>("opSprite");
            OfPickup = container.Q<ObjectField>("opPickup");
            UfCapacity = container.Q<UnsignedIntegerField>("txtCapacity");

            OfSprite.objectType = typeof(Sprite);
            OfPickup.objectType = typeof(GameObject);
            _nameControl.TxtName.RegisterValueChangedCallback((evt) => 
            {
                if (IsActive)
                    TempName = evt.newValue;
            });
        }

        public virtual void UpdateInfo(InventoryItem data, ItemDataArgs args, ItemType type = ItemType.Consumable)
        {
            TempName = data.Name;
            _originalName = data.Name;
            UpdateName();
            TxtDescription.value = data.Description;
            OfSprite.value = data.Sprite;
            OfPickup.value = args?.GetPickupPrefab();
            UfCapacity.value = (uint)data.Capacity;

            _changes ??= new InventoryItem();
            _changes.UpdateInfo(data.Name, data.Description, type, (Sprite)OfSprite.value, null, unchecked((int)UfCapacity.value));
            _args = args;
        }

        public virtual (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var data = new InventoryItem();
            ItemDataArgs newArgs;

            if (OfPickup.value == null)
                newArgs = null;
            else
                newArgs = new ItemDataArgs(AssetDatabase.GetAssetPath(OfPickup.value as GameObject));

            data.UpdateInfo(
                _nameControl.TxtName.value,
                TxtDescription.value,
                ItemType.None,
                (Sprite)OfSprite.value,
                null,
                unchecked((int)UfCapacity.value)
                );

            return (data, newArgs);
        }

        public override void Clear()
        {
            TxtDescription.value = "";
            OfSprite.value = null;
            OfPickup.value = null;
            UfCapacity.value = 0;
            CurModificationType = ModificationTypes.None;
            _changes = null;
            _args = null;
            _id = null;
            base.Clear();
        }

        protected void ClearItemInfo()
        {
            _changes.UpdateInfo("", "", _changes.Type, null, null, 0);
        }

        public override bool VerifyData(out List<string> errors)
        {
            errors = new();
            bool result = true;

            result &= _nameControl.VerifyData(out errors);

            return result;
        }

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_changes == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;

                if (_nameControl.Check_Changes() != ModificationTypes.None)
                    CurModificationType = ModificationTypes.Rename;

                if (TxtDescription.value != _changes.Description)
                    CurModificationType = ModificationTypes.EditData;

                if (OfSprite.value != _changes.Sprite)
                    CurModificationType = ModificationTypes.EditData;

                if (OfPickup.value != _args?.GetPickupPrefab())
                    CurModificationType = ModificationTypes.EditData;

                if (UfCapacity.value != _changes.Capacity)
                    CurModificationType = ModificationTypes.EditData;

                return CurModificationType;
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public virtual bool Save()
        {
            if (!VerifyData(out var errors))
            {
                Utilities.UtilitiesUI.Notify(errors.Count > 1 ? "Invalid Data" : errors[0], BorderColour.Error);
                return false;
            }

            CurModificationType = Check_Changes();
            if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
            {
                Utilities.UtilitiesUI.Notify("No changes were found", BorderColour.HighlightBorder);
                return false;
            }
            else
                CurModificationType = ModificationTypes.Add;

            Utilities.UtilitiesUI.DisableNotification();
            var (data, args) = GetInfo(null);
            var creationData = new ItemCreationData(_nameControl.TxtName.value, data, args);

            return SavingSystem.SaveCreation(ElementType.Item, in _id, creationData, CurModificationType);
        }

        public virtual CreationData Load(ElementType type, string id)
        {
            var result = SavingSystem.Load(type, id);

            if (result == null) return null;
            
            _id = id;
            var item = (result as ItemCreationData);
            Set_CreationState(CreationsState.Editing);
            UpdateInfo(item.Data, item.args);

            return result;
        }

        public override void Load_Changes()
        {
            TempName = _changes.name;
            UpdateName();
            TxtDescription.value = _changes.Description;
            OfSprite.value = _changes.Sprite;
            OfPickup.value = _args?.GetPickupPrefab();
            UfCapacity.value = (uint)_changes.Capacity;
            CurModificationType = ModificationTypes.None;
        }

        public override void Remove_Changes()
        {
            _changes = null;
            _args = null;
            _id = null;
        }
    }

    public interface IClearable
    {
        public abstract void Clear();
    }

    public record ItemDataArgs 
    {
        public string pickupPath;

        public ItemDataArgs(string pickupPath)
        {
            this.pickupPath = pickupPath;
        }

        public GameObject GetPickupPrefab()
        {
            if (string.IsNullOrEmpty(pickupPath)) return null;
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(pickupPath);
        }
    }
}
