using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Interaction
{
    public class Interactable : MonoBehaviour, IJsonSaveable
    {
        private bool disabled;

        public JToken CaptureAsJToken()
        {
            JObject state = new JObject();

            state["Disabled"] = true;

            return state;
        }

        public virtual void Interact()
        {
            if (disabled) return;

            SetDisabled(true);
        }

        public void Restart()
        {
            SetDisabled(false);
        }

        public void RestoreFromJToken(JToken state)
        {
            SetDisabled(state["Disabled"].ToObject<bool>());
        }

        private void SetDisabled(bool value)
        {
            disabled = value;
        }
    }
}
