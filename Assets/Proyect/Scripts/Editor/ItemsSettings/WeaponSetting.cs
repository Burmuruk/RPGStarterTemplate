using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class WeaponSetting : ItemBuffReader
    {
        private Weapon _changesWeapon;

        public EnumField Placement { get; private set; }
        public UnsignedIntegerField Damage { get; private set; }
        public FloatField RateDamage { get; private set; }
        public FloatField MinDistance { get; private set; }
        public FloatField MaxDistance { get; private set; }
        public FloatField ReloadTime { get; private set; }
        public IntegerField MaxAmmo { get; private set; }

        public EnumField EFBodyPart { get; private set; }
        public EnumModifierUI<WeaponType> EMWeaponType { get; private set; }
        
        public override void Initialize(VisualElement container, NameSettings name)
        {
            base.Initialize(container, name);

            Placement = container.Q<EnumField>("efBodyPart");
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
            Placement.Init(EquipmentType.None);

            var typeAdder = container.Q<VisualElement>("TypeAdderWeapon");
            EMWeaponType = new EnumModifierUI<WeaponType>(typeAdder);
            EMWeaponType.Name.text = "Weapon type";
            //var weaponType = container.Q<VisualElement>("TypeAdderWeapon");
            //weaponType.Q<Label>().text = "Weapon type";
            //weaponType.Q<EnumField>().Init(WeaponType.None);

            BuffAdder = new BuffAdderUI(container);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);

            var weapon = data as Weapon;
            var buffArgs = args as BuffsNamesDataArgs;

            if (weapon == null) return;

            Placement.SetValueWithoutNotify(weapon.BodyPart);
            Damage.value = (uint)weapon.Damage;
            RateDamage.value = weapon.ReloadTime;
            MinDistance.value = weapon.MinDistance;
            MaxDistance.value = weapon.MaxDistance;
            ReloadTime.value = weapon.ReloadTime;
            MaxAmmo.value = weapon.MaxAmmo;

            UpdateBuffs(weapon.BuffsData, buffArgs);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var createdBuffs = args as CreatedBuffsDataArgs;

            if (createdBuffs == null) return default;

            Weapon weapon = new Weapon();
            weapon.Copy(base.GetInfo(args).item);

            (var buffs, var buffsNames) = GetBuffsInfo();

            weapon.Populate(
                (EquipmentType)Placement.value,
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
            Placement.SetValueWithoutNotify(default);
            Damage.value = 0;
            RateDamage.value = 0;
            MinDistance.value = 0;
            MaxDistance.value = 0;
            ReloadTime.value = 0;
            MaxAmmo.value = 0;
            EFBodyPart.SetValueWithoutNotify(default);
            EMWeaponType.Clear();
            BuffAdder.Clear();
        }

        public override bool Check_Changes()
        {
            bool hasChanges = false;

            if (_nameControl.Check_Changes())
            {
                hasChanges = true;
            }

            if (_changesWeapon.BodyPart != (EquipmentType)Placement.value)
            {
                hasChanges = true;
                Highlight(Placement, true);
            }
            if (_changesWeapon.Damage != Damage.value)
            {
                hasChanges = true;
                Highlight(Damage, true);
            }
            if (_changesWeapon.DamageRate != RateDamage.value)
            {
                hasChanges = true;
                Highlight(RateDamage, true);
            }
            if (_changesWeapon.MinDistance != MinDistance.value)
            {
                hasChanges = true;
                Highlight(MinDistance, true);
            }
            if (_changesWeapon.MaxDistance != MaxDistance.value)
            {
                hasChanges = true;
                Highlight(MaxDistance, true);
            }
            if (_changesWeapon.ReloadTime != ReloadTime.value)
            {
                hasChanges = true;
                Highlight(ReloadTime, true);
            }
            if (_changesWeapon.MaxAmmo != MaxAmmo.value)
            {
                hasChanges = true;
                Highlight(MaxAmmo, true);
            }
            if (_changesWeapon.ReloadTime != ReloadTime.value)
            {
                hasChanges = true;
                Highlight(ReloadTime, true);
            }
            if (_changesWeapon.MaxAmmo != MaxAmmo.value)
            {
                hasChanges = true;
                Highlight(MaxAmmo, true);
            }
            //if (_changesWeapon.BodyPart != (EquipmentType)EFBodyPart.value)
            //{
            //    hasChanges = true;
            //    Highlight(EFBodyPart, true);
            //}
            if ((WeaponType)_changesWeapon.GetSubType() != (WeaponType)Damage.value)
            {
                hasChanges = true;
                Highlight(EMWeaponType.Name, true);
            }

            return hasChanges;
        }
    }
}
