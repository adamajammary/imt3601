using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class NPCThread {
    //==============Member variables==================================================
    public struct instruction {
        public Vector3 moveDir;
        public Vector3 goal;
        public int id;

        public instruction(int id, Vector3 moveDir, Vector3 goal) {
            this.id = id;
            this.moveDir = moveDir;
            this.goal = goal;
        }
    }

    private Thread _thread;
    private const int minUpdateTime =16; // This means that the minimum update time for the thread is 16 milliseconds.
    private Stopwatch _stopwatch;
    private bool _isUpdating;
    private bool _wait;
    private bool _run;
    private BlockingQueue<instruction> _instructions;
    private List<NPCBrain> _npcBrains;
    private List<NPCBrain> _deadNpcs;
    private WorldGrid _worldGrid;

    //==============Constructor==================================================
    public NPCThread(BlockingQueue<instruction> i, List<NPCWorldView.GameCharacter> npcs) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        this._npcBrains = new List<NPCBrain>();
        this._deadNpcs = new List<NPCBrain>();
        this._wait = false;
        this._worldGrid = WorldData.worldGrid.getCopy();

        foreach (var npc in npcs) {
            npc.worldGrid = this._worldGrid;
            this._npcBrains.Add(new NPCBrain(npc, this._instructions, this._worldGrid));
        }

        this._run = true;
        this._stopwatch = new Stopwatch();
        this._thread.Start();
    }

    public bool isUpdating { get { return this._isUpdating; } }
    public bool wait { get { return this._wait; } set { this._wait = value; } }

    public void stop() {
        this._run = false;
    }

    //==============NPC Loop==================================================
    void threadRunner() {
        while (this._run) {
            this._stopwatch.Reset();
            this._stopwatch.Start();
            if (!this._wait) {
                this._isUpdating = true;
                foreach (var npcBrain in this._npcBrains) {
                    if (npcBrain.npcAlive())
                        npcBrain.update();
                    else
                        this._deadNpcs.Add(npcBrain);
                }
                this._isUpdating = false;
                foreach (var npcBrain in this._deadNpcs)
                    this._npcBrains.Remove(npcBrain);
                this._deadNpcs.Clear();
            }
            int sleepTime = (int) (minUpdateTime - this._stopwatch.ElapsedMilliseconds);
            if (sleepTime > 0) Thread.Sleep(sleepTime);
            this._stopwatch.Stop();
            //UnityEngine.Debug.Log(this._stopwatch.ElapsedMilliseconds);
        }
    }
}
