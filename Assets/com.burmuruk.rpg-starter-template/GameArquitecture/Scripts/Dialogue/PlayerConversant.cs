using System;
using System.Linq;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {
        [SerializeField] string playerName;
        Dialogue currentDialogue;
        DialogueNode currentNode = null;
        AIConversant currentConversant = null;

        public bool IsChoosing { get; private set; }
        public bool IsActive { get => currentDialogue != null; }

        public event Action OnConversationUpdated;
        //private void Awake()
        //{
        //    currentNode = currentDialogue.GetRootNode();
        //}

        public void StartDialogue(AIConversant newConversant, Dialogue newDialogue)
        {
            currentConversant = newConversant;
            currentDialogue = newDialogue;
            currentNode = newDialogue.dialogueNode;
            TriggerEnterAction();
            OnConversationUpdated?.Invoke();
        }

        public void Quit()
        {
            currentDialogue = null;
            TriggerExitAction();
            currentNode = null;
            IsChoosing = false;
            currentConversant = null;
            OnConversationUpdated?.Invoke();
        }

        public string GetText()
        {
            if (currentNode == null)
            {
                return "";
            }

            return currentNode.Message;
        }

        //public IEnumerable<DialogueNodeOld> GetChoices()
        //{
        //    return currentDialogue.GetPlayerChildren(currentNode);
        //}

        public string GetCurrentConversantName()
        {
            if (IsChoosing)
            {
                return playerName;
            }
            else
            {
                return currentConversant.GetName();
            }
        }

        public void SelectChoice(int idx)
        {
            currentNode = currentNode.Children[idx];
            TriggerEnterAction();
            IsChoosing = false;
            Next();
        }

        public void Next()
        {
            int numPlayerResponses = currentNode.Children.Count();
            if (numPlayerResponses > 0)
            {
                IsChoosing = true;
                TriggerExitAction();
                OnConversationUpdated?.Invoke();
                return;
            }

            var children = currentNode.Children;
            int randomIndex = UnityEngine.Random.Range(0, children.Count());
            TriggerExitAction();

            currentNode = children[randomIndex];
            TriggerEnterAction();
            OnConversationUpdated?.Invoke();
        }

        public bool HasNext()
        {
            return currentNode.Children.Count() > 0;
        }

        private void TriggerEnterAction()
        {
            if (currentNode != null)
            {
                TriggerAction(currentNode.GetOnEnterAction());
            }
        }

        private void TriggerExitAction()
        {
            if (currentNode != null)
            {
                TriggerAction(currentNode.GetOnExitAction());
            }
        }

        private void TriggerAction(string action)
        {
            if (action == "") return;

            foreach (var trigger in FindObjectsByType<DialogueTrigger>(FindObjectsSortMode.None))
            {
                if (trigger.Action == action)
                {
                    trigger.Trigger(action); 
                }
            }
        }
    }
}
