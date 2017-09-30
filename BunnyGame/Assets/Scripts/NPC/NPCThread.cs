using System.Threading;
using System;
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
    System.Random _rng;
    

	// Use this for initialization
	public NPCThread (BlockingQueue<instruction> i) {
        this._runThread = true;
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        _rng = new System.Random();
        this._thread.Start();
	}
	
	// Update is called once per frame
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            if (this._instructions.isEmpty()) {
                var npcs = NPCWorldView.getNpcs();
                var players = NPCWorldView.getPlayers();
                foreach (var npc in npcs) {
                    Vector3 avoidDir = avoidObstacle(npc);
                    if (avoidDir != Vector3.zero) {
                        npc.update(npc.getPos(), avoidDir);
                        instruction i = new instruction(npc.getId(), avoidDir);
                        this._instructions.Enqueue(i);
                    } else if (_rng.NextDouble() < 0.05f) {
                        Vector3 dir = npc.getDir();
                        if (this._rng.NextDouble() > 0.5f) 
                            dir = Quaternion.AngleAxis(10, Vector3.up) * dir;
                        else
                            dir = Quaternion.AngleAxis(-10, Vector3.up) * dir;
                        instruction i = new instruction(npc.getId(), dir);
                        this._instructions.Enqueue(i);
                    }
                }
            }
        }
	}

    private Vector3 avoidObstacle(NPCWorldView.GameCharacter npc) {
        float viewDist = 10.0f;
        float turnAngle = 10;
        Vector3 dir = npc.getDir();
        if (NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + dir * 10f) != float.MaxValue) {
            Vector3 left = Quaternion.AngleAxis(turnAngle, Vector3.up) * dir;
            Vector3 right = Quaternion.AngleAxis(-turnAngle, Vector3.up) * dir;
            Vector3 superLeft = Quaternion.AngleAxis(90, Vector3.up) * dir;
            Vector3 superRight = Quaternion.AngleAxis(-90, Vector3.up) * dir;
            float leftDist = NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + superLeft * viewDist);
            float rightDist = NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + superRight * viewDist);
            return (leftDist >= rightDist) ? left : right;
        }

        return Vector3.zero;
    }

    float angle(Vector3 a3, Vector3 b3) {
        Vector2 a2 = new Vector2(a3.x, a3.z);
        Vector2 b2 = new Vector2(b3.x, b3.z);
        return Vector2.Angle(a2, b2); 
    }
}
