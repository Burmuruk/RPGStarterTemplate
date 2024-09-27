using System;

namespace Burmuruk.Tesis.Stats
{
    [Serializable]
    public struct BuffData
    {
        public ModifiableStat stat;
        public float value;
        public float duration;
        public float rate;
        public bool percentage;
        public bool affectAll;
        public float probability;
    }
}
