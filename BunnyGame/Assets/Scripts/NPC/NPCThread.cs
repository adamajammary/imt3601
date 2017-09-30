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
                int[] cellPos = NPCWorldView.convertWorld2Cell(npc.getPos());
                var landCell = NPCWorldView.getCell(NPCWorldView.WorldPlane.LAND, cellPos[0], cellPos[1]);
                var waterCell = NPCWorldView.getCell(NPCWorldView.WorldPlane.WATER, cellPos[0], cellPos[1]);
                Debug.Log("FUUUCK");
                if (landCell.blocked) {
                    //find out if the NPC is facing the obstacle, or is moving away from it
                    Vector3 temp = landCell.pos - npc.getPos();
                    Vector2 cellDir = new Vector2(temp.x, temp.z);
                    Vector2 dir = new Vector2(npc.getDir().x, npc.getDir().z);
                    float angle = Vector2.Angle(dir, cellDir);
                    
                    if (angle < 90) {
                        Debug.Log("Sending instructions!");
                        this._instructions.Enqueue(new instruction(npc.getId(), npc.getDir() * -1));
                    }
                }
            }
            Thread.Sleep(100); // Will improve next week
        }
	}
}
