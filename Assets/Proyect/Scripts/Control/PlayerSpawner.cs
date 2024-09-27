using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class PlayerSpawner : MonoBehaviour, IJsonSaveable, ISerializationCallbackReceiver
    {
        [SerializeField] bool spawnEnabled = false;
        [SerializeField] private int _id = 0;

        public bool Enabled { get => spawnEnabled; private set => spawnEnabled = value; }

        private void Awake()
        {
            if (TemporalSaver.TryLoad(_id, out object data))
                Enabled = (bool)data;
        }

        public void Enable(bool shouldEnable)
        {
            Enabled = shouldEnable;
            TemporalSaver.Save(_id, shouldEnable);
        }

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
            if (_id == 0)
                _id = GetHashCode();
        }

        public JToken CaptureAsJToken(out SavingExecution execution)
        {
            execution = SavingExecution.Level;
            JObject state = new JObject();

            state["Enabled"] = Enabled;

            return state;
        }

        public void LoadAsJToken(JToken state)
        {
            Enabled = (state as JObject)["Enabled"].ToObject<bool>();
        }
    }
}
