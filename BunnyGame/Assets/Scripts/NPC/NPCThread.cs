using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCThread {
    public struct instruction {
        public Vector3 moveDir;
        public int id;

        public instruction(int id, Vector3 moveDir) {
            this.id = id;
            this.moveDir = moveDir;
        }
    }

    private Thread _thread;
    private bool _runThread;
    private BlockingQueue<instruction> _instructions;
    

	// Use this for initialization
	public NPCThread (BlockingQueue<instruction> i) {
        this._runThread = true;
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        this._thread.Start();
	}
	
	// Update is called once per frame
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            var npcs = NPCWorldView.getNpcs();
            var players = NPCWorldView.getPlayers();
            foreach (var npc in npcs) {
                Vector3 avoidDir = avoidObstacle(npc);
                if (avoidDir != Vector3.zero) {
                    npc.update(npc.getPos(), avoidDir);
                    instruction i = new instruction(npc.getId(), avoidDir);
                    this._instructions.Enqueue(i);
                }           
            }
            
        }
	}

    private Vector3 avoidObstacle(NPCWorldView.GameCharacter npc) {
        Vector3 avoidDir = Vector3.zero;
        if (NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + npc.getDir() * 7.5f))
            avoidDir = -npc.getDir();
        return avoidDir.normalized;
    }

    float angle(Vector3 a3, Vector3 b3) {
        Vector2 a2 = new Vector2(a3.x, a3.z);
        Vector2 b2 = new Vector2(b3.x, b3.z);
        return Vector2.Angle(a2, b2); 
    }
}
