using Burmuruk.RPGStarterTemplate.Inventory;

namespace Burmuruk.RPGStarterTemplate.Combat.Samples
{
    public class FighterSample : Fighter
    {
        protected override void SetAttackTrigger(EquipableItem equipable)
        {
            if (animator == null)
                return;

            if (equipable is Weapon weapon)
            {
                string typeName = weapon.MinDistance switch
                {
                    < 5 => "CloseRange",
                    _ => "LongRange"
                };
                animator?.SetTrigger(typeName);

                string typeWeapon = weapon.Name.Contains("Bow") ? "CrossBow" : "Gun";
                animator?.SetTrigger(typeWeapon);

                string triggerName = (WeaponType)weapon.GetSubType() switch
                {
                    WeaponType.Gun => "GunAttack",
                    _ => "MeleeAttack"
                };
                animator?.SetTrigger(triggerName);
            }
            else if (equipable is null)
            {
                animator?.SetTrigger("UnarmedAttack");
            }
        }
    }
}
