using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public enum Formation
    {
        None,
        Follow,
        LockTarget,
        Free,
        Protect
    }

    public interface ISubordinate
    {
        Character[] Fellows { get; set; }
        float FellowGap { get; }
        public Formation Formation { get; }
        public void SetFormation(Formation formation, object args);
    }
}
