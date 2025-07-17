using Burmuruk.RPGStarterTemplate.Combat;
using Burmuruk.RPGStarterTemplate.Inventory;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public class WeaponSetting : ItemBuffReader
    {
        public UnsignedIntegerField Damage { get; private set; }
        public FloatField RateDamage { get; private set; }
        public FloatField MinDistance { get; private set; }
        public FloatField MaxDistance { get; private set; }
        public FloatField ReloadTime { get; private set; }
        public IntegerField MaxAmmo { get; private set; }

        public EnumField EFBodyPart { get; private set; }
        public EnumModifierUI<WeaponType> EMWeaponType { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            base.Initialize(container, name);

            Damage = container.Q<UnsignedIntegerField>("txtDamage");
            RateDamage = container.Q<FloatField>("txtRateDamage");
            MinDistance = container.Q<FloatField>("MinDistance");
            MaxDistance = container.Q<FloatField>("MaxDistance");
            ReloadTime = container.Q<FloatField>("txtReloadTime");
            MaxAmmo = container.Q<IntegerField>("txtMaxAmmo");

            EFBodyPart = container.Q<EnumField>("efBodyPart");
            EFBodyPart.Init(EquipmentType.None);
            //var bodyPart = container.Q<EnumField>("efBodyPart");
            //bodyPart.Init(EquipmentPlace.None);

            var typeAdder = container.Q<VisualElement>("TypeAdderWeapon");
            EMWeaponType = new EnumModifierUI<WeaponType>(typeAdder);
            EMWeaponType.Name.text = "Weapon type";
            //var weaponType = container.Q<VisualElement>("TypeAdderWeapon");
            //weaponType.Q<Label>().text = "Weapon type";
            //weaponType.Q<EnumField>().Init(WeaponType.None);

            BuffAdder = new BuffAdderUI(container);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args, ItemType type = ItemType.Weapon)
        {
            _changes = new Weapon();
            base.UpdateInfo(data, args, type);

            var weapon = data as Weapon;
            var buffArgs = args as BuffsNamesDataArgs;

            if (weapon == null) return;

            EFBodyPart.value = weapon.BodyPart;
            Damage.value = (uint)weapon.Damage;
            RateDamage.value = weapon.ReloadTime;
            MinDistance.value = weapon.MinDistance;
            MaxDistance.value = weapon.MaxDistance;
            ReloadTime.value = weapon.ReloadTime;
            MaxAmmo.value = weapon.MaxAmmo;
            EMWeaponType.Value = (WeaponType)weapon.GetSubType();

            (_changes as Weapon).UpdateInfo(
                weapon.BodyPart, EMWeaponType.Value, weapon.Damage, weapon.DamageRate,
                weapon.MinDistance, weapon.MaxDistance, weapon.ReloadTime, weapon.MaxAmmo, weapon.Buffs);

            UpdateBuffs(weapon.Buffs, buffArgs);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            //var createdBuffs = args as CreatedBuffsDataArgs;

            //if (createdBuffs == null) return default;

            Weapon weapon = new Weapon();
            weapon.Copy(base.GetInfo(args).item);

            (var buffs, var buffsNames) = GetBuffsInfo();

            weapon.UpdateInfo(
                (EquipmentType)EFBodyPart.value,
                EMWeaponType.Value,
                (int)unchecked(Damage.value),
                RateDamage.value,
                MinDistance.value,
                MaxDistance.value,
                ReloadTime.value,
                MaxAmmo.value,
                buffs.ToArray()
                );

            return (weapon, buffsNames);
        }

        public override void Clear()
        {
            base.Clear();
            Damage.value = 0;
            RateDamage.value = 0;
            MinDistance.value = 0;
            MaxDistance.value = 0;
            ReloadTime.value = 0;
            MaxAmmo.value = 0;
            EFBodyPart.value = EquipmentType.None;
            EMWeaponType.Clear();
            BuffAdder.Clear();

            _changes = null;
        }

        public override void Remove_Changes()
        {
            base.Remove_Changes();
            BuffAdder.Remove_Changes();
        }

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_changes == null) return CurModificationType = ModificationTypes.Add;

                base.Check_Changes();
                var _changesWeapon = _changes as Weapon;

                if (_changesWeapon.Damage != Damage.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(Damage, true);
                }
                if (_changesWeapon.DamageRate != RateDamage.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(RateDamage, true);
                }
                if (_changesWeapon.MinDistance != MinDistance.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(MinDistance, true);
                }
                if (_changesWeapon.MaxDistance != MaxDistance.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(MaxDistance, true);
                }
                if (_changesWeapon.ReloadTime != ReloadTime.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(ReloadTime, true);
                }
                if (_changesWeapon.MaxAmmo != MaxAmmo.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(MaxAmmo, true);
                }
                if (_changesWeapon.ReloadTime != ReloadTime.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(ReloadTime, true);
                }
                if (_changesWeapon.MaxAmmo != MaxAmmo.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(MaxAmmo, true);
                }
                if (_changesWeapon.BodyPart != (EquipmentType)EFBodyPart.value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(EFBodyPart, true);
                }
                if ((WeaponType)_changesWeapon.GetSubType() != EMWeaponType.Value)
                {
                    CurModificationType = ModificationTypes.EditData;
                    Highlight(EMWeaponType.Name, true);
                }

                return CurModificationType;
            }
            catch (InvalidDataExeption e)
            {
                throw e; 
            }
        }

        public override bool Save()
        {
            try
            {
                if (!VerifyData())
                    throw new InvalidDataExeption("Invalid Data");

                if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
                {
                    Notify("No changes were found", BorderColour.HighlightBorder);
                    return false;
                }
                else
                    CurModificationType = ModificationTypes.Add;

                DisableNotification();
                var (item, args) = GetBuffsIds();
                var creationData = new BuffUserCreationData(_nameControl.TxtName.text.Trim(), item, args);

                return SavingSystem.SaveCreation(ElementType.Weapon, in _id, creationData, CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override CreationData Load(ElementType type, string id)
        {
            var result = SavingSystem.Load(type, id);

            if (result == null) return null;
            
            var weapon = result as BuffUserCreationData;
            _id = id;
            (var item, var args) = (weapon.Data, weapon.Names);
            Set_CreationState(CreationsState.Editing);
            UpdateInfo(item, args);

            return weapon;
        }

        public override void Load_Changes()
        {
            base.Load_Changes();

            var changes = _changes as Weapon;

            Damage.value = unchecked((uint)changes.Damage);
            RateDamage.value = changes.DamageRate;
            MinDistance.value = changes.MinDistance;
            MaxDistance.value = changes.MaxDistance;
            ReloadTime.value = changes.ReloadTime;
            MaxAmmo.value = changes.MaxAmmo;
            EFBodyPart.value = changes.BodyPart;
            EMWeaponType.Value = (WeaponType)changes.GetSubType();
            BuffAdder.Load_Changes();
        }
    }
}
