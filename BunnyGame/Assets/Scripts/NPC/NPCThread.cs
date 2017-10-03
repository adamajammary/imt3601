using System.Threading;
using System.Diagnostics;
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

    private enum State { FLEEING = 1, AVOIDING = 2, ROAMING = 4 }
    private class NPCState {
        private int _id;
        private State _state;
      
        public NPCState(int id) {
            this._id = id;
            _state = State.ROAMING;
        }

        public int id { get { return id; } }
        public State get() { return this._state; }
        public bool contains(State state) { return ((this._state & state) == state); }
        public void add(State state) { this._state |= state; }
        public void remove(State state) { if (this.contains(state)) this._state ^= state; }
    }

    private Thread _thread;
    private bool _isUpdating;
    private BlockingQueue<instruction> _instructions;
    private Dictionary<int, NPCState> _npcStates;
    System.Random _rng;    

	// Use this for initialization
	public NPCThread (BlockingQueue<instruction> i) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        this._rng = new System.Random(691337);

        this._npcStates = new Dictionary<int, NPCState>();
        var npcs = NPCWorldView.getNpcs();
        foreach (var npc in npcs.Values) {
            this._npcStates.Add(npc.getId(), new NPCState(npc.getId()));
        }

        this._thread.Start();
	}
	
    public bool isUpdating { get { return this._isUpdating; } }

	// Update is called once per frame
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            if (this._instructions.isEmpty()) {
                this._isUpdating = true;
                var npcs = NPCWorldView.getNpcs();
                foreach (var npc in npcs.Values) {
                    float speed = 1.0f;
                    NPCState state;
                    this._npcStates.TryGetValue(npc.getId(), out state);
                    Vector3 avoidDir, fleeDir, roamDir;

                    avoidDir = this.avoidObstacle(npc);
                    if (avoidDir != Vector3.zero) state.add(State.AVOIDING);
                    else state.remove(State.AVOIDING);

                    var player = this.closestPlayer(npc);
                    if (player != null) {
                        if (this.canSeePlayer(npc, player)) state.add(State.FLEEING);                                           
                    } else state.remove(State.FLEEING);

                    if (state.contains(State.FLEEING)) speed = 3.0f;

                    if (state.contains(State.AVOIDING)) {
                        this.sendInstuction(npc, avoidDir.normalized * speed);
                    } else if (state.contains(State.FLEEING)) {
                        fleeDir = this.avoidPlayer(npc, player);
                        this.sendInstuction(npc, fleeDir.normalized * speed);
                    } else if (state.contains(State.ROAMING)) {
                        //This thing goes out of sync real fast
                        //roamDir = this.randomDir(npc);
                        //if (roamDir != Vector3.zero) this.sendInstuction(npc, roamDir.normalized * speed);
                    }
                    npc.update(npc.getPos() + npc.getDir() * (1 / 60),  npc.getDir()); // Try to guess next pos
                }
                this._isUpdating = false;
            }
        }
	}

    private void sendInstuction(NPCWorldView.GameCharacter npc, Vector3 dir) {
        npc.update(npc.getPos(), dir); // Try to guess next pos
        instruction i = new instruction(npc.getId(), dir);
        this._instructions.Enqueue(i);
    }

    private Vector3 avoidObstacle(NPCWorldView.GameCharacter npc) {
        float viewDist = 10.0f;
        float turnAngle = 10;
        Vector3 dir = npc.getDir();
        if (detectObstacle(npc)) {
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

    private bool detectObstacle(NPCWorldView.GameCharacter npc) {
        float viewDist = 5.0f;
        float fov = 45;
        Vector3 dir = npc.getDir();
        Vector3[] eyes = new Vector3[3];
        eyes[0] = dir;
        eyes[1] = Quaternion.AngleAxis(fov, Vector3.up) * dir;
        eyes[2] = Quaternion.AngleAxis(-fov, Vector3.up) * dir;
        foreach (var eye in eyes) {
            if (NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + eye * viewDist) != float.MaxValue)
                return true;
        }
        return false;
    }

    private bool canSeePlayer(NPCWorldView.GameCharacter npc, NPCWorldView.GameCharacter player) {        
            float a = angle(npc.getDir(), player.getPos() - npc.getPos());
            if (a < 90) { // In field of view?
                if (NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), player.getPos()) == float.MaxValue) { // in line of sight?
                    return true;
                }
            }        
        return false;
    }

    private Vector3 avoidPlayer(NPCWorldView.GameCharacter npc, NPCWorldView.GameCharacter player) {
        float turnAngle = 5;
        Vector3 dir = npc.getDir();
        Vector3 flee = npc.getPos() - player.getPos();
        flee.y = 0;

        Vector3 left = Quaternion.AngleAxis(turnAngle, Vector3.up) * dir;
        Vector3 right = Quaternion.AngleAxis(-turnAngle, Vector3.up) * dir;
        float leftAngle = angle(flee, left);
        float rightAngle = angle(flee, right);
        return (leftAngle <= rightAngle) ? left : right;
    }

    private NPCWorldView.GameCharacter closestPlayer(NPCWorldView.GameCharacter npc) {
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
        if (closestDist < 15) return closestPlayer;
        else return null;        
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

