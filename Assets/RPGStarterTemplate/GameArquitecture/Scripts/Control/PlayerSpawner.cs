using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Utilities;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class PlayerSpawner : ActivationObject
    {
        private void Awake()
        {
            if (TemporalSaver.TryLoad(_id, out object data))
                Enabled = (bool)data;
        }
    }
}
