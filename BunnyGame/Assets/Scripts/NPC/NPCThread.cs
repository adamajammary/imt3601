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
    private bool _isUpdating;
    private BlockingQueue<instruction> _instructions;
    System.Random _rng;
    

	// Use this for initialization
	public NPCThread (BlockingQueue<instruction> i) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        _rng = new System.Random();
        this._thread.Start();
	}
	
    public bool isUpdating { get { return this._isUpdating; } }

	// Update is called once per frame
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            if (this._instructions.isEmpty()) {
                this._isUpdating = true;
                var npcs = NPCWorldView.getNpcs();
                var players = NPCWorldView.getPlayers();
                foreach (var npc in npcs.Values) {
                    Vector3 dir = avoidObstacle(npc);
                    if (dir != Vector3.zero) {
                        this.sendInstuction(npc, dir);
                    } else {
                        dir = avoidPlayers(npc);
                        if (dir != Vector3.zero) {
                            this.sendInstuction(npc, dir);
                        } else {
                            dir = this.randomDir(npc);
                            if (dir != Vector3.zero) {
                                this.sendInstuction(npc, dir);
                            }
                        }
                    }
                }
                this._isUpdating = false;
            }
        }
	}

    private void sendInstuction(NPCWorldView.GameCharacter npc, Vector3 dir) {
        npc.update(npc.getPos(), dir);
        instruction i = new instruction(npc.getId(), dir);
        this._instructions.Enqueue(i);
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

    private Vector3 avoidPlayers(NPCWorldView.GameCharacter npc) {
        var players = NPCWorldView.getPlayers();
        NPCWorldView.GameCharacter closestPlayer = null;
        float closestDist = float.MaxValue;
        foreach (var player in players.Values) {
            float dist = Vector3.Distance(npc.getPos(), player.getPos());
            if (dist < closestDist) {
                closestDist = dist;
                closestPlayer = player;
            }
        }

        if (closestDist < 20) {
            float a = angle(npc.getDir(), closestPlayer.getPos() - npc.getPos());
            if (a < 75) { // In field of view?
                if (NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), closestPlayer.getPos()) == float.MaxValue) { // in line of sight?
                    Vector3 flee = npc.getPos() - closestPlayer.getPos();
                    flee.y = 0;
                    return flee.normalized;
                }
            }
        }
        return Vector3.zero;
    }

    private Vector3 randomDir(NPCWorldView.GameCharacter npc) {
        if (this._rng.NextDouble() < 0.05f) {
            Vector3 dir = npc.getDir();
            if (this._rng.NextDouble() > 0.5f)
                return Quaternion.AngleAxis(10, Vector3.up) * dir;
            else
                return Quaternion.AngleAxis(-10, Vector3.up) * dir;
        }
        return Vector3.zero;
    }

    float angle(Vector3 a3, Vector3 b3) {
        Vector2 a2 = new Vector2(a3.x, a3.z);
        Vector2 b2 = new Vector2(b3.x, b3.z);
        return Vector2.Angle(a2, b2); 
    }
}

