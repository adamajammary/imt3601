using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class NPCThread {
    //==============Member variables==================================================
    public struct instruction {
        public Vector3 moveDir;
        public int id;

        public instruction(int id, Vector3 moveDir) {
            this.id = id;
            this.moveDir = moveDir;
        }
    }

    private Thread _thread;
    private bool _isUpdating;
    private BlockingQueue<instruction> _instructions;
    private List<NPCBrain> _npcBrains; 

    //==============Constructor==================================================
    public NPCThread (BlockingQueue<instruction> i) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;

        var npcs = NPCWorldView.getNpcs();
        foreach (var npc in npcs.Values) 
            this._npcBrains.Add(new NPCBrain(npc, this._instructions));

        this._thread.Start();
	}
	
    public bool isUpdating { get { return this._isUpdating; } }

	//==============NPC Loop==================================================
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            if (this._instructions.isEmpty()) {
                this._isUpdating = true;
                foreach (var npcBrain in this._npcBrains) 
                    npcBrain.update();                
                this._isUpdating = false;
            }
        }
	}    
}

