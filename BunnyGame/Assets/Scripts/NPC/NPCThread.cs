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
                int[] cellPos = this.convertWorld2Cell(npc.getPos());
                var currentCell = NPCWorldView.getCell(cellPos[0], cellPos[1]);
                if (currentCell.blocked) {
                    //find out if the NPC is facing the obstacle, or is moving away from it
                    //Math source: https://math.stackexchange.com/questions/878785/how-to-find-an-angle-in-range0-360-between-2-vectors
                    Vector3 temp = currentCell.pos - npc.getPos();
                    Vector2 cellDir = new Vector2(temp.x, temp.z);
                    Vector2 dir = new Vector2(npc.getDir().x, npc.getDir().z);
                    float dot = Vector2.Dot(dir, cellDir) / (dir.magnitude * cellDir.magnitude);
                    float det = dir.x * cellDir.y - dir.y * cellDir.x;
                    float angle = Mathf.Atan2(det, dot);
                    if (angle < 90 || angle > 270) {
                        //Debug.Log("Adding instruction for npc with id " + npc.getId());
                        this._instructions.Enqueue(new instruction(npc.getId(), npc.getDir() * -1));
                    }
                }
            }
            Thread.Sleep(100); // Will improve next week
        }
	}

    int[] convertWorld2Cell(Vector3 world) {
        int[] cellPos = { 0, 0 };
        //new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2) + _offset;
        world -= NPCWorldView.offset;
        world /= NPCWorldView.cellSize;
        //world.x -= NPCWorldView.cellSize / 2;
        //world.z -= NPCWorldView.cellSize / 2;

        cellPos[0] = clamp((int)world.x);
        cellPos[1] = clamp((int)world.z);

        return cellPos;
    }

    int clamp(int input) {
        input = (input >= 0) ? input : 0;
        input = (input < NPCWorldView.cellCount) ? input : NPCWorldView.cellCount - 1;
        return input;
    }
}
