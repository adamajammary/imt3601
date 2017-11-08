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
    private bool _isUpdating;
    private bool _wait;
    private BlockingQueue<instruction> _instructions;
    private List<NPCBrain> _npcBrains;
    private List<NPCBrain> _deadNpcs;

    //==============Constructor==================================================
    public NPCThread(BlockingQueue<instruction> i) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        this._npcBrains = new List<NPCBrain>();
        this._deadNpcs = new List<NPCBrain>();
        this._wait = false;

        var npcs = NPCWorldView.npcs;
        foreach (var npc in npcs.Values)
            this._npcBrains.Add(new NPCBrain(npc, this._instructions));

        this._thread.Start();
    }

    public bool isUpdating { get { return this._isUpdating; } }
    public bool wait { get { return this._wait; } set { this._wait = value; } }

    //==============NPC Loop==================================================
    void threadRunner() {
        while (NPCWorldView.runNpcThread) {
            if (this._instructions.isEmpty() && !this._wait) {
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
        }
    }
}
