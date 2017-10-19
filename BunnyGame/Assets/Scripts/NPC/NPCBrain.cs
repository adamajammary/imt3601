using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain {
    //==============Member variables==================================================
    private delegate void stateFunction();
    private Stack<stateFunction>                _state;
    private NPCWorldView.GameCharacter          _npc;
    private BlockingQueue<NPCThread.instruction>          _instructions;
    private float                               _speed;
    private Stack<NPCWorldView.worldCellData>   _path;

    //==============Constructor==================================================
    public NPCBrain(NPCWorldView.GameCharacter npc, BlockingQueue<NPCThread.instruction> i){
        this._npc               = npc;
        this._state             = new Stack<stateFunction>();
        this._instructions      = i;
        this._speed             = 1.0f;
        this._path              = new Stack<NPCWorldView.worldCellData>();

        this._state.Push(roam);
    }

    //==============Update==================================================
    public void update() {
        this._state.Peek()();
    }

    //==============Functions==================================================
    private void sendInstuction(Vector3 dir) {
        if (dir == Vector3.zero || dir == this._npc.getDir()) return;
        this._npc.update(this._npc.getPos() + this._npc.getDir() * (1 / 60), this._npc.getDir()); // Try to guess next pos
        this._npc.update(this._npc.getPos(), dir); // Try to guess next pos
        NPCThread.instruction i = new NPCThread.instruction(this._npc.getId(), dir);
        this._instructions.Enqueue(i);
    }

    void AStar(NPCWorldView.worldCellData startCell, NPCWorldView.worldCellData goal) {
        Dictionary<Vector3, NPCWorldView.worldCellData> closed =
            new Dictionary<Vector3, NPCWorldView.worldCellData>();                                             //For quickly looking up closed nodes
        SortedList<float, NPCWorldView.worldCellData> open =
            new SortedList<float, NPCWorldView.worldCellData>(new NPCWorldView.DuplicateKeyComparer<float>()); //For quickly finding best node to visit                                

        this._path.Clear(); //Clear any old paths

        var current = startCell;

        NPCWorldView.resetAStarData();
        current.g = 0;
        open.Add(current.f, current); //Push the start node
        while (open.Count > 0) {
            do { //Outdated cells might still be in the list
                current = open.Values[0];
                open.RemoveAt(0);
            } while (closed.ContainsKey(current.pos) && open.Count > 0);

            if (current.pos == goal.pos) {    //Victor               
                NPCWorldView.worldCellData tmp = goal;
                while (tmp.parent != null) {
                    this._path.Push(tmp);
                    tmp = tmp.parent;
                }
                this._path.Push(tmp);
                return;
            }
            if (open.Count == 0 && closed.ContainsKey(current.pos))
                break;
            //Close current tile
            closed.Add(current.pos, current);

            for (int i = 0; i < current.neighbours.Count; i++) {
                var cell = current.neighbours[i];
                if (!closed.ContainsKey(cell.pos) && !cell.blocked) {
                    float g = current.g + 1;
                    if (g < cell.g) { //New and better G value?
                        cell.h = Mathf.Abs(goal.x - cell.x) + Mathf.Abs(goal.y - cell.y);
                        cell.g = g;
                        cell.parent = current;
                        open.Add(cell.f, cell);
                    }
                }
            }
        }
    }

    private float hash(Vector3 i) {
        float temp = Mathf.Abs((Mathf.Sin(i.x) + Mathf.Sin(i.y) + Mathf.Sin(i.z)) * 1464.468f);
        return temp - (int)temp; 
    }

    //==============States==================================================
    private void roam() {
        Vector3 roamDir = Vector3.zero;
        Vector3 mapPos = this._npc.getMapPos();
        if (hash(mapPos) < 0.2) {
            if (hash(mapPos/147f) > 0.5f) 
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._npc.getDir();
             else 
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._npc.getDir();            
        }
        sendInstuction(roamDir);
    }

    private void avoidObstacle() {
        
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
        return turnTowards(npc.getDir(), flee);
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
            return turnTowards(npc.getDir(), fleeDir.normalized);
        } else
            return Vector3.zero;
    }

    private Vector3 turnTowards(Vector3 current, Vector3 dir) {
        float turnAngle = 5;

        Vector3 left = Quaternion.AngleAxis(turnAngle, Vector3.up) * current;
        Vector3 right = Quaternion.AngleAxis(-turnAngle, Vector3.up) * current;
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
