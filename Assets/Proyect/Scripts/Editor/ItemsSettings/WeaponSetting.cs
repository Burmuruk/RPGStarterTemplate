using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class WeaponSetting : BaseItemSetting, IEnumContainer
    {
        public EnumField Placement { get; private set; }
        public UnsignedIntegerField Damage { get; private set; }
        public FloatField RateDamage { get; private set; }
        public FloatField MinDistance { get; private set; }
        public FloatField MaxDistance { get; private set; }
        public FloatField ReloadTime { get; private set; }
        public IntegerField MaxAmmo { get; private set; }

        public EnumField EFBodyPart { get; private set; }
        public EnumModifierUI EMWeaponType { get; private set; }
        public BuffAdderUI BuffAdder { get; private set; }

        public override void Initialize(VisualElement container, TextField name)
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
            EMWeaponType = new EnumModifierUI(typeAdder, null, WeaponType.None);
            EMWeaponType.Name.text = "Weapon type";
            //var weaponType = container.Q<VisualElement>("TypeAdderWeapon");
            //weaponType.Q<Label>().text = "Weapon type";
            //weaponType.Q<EnumField>().Init(WeaponType.None);

            BuffAdder = new BuffAdderUI(container);
        }

        public void SetBuffs(List<string> buffTypes)
        {
            BuffAdder.SetBuffs(buffTypes);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);

            var weapon = data as Weapon;

            if (weapon == null) return;

            Placement.SetValueWithoutNotify(weapon.BodyPart);
            Damage.value = (uint)weapon.Damage;
            RateDamage.value = weapon.ReloadTime;
            MinDistance.value = weapon.MinDistance;
            MaxDistance.value = weapon.MaxDistance;
            ReloadTime.value = weapon.ReloadTime;
            MaxAmmo.value = weapon.MaxAmmo;

            UpdateBuffs(weapon, args as BuffsNamesDataArgs);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var createdBuffs = args as CreatedBuffsDataArgs;

            if (createdBuffs == null) return default;

            Weapon weapon = new Weapon();
            weapon.Copy(base.GetInfo(args).item);

            var curBuffs = (from buff in BuffAdder.GetBuffsData()
                            where !string.IsNullOrEmpty(buff.Name)
                            select buff).ToList();
            var buffs = RemoveRegisteredBuffs(curBuffs);

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

            return (weapon, new BuffsNamesDataArgs((from n in curBuffs select n.Name).ToList()));
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

        private void UpdateBuffs(Weapon weapon, BuffsNamesDataArgs buffArgs)
        {
            if (buffArgs == null) return;

            List<(string, BuffData?)> buffsData = new();
            int i = 0;
            int consumableIdx = 0;

            foreach (var name in buffArgs.BuffsNames)
            {
                if (name is null || name == BuffAdderUI.INVALIDNAME)
                {
                    buffsData.Add((BuffAdderUI.INVALIDNAME, weapon.BuffsData[consumableIdx++]));
                }
                else
                {
                    buffsData.Add((name, null));
                }

                ++i;
            }

            BuffAdder.UpdateData(buffsData);
        }

        private List<BuffData> RemoveRegisteredBuffs(List<NamedBuff> localBuffs)
        {
            List<BuffData> buffsData = new();

            foreach (NamedBuff curLocalBuff in localBuffs)
            {
                //if (curLocalBuff.Name != BuffAdderUI.INVALIDNAME)
                //{
                //    if (registeredBuffs != null)
                //    {
                //        foreach (var registeredBuff in registeredBuffs)
                //        {
                //            if (registeredBuff.Name == curLocalBuff.Name)
                //            {
                //                buffsData.Add(default);
                //                break;
                //            }
                //        }
                //    }

                //    continue;
                //}

                if (curLocalBuff.Name == BuffAdderUI.INVALIDNAME)
                    buffsData.Add(curLocalBuff.Data.Value);
            }

            return buffsData;
        }

        public void OnDataChanged(Enum value, in string[] newValues)
        {
            if (value is WeaponType)
            {

            }
            else if (value is EquipmentType)
            {

            }
        }
    }
}
