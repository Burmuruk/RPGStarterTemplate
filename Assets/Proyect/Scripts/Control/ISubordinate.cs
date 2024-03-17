using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public interface ISubordinate
    {
        Character[] Fellows { get; set; }
        float FellowGap { get; }
        public Formation Formation { get; }
        public void SetFormation(Vector2 formation);
    }

    public enum Formation
    {
        None,
        Follow,
        LockTarget,
        Free,
        Protect
    }
}
