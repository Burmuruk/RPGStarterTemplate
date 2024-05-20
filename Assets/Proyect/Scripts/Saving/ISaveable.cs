using System.Collections;
using UnityEngine;

namespace Burmuruk.Tesis.Saving
{
    public interface ISaveable
    {
        int ID { get; }
        object CaptureState();
        void RestoreState(object args);
    }
}