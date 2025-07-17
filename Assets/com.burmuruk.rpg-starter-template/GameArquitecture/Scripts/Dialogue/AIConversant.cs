using Burmuruk.RPGStarterTemplate.Control;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    public class AIConversant : MonoBehaviour
    {
        [SerializeField] Dialogue dialogue;
        [SerializeField] string conversantName;


        public bool HandleRaycast(Character callingController)
        {
            if (dialogue == null) return false;

            if (Input.GetMouseButtonDown(0))
            {
                callingController.GetComponent<PlayerConversant>().StartDialogue(this, dialogue);
            }

            return true;
        }

        public string GetName()
        {
            return conversantName;
        }
    }
}
