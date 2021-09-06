using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaliMod
{
    public class ThreadQueue : WorldComponent
    {
        private readonly Queue<Action> actionQueue = new Queue<Action>();
        private bool threadIsRunning = false;
        private Action nextAction;

        private void RunNextAction()
        {
            if (actionQueue.Count > 0 && !threadIsRunning)
            {
                nextAction = actionQueue.Dequeue();
                Task task = new Task(Run);
                task.Start();
            }
        }

        private void Run()
        {
            threadIsRunning = true;
            nextAction?.Invoke();
            threadIsRunning = false;
        }

        public void AddActionToQueue(Action action)
        {
            if (actionQueue.Count < 100000)
            {
                actionQueue.Enqueue(action);
            }
        }

        private int onTick = 0;

        public ThreadQueue(World world) : base(world)
        {
            this.actionQueue = new Queue<Action>();
        }

        public override void WorldComponentTick()
        {
            if (onTick == 2)
            {
                onTick = 0;
                RunNextAction();
            }
            onTick++;
        }
    }
}