using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Burmuruk.Tesis.Interaction
{
    public class Interactable : MonoBehaviour, IJsonSaveable, IInteractable
    {
        [SerializeField] bool isPersistent;
        private bool disabled;

        public UnityEvent OnInteract;

        public JToken CaptureAsJToken(out SavingExecution execution)
        {
            execution = SavingExecution.General;

            if (!isPersistent) return null;

            JObject state = new JObject();

            state["Disabled"] = true;

            return state;
        }

        public virtual void Interact()
        {
            if (disabled) return;

            OnInteract?.Invoke();

            SetDisabled(true);
        }

        public void Restart()
        {
            SetDisabled(false);
        }

        public void LoadAsJToken(JToken state)
        {
            if (state == null) return;

            SetDisabled(state["Disabled"].ToObject<bool>());
        }

        private void SetDisabled(bool value)
        {
            disabled = value;
        }
    }
}
