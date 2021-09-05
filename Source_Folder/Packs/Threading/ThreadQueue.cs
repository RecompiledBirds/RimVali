using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace AvaliMod
{
    public class ThreadQueue : GameComponent
    {
        private Queue<Action> actionQueue = new Queue<Action>();
        private bool threadIsRunning = false;
        private Action nextAction;


        public bool CanStartNextThread
        {
            get
            {

                ThreadPool.GetAvailableThreads(out int wThreads, out _);
                return !threadIsRunning && wThreads > 0;
            }
        }
        public override void StartedNewGame()
        {
            actionQueue = new Queue<Action>();
            base.StartedNewGame();
        }

        public override void LoadedGame()
        {
            actionQueue = new Queue<Action>();
            base.LoadedGame();
        }


        private void RunNextAction()
        {
            if (actionQueue.Count > 0 && CanStartNextThread)
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
            if (actionQueue.Count < 100)
            {
                actionQueue.Enqueue(action);
            }
        }

        int onTick = 0;

        public ThreadQueue(Game game)
        {
            actionQueue = new Queue<Action>();
        }

        public override void GameComponentTick()
        {
            if(onTick == 120){
                onTick = 0;
                RunNextAction();
            }
            onTick++;
            base.GameComponentTick();
        }
    }
}
