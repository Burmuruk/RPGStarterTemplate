using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class WeaponSetting : BaseItemSetting
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
            //bodyPart.Init(EquipmentType.None);

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

        public override object GetInfo(object args)
        {
            Weapon weapon = new Weapon();

            var currentBuffs = BuffAdder.GetBussData();
            var buffsCreated = (Dictionary<string, BuffData>)args;
            var buffsData = new List<BuffData>();

            foreach ((string buffName, BuffData? data) buff in currentBuffs)
            {
                if (buff.data.HasValue)
                {
                    buffsData.Add(buff.data.Value);
                }
                else
                {
                    foreach (var creation in buffsCreated)
                    {
                        if (creation.Key == buff.buffName)
                        {
                            buffsData.Add(creation.Value);
                            break;
                        }
                    }
                }
            }

            weapon.Populate(TxtName.text, 
                TxtDescription.text, 
                ItemType.Weapon, 
                (Sprite)OfSprite.value, 
                (Pickup)OfPickup.value, 
                Int32.Parse(UfCapacity.value.ToString()),
                (unchecked((int)Damage.value), 
                RateDamage.value, 
                MinDistance.value, 
                MaxDistance.value, 
                ReloadTime.value, 
                MaxAmmo.value,
                buffsData.ToArray()));

            //(m_damage, m_rateDamage, m_minDistance, m_maxDistance, reloadTime, maxAmmo, m_buffsData)
            //var values = ((int damage, float rateDamage, float minDistance, float maxDistance,
            //float reloadTime, int maxAmmo, BuffData[] data))args;

            return weapon;
        }
    }
}
