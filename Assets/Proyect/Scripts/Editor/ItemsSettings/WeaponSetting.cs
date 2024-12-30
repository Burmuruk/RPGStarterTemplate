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

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            Placement = container.Q<EnumField>("efBodyPart");
            Damage = container.Q<UnsignedIntegerField>("txtDamage");
            RateDamage = container.Q<FloatField>("txtRateDamage");
            MinDistance = container.Q<FloatField>("MinDistance");
            MaxDistance = container.Q<FloatField>("MaxDistance");
            ReloadTime = container.Q<FloatField>("txtReloadTime");
            MaxAmmo = container.Q<IntegerField>("txtMaxAmmo");

            var buffAdder = new BuffAdderUI(container);
        }
    }
}
