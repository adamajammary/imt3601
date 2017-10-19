using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain { 
    //==============Member variables==================================================
    private Stack<State>                            _state;
    private NPCWorldView.GameCharacter              _npc;
    private BlockingQueue<NPCThread.instruction>    _instructions;
    private float                                   _speed;

    //==============Constructor==================================================
    public NPCBrain(NPCWorldView.GameCharacter npc, BlockingQueue<NPCThread.instruction> i){
        this._npc               = npc;
        this._state             = new Stack<State>();
        this._instructions      = i;
        this._speed             = 1.0f;

        this._state.Push(new Roam(this));
    }

    //==============Update==================================================
    public void update() {
        this._state.Peek().update();
    }

    //==============Functions==================================================
    public bool npcAlive() {
        return this._npc != null;
    }

    private void sendInstuction(Vector3 dir) {
        if (dir == Vector3.zero || dir == this._npc.getDir()) return;
        NPCThread.instruction i = new NPCThread.instruction(this._npc.getId(), dir.normalized * this._speed);
        this._instructions.Enqueue(i);
    }   

    //==============State super class==================================================
    private class State {
        protected NPCBrain _brain;
        public State(NPCBrain x) { this._brain = x; }
        public virtual void update() { }

        //Usefull functions for any state
        protected bool detectObstacle() {
            var npc = this._brain._npc;
            float viewDist = 5;
            float fov = 45;
            Vector3 dir = npc.getDir();
            Vector3[] eyes = new Vector3[3];
            eyes[0] = dir;
            eyes[1] = Quaternion.AngleAxis(fov, Vector3.up) * dir;
            eyes[2] = Quaternion.AngleAxis(-fov, Vector3.up) * dir;
            foreach (var eye in eyes) {
                if (NPCWorldView.rayCast(true, npc.getPos(), npc.getPos() + eye * viewDist) != float.MaxValue)
                    return true;
            }
            return false;
        }

        protected Vector3 turnTowards(Vector3 current, Vector3 dir) {
            float turnAngle = 10;

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

    //==============State classes==================================================

    //==============Roam state==================================================
    private class Roam : State {
        Vector3 lastCell;
        public Roam(NPCBrain x) : base(x) {
            lastCell = new Vector3(-1000, -1000, -1000);
        }

        override public void update() {
            roam();
            if (detectObstacle()) {
                this._brain._state.Push(new AvoidObstacle(this._brain, this._brain._npc.getDir()));
            }
        }

        private void roam() {
            Vector3 roamDir = Vector3.zero;
            Vector3 mapPos = this._brain._npc.getMapPos();
            if (lastCell == mapPos) return;
            if (hash(mapPos) < 0.5)
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._brain._npc.getDir();
            else
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._brain._npc.getDir();
            this._brain.sendInstuction(roamDir);
            lastCell = mapPos;
        }

        private float hash(Vector3 i) {
            float temp = Mathf.Abs((Mathf.Sin(i.x) + Mathf.Sin(i.y) + Mathf.Sin(i.z)) * 1464.468f);
            return temp - (int)temp;
        }
    }

    //==============Avoid obstacle state==================================================
    private class AvoidObstacle : State {
        private Stack<NPCWorldView.worldCellData> _path;
        public AvoidObstacle(NPCBrain x, Vector3 prefDir) : base(x) {
            this._path = new Stack<NPCWorldView.worldCellData>();
            AStar(this._brain._npc.getCell(), findTargetCell(prefDir));
        }

        override public void update() {
            walkPath();
            if (this._path.Count == 0) {
                this._brain._state.Pop(); // Return to previous state
            }
            
        }

        private void walkPath() {
            var npc = this._brain._npc;
            Vector3 pathDir;

            pathDir = this._path.Peek().pos - npc.getPos();
            pathDir.y = 0;
            pathDir.Normalize();
            this._brain.sendInstuction(pathDir);
            if (npc.getCell() == this._path.Peek()) this._path.Pop();
        }

        private NPCWorldView.worldCellData findTargetCell(Vector3 prefDir) {
            NPCWorldView.worldCellData target = null;
            Vector3 testDir;
            float degInc = 180 / 8;
            while (target == null) {
                float start = 5;
                target = probeDir(prefDir, start);
                if (target != null) break;
                for (float deg = degInc; deg < 180; deg += degInc) {
                    testDir = Quaternion.AngleAxis(deg, Vector3.up) * prefDir;
                    target = probeDir(testDir, start);
                    if (target != null) break;
                    testDir = Quaternion.AngleAxis(-deg, Vector3.up) * prefDir;
                    target = probeDir(testDir, start);
                    if (target != null) break;
                }
                start += 10;
            }
            return target; //This shouldn't happen
        }

        private NPCWorldView.worldCellData probeDir(Vector3 dir, float start) {
            Vector3 pos = this._brain._npc.getPos();
            float probeLen = start + 10;
            float mult;
            float cellSize = NPCWorldView.cellSize;
            for (mult = start; mult < probeLen; mult++) {
                Vector3 rayEnd = pos + dir * cellSize * mult;
                int[] i = NPCWorldView.convertWorld2Cell(rayEnd);
                var cell = NPCWorldView.getCell(true, i[0], i[1]);
                var waterCell = NPCWorldView.getCell(false, i[0], i[1]);
                if (!cell.blocked && waterCell.blocked)
                    return cell;
            }
            return null;            
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

                //Close current tile
                closed.Add(current.pos, current);
                foreach (var cell in current.neighbours) {
                    var waterCell = NPCWorldView.getCell(false, cell.x, cell.y);
                    if (!closed.ContainsKey(cell.pos) && !cell.blocked && waterCell.blocked) {
                        float g = current.g + Vector3.Distance(cell.pos, current.pos);
                        if (g < cell.g) { //New and better G value?
                            cell.h = Vector3.Distance(cell.pos, goal.pos);
                            cell.g = g;
                            cell.parent = current;
                            open.Add(cell.f, cell);
                        }
                    }
                }
            }
        }
    }



    //private bool canSeePlayer(NPCWorldView.GameCharacter npc, NPCWorldView.GameCharacter player) {
    //    float a = angle(npc.getDir(), player.getPos() - npc.getPos());
    //    if (a < 90) { // In field of view?
    //        if (NPCWorldView.rayCast(true, npc.getPos(), player.getPos()) == float.MaxValue) { // in line of sight?
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //private Vector3 avoidPlayer(NPCWorldView.GameCharacter npc, NPCWorldView.GameCharacter player) {
    //    Vector3 flee = npc.getPos() - player.getPos();
    //    flee.y = 0;
    //    return turnTowards(npc.getDir(), flee);
    //}

    //private NPCWorldView.GameCharacter closestPlayer(NPCWorldView.GameCharacter npc) {
    //    var players = NPCWorldView.getPlayers();
    //    NPCWorldView.GameCharacter closestPlayer = null;
    //    float closestDist = float.MaxValue;
    //    foreach (var player in players.Values) {
    //        float dist = Vector3.Distance(npc.getPos(), player.getPos());
    //        if (dist < closestDist) {
    //            closestDist = dist;
    //            closestPlayer = player;
    //        }
    //    }
    //    if (closestDist < 15) return closestPlayer;
    //    else return null;
    //}

    //private Vector3 fleeFireWall(NPCWorldView.GameCharacter npc) {
    //    float dist = Vector3.Distance(npc.getPos(), NPCWorldView.FireWall.pos);
    //    float viewDist = 20;
    //    if ((NPCWorldView.FireWall.radius - dist) < viewDist) {
    //        Vector3 fleeDir = NPCWorldView.FireWall.pos - npc.getPos();
    //        fleeDir.y = 0;
    //        return turnTowards(npc.getDir(), fleeDir.normalized);
    //    } else
    //        return Vector3.zero;
    //}
}
