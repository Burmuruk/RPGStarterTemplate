using System.Collections.Generic;

namespace Burmuruk.Tesis.Utilities
{
    public struct ActionScheduler
    {
        LinkedList<ActionStatus> actions;

        public bool Initilized { get; private set; }

        public enum Priority
        {
            None,
            Low,
            Medium,
            High,
            Emergency,
            System
        }

        enum ActionState
        {
            None,
            Paused,
            Running,
            Canceling,
        }

        class ActionStatus
        {
            public IScheduledAction action;
            public Priority priority;
            public ActionState state;

            public ActionStatus(IScheduledAction action, Priority priority, ActionState state)
            {
                this.action = action;
                this.priority = priority;
                this.state = state;
            }
        }

        public void AddAction(IScheduledAction action, Priority priority)
        {
            actions ??= new();
            Initilized = true;

            switch (priority)
            {
                case Priority.Low:
                    actions.AddLast(new ActionStatus(action, priority, ActionState.None));
                    break;

                case Priority.Medium:
                case Priority.High:
                    break;

                case Priority.Emergency:
                    var curPriority = actions.First.Value.priority;

                    if (actions.Count > 0 && curPriority != Priority.Emergency || curPriority != Priority.System)
                    {
                        Pause(actions.First.Value.action);
                    }

                    break;

                case Priority.System:
                    if (actions.Count > 0 && actions.First.Value.priority != Priority.System)
                    {
                        Pause(actions.First.Value.action);
                    }

                    break;

                default:
                    return;
            }

            AddActionByPriority(new ActionStatus(action, priority, ActionState.None));
        }

        public void Start(IScheduledAction action, Priority priority)
        {
            action.StartAction();

            ActionStatus actionStatus;
            if (GetAction(action, out actionStatus))
            {
                actionStatus.state = ActionState.Running;
            }
            else
            {
                AddAction(action, priority);
            }
        }

        public void Pause(IScheduledAction action)
        {
            action.PauseAction();

            ActionStatus actionStatus;
            if (GetAction(action, out actionStatus))
                actionStatus.state = ActionState.Paused;
        }

        public void Cancel(IScheduledAction action)
        {
            ActionStatus actionStatus;

            if (GetAction(action, out actionStatus))
            {
                if (actionStatus.state == ActionState.Running)
                    actionStatus.action.CancelAction();

                actions.Remove(actionStatus);
            }
        }

        public void Finished(IScheduledAction action)
        {
            action.StartAction();

            ActionStatus actionStatus;
            if (GetAction(action, out actionStatus))
                actionStatus.state = ActionState.Running;
        }

        void AddActionByPriority(ActionStatus newAction)
        {
            var curNode = actions.First;
            int curPriority = (int)newAction.priority;

            while(curNode.Previous.Value.action != actions.First.Value.action)
            {
                if ((int)curNode.Value.priority < curPriority)
                {
                    actions.AddAfter(curNode, newAction);
                }
            }
        }

        bool GetAction(IScheduledAction action, out ActionStatus actionIdx)
        {
            actionIdx = null;

            foreach (ActionStatus item in actions)
            {
                if (item.action == action)
                {
                    actionIdx = item;
                    return true;
                }
            }

            return false;
        }
    }

    public interface IScheduledAction
    {
        void StartAction();
        void PauseAction();
        void ContinueAction();
        void CancelAction();
        void FinishAction();
    }
}
