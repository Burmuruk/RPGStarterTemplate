using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Burmuruk.Tesis.Interaction
{
    public class Interactable : MonoBehaviour, IJsonSaveable, IInteractable
    {
        [SerializeField] bool isPersistent;
        [SerializeField] bool triggerOnCollition = false;
        private bool disabled;

        public UnityEvent OnInteract;

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerOnCollition || disabled) return;

            if (other.GetComponent<AIGuildMember>() == FindAnyObjectByType<PlayerManager>().CurPlayer)
            {
                Interact();
            }
        }

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

        public void StopPlayer()
        {
            FindObjectOfType<Character>().StopActions(true);
        }

        public void UnpausePlayer()
        {
            FindObjectOfType<Character>().StopActions(false);
        }

        private void SetDisabled(bool value)
        {
            disabled = value;
        }
    }
}
