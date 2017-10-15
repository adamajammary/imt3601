using System.Threading;
using System.Diagnostics;
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

    private enum State { FLEEING = 1, AVOIDING = 2, FIREWALL = 4, ROAMING = 8 }
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
    private Vector3[] avoidSensors;
    System.Random _rng;

    //==============Constructor==================================================
    public NPCThread (BlockingQueue<instruction> i) {
        this._thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        this._instructions = i;
        this._rng = new System.Random(691337);

        this._npcStates = new Dictionary<int, NPCState>();
        var npcs = NPCWorldView.getNpcs();
        foreach (var npc in npcs.Values) {
            this._npcStates.Add(npc.getId(), new NPCState(npc.getId()));
        }

        avoidSensors = new Vector3[8];
        for (int j = 0; j < avoidSensors.Length; j++) {
            avoidSensors[j] = Quaternion.AngleAxis(360 * ((float)j / (float)avoidSensors.Length), Vector3.up) * Vector3.forward;
        }

        this._thread.Start();
	}
	
    public bool isUpdating { get { return this._isUpdating; } }

	//==============NPC Loop==================================================
	void threadRunner () {
        while (NPCWorldView.getRunNPCThread()) {
            if (this._instructions.isEmpty()) {
                this._isUpdating = true;
                var npcs = NPCWorldView.getNpcs();
                foreach (var npc in npcs.Values) {
                    float speed = 1.0f;
                    NPCState state;
                    this._npcStates.TryGetValue(npc.getId(), out state);
                    Vector3 avoidDir, fleeDir, fireWallDir;

                    avoidDir = this.avoidObstacle(npc);
                    if (avoidDir != Vector3.zero) state.add(State.AVOIDING);
                    else state.remove(State.AVOIDING);

                    fireWallDir = fleeFireWall(npc);
                    if (fireWallDir != Vector3.zero) state.add(State.FIREWALL);
                    else state.remove(State.FIREWALL);

                    var player = this.closestPlayer(npc);
                    if (player != null) {
                        if (this.canSeePlayer(npc, player)) state.add(State.FLEEING);                                           
                    } else state.remove(State.FLEEING);

                    if (state.contains(State.FLEEING) || state.contains(State.FIREWALL)) speed = 3.0f;

                    if (state.contains(State.AVOIDING)) {
                        this.sendInstuction(npc, avoidDir.normalized * speed);
                    } else if (state.contains(State.FIREWALL)) {
                        this.sendInstuction(npc, fireWallDir.normalized * speed);
                    } else if (state.contains(State.FLEEING)) {
                        fleeDir = this.avoidPlayer(npc, player);
                        this.sendInstuction(npc, fleeDir.normalized * speed);
                    }
                    npc.update(npc.getPos() + npc.getDir() * (1 / 60),  npc.getDir()); // Try to guess next pos
                }
                this._isUpdating = false;
            }
        }
	}

    //==============Functions==================================================
    private void sendInstuction(NPCWorldView.GameCharacter npc, Vector3 dir) {
        if (dir == npc.getDir()) return;
        npc.update(npc.getPos(), dir); // Try to guess next pos
        instruction i = new instruction(npc.getId(), dir);
        this._instructions.Enqueue(i);
    }

    private Vector3 avoidObstacle(NPCWorldView.GameCharacter npc) {
        float viewDist = 15f;
        Vector3 dir = npc.getDir();
        if (detectObstacle(npc)) {
            Vector3 bestDir = Vector3.forward;
            float bestLen = 0;
            for (int i = 0; i < avoidSensors.Length; i++) {
                float len = NPCWorldView.rayCast(NPCWorldView.WorldPlane.LAND, npc.getPos(), npc.getPos() + avoidSensors[i] * viewDist);
                if (len > bestLen) {
                    bestLen = len;
                    bestDir = avoidSensors[i];
                }
            }
            return turnTowards(bestDir);
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
        Vector3 flee = npc.getPos() - player.getPos();
        flee.y = 0;
        return  turnTowards(flee);
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

    private Vector3 fleeFireWall(NPCWorldView.GameCharacter npc) {
        float dist = Vector3.Distance(npc.getPos(), NPCWorldView.FireWall.pos);
        float viewDist = 20;
        if ((NPCWorldView.FireWall.radius - dist) < viewDist) {
            Vector3 fleeDir = NPCWorldView.FireWall.pos - npc.getPos();
            fleeDir.y = 0;
            return turnTowards(fleeDir.normalized);
        } else
            return Vector3.zero;
    }

    private Vector3 turnTowards(Vector3 dir) {
        float turnAngle = 5;

        Vector3 left = Quaternion.AngleAxis(turnAngle, Vector3.up) * dir;
        Vector3 right = Quaternion.AngleAxis(-turnAngle, Vector3.up) * dir;
        float leftAngle = angle(dir, left);
        float rightAngle = angle(dir, right);
        Vector3 retVec = (leftAngle <= rightAngle) ? left : right;
        return (leftAngle >= turnAngle || rightAngle >= turnAngle) ? retVec : dir;
    }

    float angle(Vector3 a3, Vector3 b3) {
        Vector2 a2 = new Vector2(a3.x, a3.z);
        Vector2 b2 = new Vector2(b3.x, b3.z);
        return Vector2.Angle(a2, b2); 
    }
}

